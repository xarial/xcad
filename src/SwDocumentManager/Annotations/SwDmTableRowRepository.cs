using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SwDocumentManager.Annotations
{
    internal class SwDmTableRowRepository : IXTableRowRepository
    {
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IXTableRow this[int index]
        {
            get 
            {
                if (index < Count)
                {
                    return CreateRow(index);
                }
                else 
                {
                    throw new IndexOutOfRangeException();
                }
            }
        }

        public IXTableRow this[string name] => m_RepoHelper.Get(name);

        public int Count => m_Table.Table.GetRowCount() - (m_Table.GetHeaderRowIndex() == -1 ? 0 : 1);

        private readonly SwDmTable m_Table;
        private readonly RepositoryHelper<IXTableRow> m_RepoHelper;

        internal SwDmTableRowRepository(SwDmTable table)
        {
            m_Table = table;
            m_RepoHelper = new RepositoryHelper<IXTableRow>(this);
        }

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) 
            => m_RepoHelper.FilterDefault(this, filters, reverseOrder);

        public IEnumerator<IXTableRow> GetEnumerator() 
            => IterateRows<SwDmTableRow>().GetEnumerator();

        public void AddRange(IEnumerable<IXTableRow> ents, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public T PreCreate<T>() where T : IXTableRow
        {
            throw new NotImplementedException();
        }

        public void RemoveRange(IEnumerable<IXTableRow> ents, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public bool TryGet(string name, out IXTableRow ent)
        {
            throw new NotImplementedException();
        }

        protected IEnumerable<T> IterateRows<T>()
            where T : SwDmTableRow
        {
            for (var i = 0; i < Count; i++)
            {
                yield return (T)CreateRow(i);
            }
        }

        protected virtual SwDmTableRow CreateRow(int index) => new SwDmTableRow(index, m_Table);
    }

    internal class SwDmBomTableRowRepository : SwDmTableRowRepository, IXBomTableRowRepository
    {
        IXBomTableRow IXRepository<IXBomTableRow>.PreCreate<IXBomTableRow>() => (IXBomTableRow)base.PreCreate<IXTableRow>();

        private readonly SwDmBomTable m_BomTable;

        internal SwDmBomTableRowRepository(SwDmBomTable table) : base(table)
        {
            m_BomTable = table;
        }

        IXBomTableRow IXBomTableRowRepository.this[int index] => (IXBomTableRow)base[index];

        IXBomTableRow IXRepository<IXBomTableRow>.this[string name] => (IXBomTableRow)base[name];

        public void AddRange(IEnumerable<IXBomTableRow> ents, CancellationToken cancellationToken)
            => base.AddRange(ents, cancellationToken);

        public void RemoveRange(IEnumerable<IXBomTableRow> ents, CancellationToken cancellationToken)
            => base.RemoveRange(ents, cancellationToken);

        public bool TryGet(string name, out IXBomTableRow ent)
        {
            if (base.TryGet(name, out var row))
            {
                ent = (IXBomTableRow)row;
                return true;
            }
            else
            {
                ent = null;
                return false;
            }
        }

        IEnumerator<IXBomTableRow> IEnumerable<IXBomTableRow>.GetEnumerator()
            => base.IterateRows<SwDmBomTableRow>().GetEnumerator();

        protected override SwDmTableRow CreateRow(int index)
            => new SwDmBomTableRow(index, m_BomTable);
    }
}
