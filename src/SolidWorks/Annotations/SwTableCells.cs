//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections;
using System.Collections.Generic;
using Xarial.XCad.Annotations;

namespace Xarial.XCad.SolidWorks.Annotations
{
    internal class SwTableCells : IXTableCells
    {
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private readonly SwTable m_Table;
        private readonly SwTableRow m_TableRow;

        private readonly SwTableRowRepository m_Rows;
        private readonly SwTableColumnRepository m_Columns;

        public SwTableCells(SwTable table, SwTableRow tableRow, SwTableRowRepository rows, SwTableColumnRepository columns)
        {
            m_Table = table;
            m_TableRow = tableRow;

            m_Rows = rows;
            m_Columns = columns;
        }

        public IXTableCell this[int index] => new SwTableCell(m_Table, m_TableRow, m_Columns[index], m_Rows);

        public IEnumerator<IXTableCell> GetEnumerator()
        {
            foreach(var col in m_Columns)
            {
                yield return new SwTableCell(m_Table, m_TableRow, col, m_Rows);
            }
        }
    }
}
