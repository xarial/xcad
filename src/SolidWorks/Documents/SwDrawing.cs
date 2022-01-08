//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Linq;
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

        private readonly Lazy<IXSheetRepository> m_SheetsLazy;

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

        internal SwDrawing(IDrawingDoc drawing, ISwApplication app, IXLogger logger, bool isCreated)
            : base((IModelDoc2)drawing, app, logger, isCreated)
        {
            m_SheetsLazy = new Lazy<IXSheetRepository>(() => new SwSheetCollection(this, OwnerApplication));
        }
    }
}