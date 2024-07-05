//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Media;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.Features;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Documents.Exceptions;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Sketch;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Graphics;
using Xarial.XCad.UI;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwSheet : ISwSelObject, IXSheet
    {
        ISheet Sheet { get; }
    }

    [DebuggerDisplay("{" + nameof(Name) + "}")]
    internal class SwSheet : SwSelObject, ISwSheet
    {
        IXAnnotationRepository IXSheet.Annotations => Annotations;

        private readonly SwDrawing m_Drawing;

        public ISheet Sheet => m_Creator.Element;

        public string Name
        {
            get
            {
                if (IsCommitted)
                {
                    return Sheet.GetName();
                }
                else
                {
                    return m_Creator.CachedProperties.Get<string>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    Sheet.SetName(value);

                    if (!string.Equals(Sheet.GetName(), value, StringComparison.CurrentCultureIgnoreCase))
                    {
                        throw new Exception($"Failed to rename sheet to '{value}'");
                    }
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public IXDrawingViewRepository DrawingViews => m_DrawingViews;

        public override bool IsCommitted => m_Creator.IsCreated;

        public override object Dispatch => Sheet;

        public IXImage Preview
        {
            get 
            {
                if (OwnerApplication.IsInProcess())
                {
                    return PictureDispUtils.PictureDispToXImage(OwnerApplication.Sw.GetPreviewBitmap(m_Drawing.Path, Name));
                }
                else 
                {
                    return new XDrawingImage(m_Drawing.GetThumbnailImage());
                }
            }
        }

        public Scale Scale
        {
            get
            {
                if (IsCommitted)
                {
                    double[] sheetPrps;

                    if (m_Drawing.OwnerApplication.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2016))
                    {
                        sheetPrps = (double[])Sheet.GetProperties2();
                    }
                    else
                    {
                        sheetPrps = (double[])Sheet.GetProperties();
                    }

                    return new Scale(sheetPrps[2], sheetPrps[3]);
                }
                else
                {
                    return m_Creator.CachedProperties.Get<Scale>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    if (!Sheet.SetScale(value.Numerator, value.Denominator, false, false))
                    {
                        throw new Exception("Failed to change the scale of the sheet");
                    }
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public PaperSize PaperSize 
        {
            get 
            {
                if (IsCommitted)
                {
                    var width = 0d;
                    var height = 0d;

                    var paperSize = Sheet.GetSize(ref width, ref height);

                    var standardPaperSize = paperSize == (int)swDwgPaperSizes_e.swDwgPapersUserDefined ? default(StandardPaperSize_e?) : (StandardPaperSize_e)paperSize;

                    return new PaperSize(standardPaperSize, width, height);
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<PaperSize>();
                }
            }
            set 
            {
                if (IsCommitted)
                {
                    //NOTE: ISheet::SetSize does not work correctly and removes the template and breaks drawing
                    SetupSheet(Sheet, Name, value, Scale, Format.Template, ViewsProjectionType);
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public ViewsProjectionType_e ViewsProjectionType
        {
            get
            {
                if (IsCommitted)
                {
                    double[] sheetPrps;

                    if (OwnerApplication.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2016))
                    {
                        sheetPrps = (double[])Sheet.GetProperties();
                    }
                    else 
                    {
                        sheetPrps = (double[])Sheet.GetProperties2();
                    }

                    var isFirstAngle = Convert.ToBoolean(sheetPrps[4]);

                    if (isFirstAngle)
                    {
                        return ViewsProjectionType_e.FirstAngle;
                    }
                    else 
                    {
                        return ViewsProjectionType_e.ThirdAngle;
                    }
                }
                else
                {
                    return m_Creator.CachedProperties.Get<ViewsProjectionType_e>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    SetupSheet(Sheet, Name, PaperSize, Scale, Format.Template, value);
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public IXSketch2D Sketch => new SwSheetSketch(this, SheetView.DrawingView.IGetSketch(), m_Drawing, OwnerApplication, false);

        public IXSheetFormat Format { get; }

        internal SwDrawingView SheetView 
        {
            get
            {
                foreach (object[] sheet in m_Drawing.Drawing.GetViews() as object[])
                {
                    var sheetView = (IView)sheet.First();

                    if (string.Equals(sheetView.Name, Sheet.GetName(), StringComparison.CurrentCultureIgnoreCase))
                    {
                        return m_Drawing.CreateObjectFromDispatch<SwDrawingView>(sheetView);
                    }
                }

                throw new Exception("Failed to find the view of the sheet");
            }
        }

        internal SwSheetAnnotationCollection Annotations { get; }

        private readonly ElementCreator<ISheet> m_Creator;

        private readonly SwDrawingViewsCollection m_DrawingViews;

        internal void InitFromExisting(SwSheet swSheet, CancellationToken cancellationToken)
            => m_Creator.Init(swSheet.Sheet, cancellationToken);

        internal void SetFromExisting(SwSheet swSheet)
            => m_Creator.Set(swSheet.Sheet);

        internal SwSheet(ISheet sheet, SwDrawing draw, SwApplication app) : base(sheet, draw, app)
        {
            m_Drawing = draw;

            m_DrawingViews = new SwDrawingViewsCollection(draw, this);
            m_Creator = new ElementCreator<ISheet>(CreateSheet, CommitCache, sheet, sheet != null);
            Annotations = new SwSheetAnnotationCollection(this);

            Format = new SwSheetFormat(this);
        }

        private ISheet CreateSheet(CancellationToken arg)
        {
            PaperSizeHelper.ParsePaperSize(PaperSize, out var paperSize, out var template, out double paperWidth, out double paperHeight);

            var scale = Scale ?? new Scale(1, 1);

            var angle = ViewsProjectionType == ViewsProjectionType_e.FirstAngle;

            if (!string.IsNullOrEmpty(Format.Template)) 
            {
                template = swDwgTemplates_e.swDwgTemplateCustom;
            }

            bool? useDiffSheetFormatForNewSheets = null;
            bool? showSheetFormatDlgForNewSheets = null;

            try
            {
                //NOTE: if this option is set dialog box is displayed to select sheet format
                if (ChangeToggleIfNeeded(swUserPreferenceToggle_e.swDrawingSheetsUseDifferentSheetFormat, false, false))
                {
                    useDiffSheetFormatForNewSheets = true;
                }

                //NOTE: if this option is set document requires rebuild and sheet format template is not visible until reload
                if (ChangeToggleIfNeeded(swUserPreferenceToggle_e.swDrawingShowSheetFormatDialog, false, true)) 
                {
                    showSheetFormatDlgForNewSheets = true;
                }

                if (OwnerApplication.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2015))
                {
                    if (!m_Drawing.Drawing.NewSheet4(Name, (int)paperSize, (int)template, scale.Numerator, scale.Denominator, angle,
                        Format.Template, paperWidth, paperHeight, "", 0, 0, 0, 0, 0, 0))
                    {
                        throw new Exception("Failed to create new sheet");
                    }
                }
                else
                {
                    if (!m_Drawing.Drawing.NewSheet3(Name, (int)paperSize, (int)template, scale.Numerator, scale.Denominator, angle,
                        Format.Template, paperWidth, paperHeight, ""))
                    {
                        throw new Exception("Failed to create new sheet");
                    }
                }
            }
            finally 
            {
                try
                {
                    if (useDiffSheetFormatForNewSheets.HasValue)
                    {
                        ChangeToggle(swUserPreferenceToggle_e.swDrawingSheetsUseDifferentSheetFormat, useDiffSheetFormatForNewSheets.Value, false);
                    }
                }
                finally
                {
                    if (showSheetFormatDlgForNewSheets.HasValue)
                    {
                        ChangeToggle(swUserPreferenceToggle_e.swDrawingShowSheetFormatDialog, showSheetFormatDlgForNewSheets.Value, true);
                    }
                }
            }

            return m_Drawing.Drawing.Sheet[Name];
        }

        internal void SetupSheet(IXSheet template)
            => SetupSheet(Sheet, template.Name, template.PaperSize, template.Scale, template.Format.Template, template.ViewsProjectionType);

        internal void SetupSheet(ISheet sheet, string name, PaperSize size, Scale scale, string templateName, ViewsProjectionType_e prjType)
        {
            PaperSizeHelper.ParsePaperSize(size, out var paperSize, out var paperTemplate, out var paperWidth, out var paperHeight);

            scale = scale ?? new Scale(1, 1);

            if (!string.IsNullOrEmpty(name))
            {
                sheet.SetName(name);

                if (!string.Equals(sheet.GetName(), name, StringComparison.CurrentCultureIgnoreCase)) 
                {
                    throw new Exception("Failed to change sheet name");
                }
            }

            var angle = prjType == ViewsProjectionType_e.FirstAngle;

            var custPrpView = sheet.CustomPropertyView;

            if (!string.IsNullOrEmpty(templateName)) 
            {
                paperTemplate = swDwgTemplates_e.swDwgTemplateCustom;
            }

            if (OwnerApplication.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2015))
            {
                if (!m_Drawing.Drawing.SetupSheet6(sheet.GetName(), (int)paperSize, (int)paperTemplate, scale.Numerator, scale.Denominator, angle,
                    templateName, paperWidth, paperHeight, custPrpView, true, 0, 0, 0, 0, 0, 0))
                {
                    throw new Exception("Failed to setup sheet");
                }
            }
            else
            {
                if (!m_Drawing.Drawing.SetupSheet5(sheet.GetName(), (int)paperSize, (int)paperTemplate, scale.Numerator, scale.Denominator, angle,
                    templateName, paperWidth, paperHeight, custPrpView, true))
                {
                    throw new Exception("Failed to setup sheet");
                }
            }
        }

        internal override void Select(bool append, ISelectData selData)
        {
            int mark;
            Callout callout;
            double x, y, z;

            if (selData != null)
            {
                mark = selData.Mark;
                callout = selData.Callout;
                x = selData.X;
                y = selData.Y;
                z = selData.Z;
            }
            else
            {
                mark = 0;
                callout = null;
                x = 0;
                y = 0;
                z = 0;
            }

            if (!m_Drawing.Model.Extension.SelectByID2(Name, "SHEET", x, y, z, append, mark,
                callout, (int)swSelectOption_e.swSelectOptionDefault))
            {
                throw new Exception($"Failed to select sheet");
            }
        }

        public override void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);

        private void CommitCache(ISheet sheet, CancellationToken cancellationToken) => m_DrawingViews.CommitCache(cancellationToken);

        public IXSheet Clone(IXDrawing targetDrawing)
        {
            Select(false);

            if (OwnerDocument.Selections.Count == 1 && OwnerDocument.Selections.First().Equals(this))
            {
                m_Drawing.Model.EditCopy();

                var curSheets = targetDrawing.Sheets.ToArray();

                PasteSheet(targetDrawing);

                var newSheet = targetDrawing.Sheets.Last();

                if (!curSheets.Contains(newSheet, new XObjectEqualityComparer<IXSheet>()))
                {
                    return newSheet;
                }
                else
                {
                    throw new Exception("Failed to get the cloned sheet");
                }
            }
            else 
            {
                throw new Exception("Failed to select the sheet for cloning");
            }
        }

        private void PasteSheet(IXDrawing targetDrawing)
        {
            const int MAX_ATTEMPTS = 3;

            var curSheetsCount = targetDrawing.Sheets.Count;

            for (int i = 0; i < MAX_ATTEMPTS; i++) 
            {
                if (((ISwDrawing)targetDrawing).Drawing.PasteSheet(
                    (int)swInsertOptions_e.swInsertOption_MoveToEnd,
                    (int)swRenameOptions_e.swRenameOption_Yes))
                {
                    if (targetDrawing.Sheets.Count == curSheetsCount + 1)
                    {
                        return;
                    }
                    else 
                    {
                        throw new Exception($"Paste sheet has succeeded, but number of sheets has not changed");
                    }
                }
                else 
                {
                    //NOTE: it was observed that in some cases paste command fails on the first attempt
                    if (targetDrawing.Sheets.Count != curSheetsCount)
                    {
                        throw new Exception($"Paste sheet has failed, but number of sheets has changed");
                    }
                }
            }

            throw new Exception($"Failed to paste sheet");
        }

        private bool ChangeToggleIfNeeded(swUserPreferenceToggle_e toggle, bool value, bool sysOpt)
        {
            bool val;

            if (sysOpt)
            {
                val = OwnerApplication.Sw.GetUserPreferenceToggle((int)toggle);
            }
            else 
            {
                val = OwnerModelDoc.Extension.GetUserPreferenceToggle(
                    (int)toggle, (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified);
            }

            if (val != value)
            {
                ChangeToggle(toggle, value, sysOpt);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ChangeToggle(swUserPreferenceToggle_e toggle, bool value, bool sysOpt)
        {
            bool res;

            if (sysOpt)
            {
                OwnerApplication.Sw.SetUserPreferenceToggle((int)toggle, value);
                res = OwnerApplication.Sw.GetUserPreferenceToggle((int)toggle) == value;
            }
            else
            {
                res = OwnerModelDoc.Extension.SetUserPreferenceToggle(
                    (int)toggle,
                    (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, value);
            }

            if (!res)
            {
                throw new Exception($"Failed to set the '{toggle}' option to '{value}'");
            }
        }
    }

    internal class SwSheetSketchEditor : SheetActivator, IEditor<SwSheetSketch>
    {
        private readonly SwSheetSketch m_SheetSketch;

        private readonly bool? m_AddToDbOrig;

        private readonly ISketchManager m_SketchMgr;

        private readonly IDrawingDoc m_Drw;

        private readonly bool m_SheetFormat;

        private readonly bool m_OrigSheetFormat;

        internal SwSheetSketchEditor(SwSheetSketch sheetSketch, SwSheet sheet, bool sheetFormat) : base(sheet)
        {
            m_SheetSketch = sheetSketch;

            m_SheetFormat = sheetFormat;

            m_Drw = ((ISwDrawing)sheet.OwnerDocument).Drawing;

            m_OrigSheetFormat = !m_Drw.GetEditSheet();

            m_SketchMgr = ((IModelDoc2)m_Drw).SketchManager;

            m_Drw.ActivateView("");
            
            SetEditTarget(m_SheetFormat);

            sheet.OwnerDocument.Selections.Clear();

            if (!m_SketchMgr.AddToDB)
            {
                m_AddToDbOrig = m_SketchMgr.AddToDB;
                m_SketchMgr.AddToDB = true;
            }
        }

        public SwSheetSketch Target => m_SheetSketch;

        public bool Cancel { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        private void SetEditTarget(bool sheetFormat)
        {
            if (m_Drw.GetEditSheet() != !sheetFormat)
            {
                if (sheetFormat)
                {
                    m_Drw.EditTemplate();
                }
                else
                {
                    m_Drw.EditSheet();
                }
            }
        }

        public override void Dispose()
        {
            if (m_AddToDbOrig.HasValue)
            {
                m_SketchMgr.AddToDB = m_AddToDbOrig.Value;
            }

            SetEditTarget(m_OrigSheetFormat);

            base.Dispose();
        }
    }

    internal class SwSheetSketch : SwSketch2D
    {
        private readonly SwSheet m_Sheet;
        private readonly SwDrawing m_Draw;

        private readonly bool m_SheetFormat;

        internal SwSheetSketch(SwSheet sheet, ISketch sketch, SwDrawing drw, SwApplication app, bool sheetFormat) : base(sketch, drw, app, true)
        {
            m_Draw = drw;
            m_Sheet = sheet;
            m_SheetFormat = sheetFormat;
        }

        protected internal override bool IsEditing => m_Draw.Sheets.Active.Equals(m_Sheet);

        protected internal override IEditor<IXSketchBase> CreateSketchEditor(ISketch sketch) => new SwSheetSketchEditor(this, m_Sheet, m_SheetFormat);
    }

    internal class SwSheetFormat : SwObject, IXSheetFormat
    {
        private const string CUSTOM_LAYOUT_TEMPLATE = "*.drt";

        private readonly SwSheet m_Sheet;

        private string m_CachedTemplate;

        internal SwSheetFormat(SwSheet sheet) : base(null, sheet.OwnerDocument, sheet.OwnerApplication)
        {
            m_Sheet = sheet;
        }

        public string Template
        {
            get
            {
                if (m_Sheet.IsCommitted)
                {
                    var templateName = m_Sheet.Sheet.GetTemplateName();

                    if (string.Equals(templateName, CUSTOM_LAYOUT_TEMPLATE, StringComparison.CurrentCultureIgnoreCase))
                    {
                        templateName = "";
                    }

                    return templateName;
                }
                else
                {
                    return m_CachedTemplate;
                }
            }
            set
            {
                if (m_Sheet.IsCommitted)
                {
                    var sheetFormatVisible = m_Sheet.Sheet.SheetFormatVisible;

                    m_Sheet.SetupSheet(m_Sheet.Sheet, m_Sheet.Name, m_Sheet.PaperSize, m_Sheet.Scale, value, m_Sheet.ViewsProjectionType);

                    var res = m_Sheet.Sheet.ReloadTemplate(false);

                    if (res == (int)swReloadTemplateResult_e.swReloadTemplate_Success)
                    {
                        //NOTE: in some cases sheet format visibility is reset after changing the sheet format
                        m_Sheet.Sheet.SheetFormatVisible = sheetFormatVisible;
                    }
                    else
                    {
                        throw new Exception($"Failed to reload template: {res}");
                    }
                }
                else
                {
                    m_CachedTemplate = value;
                }
            }
        }

        public IXSketch2D Sketch => new SwSheetSketch(m_Sheet, 
            m_Sheet.SheetView.DrawingView.IGetSketch(),
            (SwDrawing)m_Sheet.OwnerDocument, m_Sheet.OwnerApplication, true);
    }

    internal static class PaperSizeHelper 
    {
        internal static void ParsePaperSize(PaperSize paperSize, out swDwgPaperSizes_e dwgPaperSize, out swDwgTemplates_e dwgTemplate, out double dwpPaperWidth, out double dwpPaperHeight) 
        {
            if (paperSize == null) 
            {
                paperSize = new PaperSize(0.1, 0.1);
            }

            dwgPaperSize = paperSize.StandardPaperSize.HasValue ? (swDwgPaperSizes_e)paperSize.StandardPaperSize.Value : swDwgPaperSizes_e.swDwgPapersUserDefined;
            dwgTemplate = paperSize.StandardPaperSize.HasValue ? (swDwgTemplates_e)paperSize.StandardPaperSize.Value : swDwgTemplates_e.swDwgTemplateNone;
            dwpPaperWidth = !paperSize.StandardPaperSize.HasValue ? paperSize.Width : 0;
            dwpPaperHeight = !paperSize.StandardPaperSize.HasValue ? paperSize.Height : 0;
        }
    }

    internal class UncommittedPreviewOnlySheet : ISwSelObject, ISwSheet 
    {
        ISwApplication ISwObject.OwnerApplication => m_App;
        ISwDocument ISwObject.OwnerDocument => m_Drw;

        #region Not Supported
        public string Name { get => throw new UnloadedDocumentPreviewOnlySheetException(); set => throw new UnloadedDocumentPreviewOnlySheetException(); }
        public IXDrawingViewRepository DrawingViews => throw new UnloadedDocumentPreviewOnlySheetException();
        public void Commit(CancellationToken cancellationToken)
            => throw new UnloadedDocumentPreviewOnlySheetException();
        public void Select(bool append)
            => throw new UnloadedDocumentPreviewOnlySheetException();
        public void Serialize(Stream stream)
            => throw new UnloadedDocumentPreviewOnlySheetException();
        public Scale Scale { get => throw new UnloadedDocumentPreviewOnlySheetException(); set => throw new UnloadedDocumentPreviewOnlySheetException(); }
        public ISheet Sheet => throw new UnloadedDocumentPreviewOnlySheetException();
        public object Dispatch => throw new UnloadedDocumentPreviewOnlySheetException();
        public bool IsSelected => throw new UnloadedDocumentPreviewOnlySheetException();
        public bool IsAlive => throw new UnloadedDocumentPreviewOnlySheetException();
        public ITagsManager Tags => throw new UnloadedDocumentPreviewOnlySheetException();
        public PaperSize PaperSize { get => throw new UnloadedDocumentPreviewOnlySheetException(); set => throw new UnloadedDocumentPreviewOnlySheetException(); }
        public string Template { get => throw new UnloadedDocumentPreviewOnlySheetException(); set => throw new UnloadedDocumentPreviewOnlySheetException(); }
        public ViewsProjectionType_e ViewsProjectionType { get => throw new UnloadedDocumentPreviewOnlySheetException(); set => throw new UnloadedDocumentPreviewOnlySheetException(); }
        public IXSheet Clone(IXDrawing targetDrawing) => throw new NotSupportedException();
        public IXSketch2D Sketch => throw new NotSupportedException();
        public IXSheetFormat Format => throw new NotSupportedException();
        public void Delete() => throw new UnloadedDocumentPreviewOnlySheetException();
        public IXAnnotationRepository Annotations => throw new UnloadedDocumentPreviewOnlySheetException();
        #endregion

        private readonly ISwApplication m_App;
        private readonly SwDrawing m_Drw;

        internal UncommittedPreviewOnlySheet(SwDrawing drw, ISwApplication app) 
        {
            m_Drw = drw;
            m_App = app;
        }

        public IXImage Preview
            => PictureDispUtils.PictureDispToXImage(m_App.Sw.GetPreviewBitmap(m_Drw.Path, ""));

        public bool IsCommitted => false;

        public IXApplication OwnerApplication => m_App;
        public IXDocument OwnerDocument => m_Drw;

        public bool Equals(IXObject other) => this == other;
    }
}
