//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.SwDocumentManager.Documents;

namespace Xarial.XCad.SwDocumentManager
{
    public interface ISwDmSelObject : ISwDmObject, IXSelObject
    {

    }

    internal class SwDmSelObject : SwDmObject, ISwDmSelObject
    {
        // #region Not Supported

        public virtual void Commit(CancellationToken cancellationToken) => throw new NotSupportedException();
        public void Delete() => throw new NotSupportedException();
        public void Select(bool append) => throw new NotSupportedException();

        // #endregion

        public SwDmSelObject(object disp, SwDmApplication ownerApp, SwDmDocument ownerDoc) : base(disp, ownerApp, ownerDoc)
        {
        }

        public bool IsSelected => throw new NotSupportedException();
        public virtual bool IsCommitted => true;
    }
}
