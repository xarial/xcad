using System;
using System.Collections;
using System.Collections.Generic;
using Xarial.XCad.Annotations;

namespace Xarial.XCad.SwDocumentManager.Annotations
{
    internal class SwDmTableCells : IXTableCells
    {
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IXTableCell this[int colIndex] => new SwDmTableCell(m_Table, m_Row, (SwDmTableColumn)m_Table.Columns[colIndex]);

        private readonly SwDmTableRow m_Row;
        private SwDmTable m_Table;

        internal SwDmTableCells(SwDmTableRow row, SwDmTable table)
        {
            m_Row = row;
            m_Table = table;
        }

        public IEnumerator<IXTableCell> GetEnumerator()
        {
            foreach (SwDmTableColumn col in m_Table.Columns) 
            {
                yield return new SwDmTableCell(m_Table, m_Row, col);
            }
        }
    }
}
