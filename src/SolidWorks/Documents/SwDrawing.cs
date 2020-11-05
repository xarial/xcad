//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
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

        public IXSheetRepository Sheets { get; }

        internal SwDrawing(IDrawingDoc drawing, ISwApplication app, IXLogger logger, bool isCreated)
            : base((IModelDoc2)drawing, app, logger, isCreated)
        {
            Sheets = new SwSheetCollection(this);
        }

        protected override swUserPreferenceStringValue_e DefaultTemplate => swUserPreferenceStringValue_e.swDefaultTemplateDrawing;
    }
}