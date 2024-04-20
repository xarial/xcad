//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swconst;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Annotations
{
    internal class SwTableColumnRepository : IXTableColumnRepository
    {
        IXTableColumn IXRepository<IXTableColumn>.this[string name] => this[name];
        IXTableColumn IXTableColumnRepository.this[int index] => this[index];
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        IEnumerator<IXTableColumn> IEnumerable<IXTableColumn>.GetEnumerator() => GetEnumerator();

        public SwTableColumn this[string name] => (SwTableColumn)RepositoryHelper.Get(this, name);

        public SwTableColumn this[int index] => new SwTableColumn(m_Table, index, m_ChangeTracker);

        public int Count => m_Table.TableAnnotation.TotalColumnCount;

        private readonly SwTable m_Table;

        private readonly ChangeTracker m_ChangeTracker;

        private readonly Lazy<SwTableColumn> m_ItemNumberColumnLazy;

        internal SwTableColumn ItemNumberColumn => m_ItemNumberColumnLazy.Value;

        internal SwTableColumnRepository(SwTable table, ChangeTracker changeTracker)
        {
            m_Table = table;

            m_ItemNumberColumnLazy = new Lazy<SwTableColumn>(GetItemNumberColumn);

            m_ChangeTracker = changeTracker;
        }

        private SwTableColumn GetItemNumberColumn()
        {
            foreach (var col in this) 
            {
                if (m_Table.TableAnnotation.GetColumnType3(col.Index, true, out _, out _) == (int)swTableColumnTypes_e.swBomTableColumnType_ItemNumber)
                {
                    return col;
                }
            }

            throw new Exception("Item number column is not present");
        }

        public void AddRange(IEnumerable<IXTableColumn> ents, CancellationToken cancellationToken)
            => RepositoryHelper.AddRange(ents, cancellationToken);

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters)
            => RepositoryHelper.FilterDefault(this, filters, reverseOrder);

        public IEnumerator<SwTableColumn> GetEnumerator()
        {
            for (var i = 0; i < Count; i++) 
            {
                yield return new SwTableColumn(m_Table, i, m_ChangeTracker);
            }
        }

        public T PreCreate<T>() where T : IXTableColumn
            => RepositoryHelper.PreCreate<IXTableColumn, T>(this,
                () => new SwTableColumn(m_Table, null, m_ChangeTracker));

        public void RemoveRange(IEnumerable<IXTableColumn> ents, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public bool TryGet(string name, out IXTableColumn ent)
        {
            foreach (var col in this)
            {
                if (string.Equals(col.Title, name, StringComparison.CurrentCultureIgnoreCase)) 
                {
                    ent = col;
                    return true;
                }
            }

            ent = null;
            return false;
        }
    }
}
