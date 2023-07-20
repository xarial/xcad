//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Linq;
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.Utils.Diagnostics;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwDrawing : ISwDocument, IXDrawing 
    {
        IDrawingDoc Drawing { get; }
    }

    public interface ISwDrawingOptions : ISwDocumentOptions, IXDrawingOptions 
    {
    }

    public interface ISwDrawingDetailingOptions : IXDrawingDetailingOptions 
    {
    }

    internal class SwDrawingDetailingOptions : ISwDrawingDetailingOptions 
    {
        private readonly SwDrawing m_Draw;

        internal SwDrawingDetailingOptions(SwDrawing draw) 
        {
            m_Draw = draw;
        }

        public bool DisplayCosmeticThreads 
        {
            get => m_Draw.GetUserPreferenceToggle(swUserPreferenceToggle_e.swDisplayCosmeticThreads);
            set => m_Draw.SetUserPreferenceToggle(swUserPreferenceToggle_e.swDisplayCosmeticThreads, value);
        }

        public bool AutoInsertCenterMarksForSlots
        {
            get => m_Draw.GetUserPreferenceToggle(swUserPreferenceToggle_e.swDetailingAutoInsertCenterMarksForSlots);
            set => m_Draw.SetUserPreferenceToggle(swUserPreferenceToggle_e.swDetailingAutoInsertCenterMarksForSlots, value);
        }

        public bool AutoInsertCenterMarksForFillets
        {
            get => m_Draw.GetUserPreferenceToggle(swUserPreferenceToggle_e.swDetailingAutoInsertCenterMarksForFillets);
            set => m_Draw.SetUserPreferenceToggle(swUserPreferenceToggle_e.swDetailingAutoInsertCenterMarksForFillets, value);
        }

        public bool AutoInsertCenterMarksForHoles
        {
            get => m_Draw.GetUserPreferenceToggle(swUserPreferenceToggle_e.swDetailingAutoInsertCenterMarksForHoles);
            set => m_Draw.SetUserPreferenceToggle(swUserPreferenceToggle_e.swDetailingAutoInsertCenterMarksForHoles, value);
        }

        public bool AutoInsertDowelSymbols
        {
            get => m_Draw.GetUserPreferenceToggle(swUserPreferenceToggle_e.swDetailingAutoInsertDowelSymbols);
            set => m_Draw.SetUserPreferenceToggle(swUserPreferenceToggle_e.swDetailingAutoInsertDowelSymbols, value);
        }
    }

    internal class SwDrawingOptions : SwDocumentOptions, ISwDrawingOptions 
    {
        internal SwDrawingOptions(SwDrawing draw) : base(draw) 
        {
            Detailing = new SwDrawingDetailingOptions(draw);
        }

        public IXDrawingDetailingOptions Detailing { get; }
    }

    internal class SwDrawing : SwDocument, ISwDrawing
    {
        IXDrawingOptions IXDrawing.Options => m_Options;

        public IDrawingDoc Drawing => Model as IDrawingDoc;

        public IXSheetRepository Sheets => m_SheetsLazy.Value;

        internal protected override swDocumentTypes_e? DocumentType => swDocumentTypes_e.swDocDRAWING;

        private readonly Lazy<SwSheetCollection> m_SheetsLazy;

        protected override bool IsLightweightMode => Sheets.Any(s => s.DrawingViews.Any(v => ((ISwDrawingView)v).DrawingView.IsLightweight()));

        protected override bool IsRapidMode 
        {
            get 
            {
                if (OwnerApplication.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2020))
                {
                    return Drawing.IsDetailingMode();
                }
                else 
                {
                    return false;
                }
            }
        }

        public override IXDocumentOptions Options => m_Options;

        public IXLayerRepository Layers { get; }

        private SwDrawingOptions m_Options;

        internal SwDrawing(IDrawingDoc drawing, SwApplication app, IXLogger logger, bool isCreated)
            : base((IModelDoc2)drawing, app, logger, isCreated)
        {
            m_SheetsLazy = new Lazy<SwSheetCollection>(() => new SwSheetCollection(this, OwnerApplication));
            m_Options = new SwDrawingOptions(this);
            Layers = new SwLayersCollection(this, app);
        }

        protected override void CommitCache(IModelDoc2 model, CancellationToken cancellationToken)
        {
            base.CommitCache(model, cancellationToken);

            if (m_SheetsLazy.IsValueCreated) 
            {
                m_SheetsLazy.Value.CommitCache(cancellationToken);
            }
        }

        protected override bool IsDocumentTypeCompatible(swDocumentTypes_e docType) => docType == swDocumentTypes_e.swDocDRAWING;

        protected override SwAnnotationCollection CreateAnnotations() => new SwDrawingAnnotationCollection(this);

        IXDrawingSaveOperation IXDrawing.PreCreateSaveAsOperation(string filePath)
        {
            var ext = System.IO.Path.GetExtension(filePath);

            switch (ext.ToLower())
            {
                case ".pdf":
                    return new SwDrawingPdfSaveOperation(this, filePath);

                case ".dxf":
                case ".dwg":
                    return new SwDxfDwgSaveOperation(this, filePath);

                default:
                    return new SwDrawingSaveOperation(this, filePath);
            }
        }

        protected override void GetPaperSize(out swDwgPaperSizes_e size, out double width, out double height)
        {
            if (m_SheetsLazy.IsValueCreated)
            {
                PaperSizeHelper.ParsePaperSize(Sheets.First().PaperSize, out size, out _, out width, out height);
            }
            else
            {
                PaperSizeHelper.ParsePaperSize(null, out size, out _, out width, out height);
            }
        }

        public override IXSaveOperation PreCreateSaveAsOperation(string filePath) => ((IXDrawing)this).PreCreateSaveAsOperation(filePath);
    }
}