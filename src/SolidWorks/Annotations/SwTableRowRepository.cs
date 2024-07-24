//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Annotations
{
    internal class SwTableRowRepository : IXTableRowRepository
    {
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IXTableRow this[string name] => m_RepoHelper.Get(name);

        public IXTableRow this[int index] => CreateRow(index);

        public int Count => m_Table.TableAnnotation.TotalRowCount - RowIndexOffset;

        internal int RowIndexOffset => m_RowIndexOffsetLazy.Value;

        private readonly Lazy<int> m_RowIndexOffsetLazy;

        protected readonly SwTable m_Table;

        protected readonly ChangeTracker m_ChangeTracker;

        protected Lazy<SwTableColumnRepository> m_ColumnsLazy;

        private readonly RepositoryHelper<IXTableRow> m_RepoHelper;

        internal SwTableRowRepository(SwTable table, ChangeTracker changeTracker) 
        {
            m_Table = table;
            m_ColumnsLazy = new Lazy<SwTableColumnRepository>(() => table.Columns);

            m_RepoHelper = new RepositoryHelper<IXTableRow>(this,
                TransactionFactory<IXTableRow>.Create(() => CreateRow(null)));

            m_ChangeTracker = changeTracker;

            m_RowIndexOffsetLazy = new Lazy<int>(m_Table.TableAnnotation.GetHeaderCount);
        }

        public void AddRange(IEnumerable<IXTableRow> ents, CancellationToken cancellationToken)
            => m_RepoHelper.AddRange(ents, cancellationToken);

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters)
            => m_RepoHelper.FilterDefault(this, filters, reverseOrder);

        public IEnumerator<IXTableRow> GetEnumerator() 
            => IterateRows<SwTableRow>().GetEnumerator();

        public T PreCreate<T>() where T : IXTableRow
            => m_RepoHelper.PreCreate<T>();

        /// <remarks>
        /// Due to current bug in SOLIDWORKS removing rows (both from API or UI) will break the indices of visible rows
        /// </remarks>
        public void RemoveRange(IEnumerable<IXTableRow> ents, CancellationToken cancellationToken)
        {
            foreach (SwTableRow row in ents.ToArray()) 
            {
                row.Delete();
            }
        }

        public bool TryGet(string name, out IXTableRow ent)
            => throw new NotSupportedException("Get row by index");

        protected IEnumerable<T> IterateRows<T>()
            where T : SwTableRow
        {
            for (var i = 0; i < Count; i++)
            {
                yield return (T)CreateRow(i);
            }
        }

        protected virtual SwTableRow CreateRow(int? index) 
            => new SwTableRow(m_Table, index, this, m_ColumnsLazy.Value, m_ChangeTracker);
    }

    internal class SwBomTableRowRepository : SwTableRowRepository, IXBomTableRowRepository
    {
        IXBomTableRow IXRepository<IXBomTableRow>.PreCreate<IXBomTableRow>() => (IXBomTableRow)base.PreCreate<IXTableRow>();

        private readonly SwBomTable m_BomTable;

        internal SwBomTableRowRepository(SwBomTable table, ChangeTracker changeTracker) : base(table, changeTracker)
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
            => base.IterateRows<SwBomTableRow>().GetEnumerator();

        protected override SwTableRow CreateRow(int? index)
            => new SwBomTableRow(m_BomTable, index, this, m_ColumnsLazy.Value, m_ChangeTracker);
    }
}
