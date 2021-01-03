using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Xarial.XCad.SwDocumentManager
{
    public interface ISwDmSelObject : IXSelObject 
    {
    
    }

    internal class SwDmSelObject : SwDmObject, ISwDmSelObject
    {
        public SwDmSelObject(object disp) : base(disp)
        {
        }

        public bool IsSelected => throw new NotSupportedException();
        public bool IsCommitted => throw new NotSupportedException();
        public void Commit(CancellationToken cancellationToken) => throw new NotSupportedException();
        public void Select(bool append) => throw new NotSupportedException();
    }
}
