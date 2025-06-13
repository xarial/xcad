using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SwDocumentManager.Annotations
{
    internal class SwDmTableColumnRepository : IXTableColumnRepository
    {
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IXTableColumn this[int index] 
        {
            get
            {
                if (index < m_Table.Table.GetColumnCount())
                {
                    return CreateColumn(index, m_Table.GetHeaderRowIndex());
                }
                else 
                {
                    throw new IndexOutOfRangeException();
                }
            }
        }

        public IXTableColumn this[string name] => m_RepoHelper.Get(name);

        public int Count => m_Table.Table.GetColumnCount();

        private readonly SwDmTable m_Table;

        private readonly RepositoryHelper<IXTableColumn> m_RepoHelper;

        internal SwDmTableColumnRepository(SwDmTable table) 
        {
            m_Table = table;

            m_RepoHelper = new RepositoryHelper<IXTableColumn>(this);
        }

        public void AddRange(IEnumerable<IXTableColumn> ents, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) 
            => m_RepoHelper.FilterDefault(this, filters, reverseOrder);

        public IEnumerator<IXTableColumn> GetEnumerator() => IterateColumns(m_Table.GetHeaderRowIndex()).GetEnumerator();

        public T PreCreate<T>() where T : IXTableColumn
        {
            throw new NotImplementedException();
        }

        public void RemoveRange(IEnumerable<IXTableColumn> ents, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public bool TryGet(string name, out IXTableColumn ent)
        {
            var headerRowIndex = m_Table.GetHeaderRowIndex();

            if (headerRowIndex != -1)
            {
                ent = IterateColumns(headerRowIndex).FirstOrDefault(c => string.Equals(c.Title, name, StringComparison.CurrentCultureIgnoreCase));
                return ent != null;
            }
            else 
            {
                ent = null;
                return false;
            }
        }

        private IEnumerable<SwDmTableColumn> IterateColumns(int headerRowIndex)
        {
            for (int i = 0; i < m_Table.Table.GetColumnCount(); i++)
            {
                yield return CreateColumn(i, headerRowIndex);
            }
        }

        private SwDmTableColumn CreateColumn(int columnIndex, int headerRowIndex)
        {
            string name;

            if (headerRowIndex != -1)
            {
                m_Table.Table.GetCellText(headerRowIndex, columnIndex, out name);
            }
            else
            {
                name = "";
            }

            return new SwDmTableColumn(columnIndex, name);
        }
    }
}
