//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Documents.Exceptions;
using Xarial.XCad.SolidWorks.Utils;
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
        public ISheet Sheet { get; }
        private readonly SwDrawing m_Drawing;

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
            => PictureDispUtils.PictureDispToXImage(OwnerApplication.Sw.GetPreviewBitmap(m_Drawing.Path, Name));

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
                    SetupSheet(Sheet, Name, value, Scale);
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        private readonly ElementCreator<ISheet> m_Creator;

        private readonly SwDrawingViewsCollection m_DrawingViews;


        internal SwSheet(ISheet sheet, SwDrawing draw, SwApplication app) : base(sheet, draw, app)
        {
            m_Drawing = draw;
            Sheet = sheet;
            m_DrawingViews = new SwDrawingViewsCollection(draw, this);
            m_Creator = new ElementCreator<ISheet>(CreateSheet, CommitCache, sheet, sheet != null);
        }

        private ISheet CreateSheet(CancellationToken arg)
        {
            PaperSizeHelper.ParsePaperSize(PaperSize, out var paperSize, out var template, out double paperWidth, out double paperHeight);

            var scale = Scale ?? new Scale(1, 1);

            if (OwnerApplication.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2015))
            {
                if (!m_Drawing.Drawing.NewSheet4(Name, (int)paperSize, (int)template, scale.Numerator, scale.Denominator, true,
                    "", paperWidth, paperHeight, "", 0, 0, 0, 0, 0, 0))
                {
                    throw new Exception("Failed to create new sheet");
                }
            }
            else 
            {
                if (!m_Drawing.Drawing.NewSheet3(Name, (int)paperSize, (int)paperSize, scale.Numerator, scale.Denominator, true,
                    "", paperWidth, paperHeight, ""))
                {
                    throw new Exception("Failed to create new sheet");
                }
            }

            return m_Drawing.Drawing.Sheet[Name];
        }

        internal void SetupSheet(IXSheet template)
            => SetupSheet(Sheet, template.Name, template.PaperSize, template.Scale);

        internal void SetupSheet(ISheet sheet, string name, PaperSize size, Scale scale)
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

            if (OwnerApplication.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2015))
            {
                if (!m_Drawing.Drawing.SetupSheet6(name, (int)paperSize, (int)paperTemplate, scale.Numerator, scale.Denominator, true,
                    "", paperWidth, paperHeight, "", true, 0, 0, 0, 0, 0, 0))
                {
                    throw new Exception("Failed to setup sheet");
                }
            }
            else
            {
                if (!m_Drawing.Drawing.SetupSheet5(name, (int)paperSize, (int)paperSize, scale.Numerator, scale.Denominator, true,
                    "", paperWidth, paperHeight, "", true))
                {
                    throw new Exception("Failed to setup sheet");
                }
            }
        }

        public override void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);

        private void CommitCache(ISheet sheet, CancellationToken cancellationToken)
            => m_DrawingViews.CommitCache(cancellationToken);
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
            dwpPaperWidth = paperSize.Width.HasValue ? paperSize.Width.Value : 0;
            dwpPaperHeight = paperSize.Height.HasValue ? paperSize.Height.Value : 0;
        }
    }

    internal class UncommittedPreviewOnlySheet : ISwSelObject, ISwSheet 
    {
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

        public SelectType_e Type => SelectType_e.Sheets;

        public bool Equals(IXObject other) => this == other;
    }

    /// <summary>
    /// This is a placeholder for a sheet(s) which already present in the template
    /// </summary>
    /// <remarks>Drawing will always contain at least one sheet so this is returned if no user sheets are found</remarks>
    internal class SwTemplatePlaceholderSheet : SwSheet
    {
        internal SwTemplatePlaceholderSheet(SwDrawing draw, SwApplication app) : base(null, draw, app)
        {
        }

        public override void Commit(CancellationToken cancellationToken)
            => throw new NotSupportedException("Placeholder sheet cannot be committed");
    }
}
