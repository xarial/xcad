using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Geometry;

namespace Xarial.XCad.SwDocumentManager.Documents
{
    public interface ISwDmPart : ISwDmDocument3D, IXPart
    {
    }

    internal class SwDmPart : SwDmDocument3D, ISwDmPart
    {
        #region Not Supported
        
        public event CutListRebuildDelegate CutListRebuild;

        #endregion

        public SwDmPart(ISwDmApplication dmApp, ISwDMDocument doc, bool isCreated,
            Action<ISwDmDocument> createHandler, Action<ISwDmDocument> closeHandler)
            : base(dmApp, doc, isCreated, createHandler, closeHandler)
        {
        }

        public IXBodyRepository Bodies => throw new NotImplementedException();
    }
}
