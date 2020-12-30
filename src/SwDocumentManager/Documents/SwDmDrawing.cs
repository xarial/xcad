//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.SwDocumentManager.Documents
{
    public interface ISwDmDrawing : ISwDmDocument, IXDrawing
    {
    }

    internal class SwDmDrawing : SwDmDocument, ISwDmDrawing
    {
        public SwDmDrawing(ISwDmApplication dmApp, ISwDMDocument doc, bool isCreated,
            Action<ISwDmDocument> createHandler, Action<ISwDmDocument> closeHandler)
            : base(dmApp, doc, isCreated, createHandler, closeHandler)
        {
        }

        public IXSheetRepository Sheets => throw new NotImplementedException();
    }
}
