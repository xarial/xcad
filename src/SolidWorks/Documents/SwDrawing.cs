//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
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
using Xarial.XCad.Utils.Diagnostics;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwDrawing : ISwDocument, IXDrawing 
    {
        IDrawingDoc Drawing { get; }
    }

    internal class SwDrawing : SwDocument, ISwDrawing
    {
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

        internal SwDrawing(IDrawingDoc drawing, SwApplication app, IXLogger logger, bool isCreated)
            : base((IModelDoc2)drawing, app, logger, isCreated)
        {
            m_SheetsLazy = new Lazy<SwSheetCollection>(() => new SwSheetCollection(this, OwnerApplication));
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
    }
}