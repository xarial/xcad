using System;
using System.Threading;
using Xarial.XCad.Annotations;
using Xarial.XCad.Toolkit.Exceptions;

namespace Xarial.XCad.SwDocumentManager.Annotations
{
    internal class SwDmTableColumn : IXTableColumn
    {
        public int Index { get => m_Index; set => throw new NotImplementedException(); }
        public bool Visible { get => true; set => throw new NotImplementedException(); }
        public string Title { get => m_Title; set => throw new NotImplementedException(); }

        public bool IsCommitted => true;

        public void Commit(CancellationToken cancellationToken) => throw new ElementAlreadyCommittedException();

        private readonly int m_Index;
        private readonly string m_Title;

        internal SwDmTableColumn(int index, string title) 
        {
            m_Index = index;
            m_Title = title;
        }
    }
}
