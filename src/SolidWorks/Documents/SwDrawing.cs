//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using Xarial.XCad.Documents;
using Xarial.XCad.Utils.Diagnostics;

namespace Xarial.XCad.SolidWorks.Documents
{
    public class SwDrawing : SwDocument, IXDrawing
    {
        public IDrawingDoc Drawing { get; }

        internal SwDrawing(IDrawingDoc drawing, ISldWorks app, ILogger logger)
            : base((IModelDoc2)drawing, app, logger)
        {
            Drawing = drawing;
        }
    }
}