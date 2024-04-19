//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
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

        public SwTableCells(SwTable table, SwTableRow tableRow)
        {
            m_Table = table;
            m_TableRow = tableRow;
        }

        public IXTableCell this[int index] => new SwTableCell(m_Table, m_TableRow, m_Table.Columns[index]);

        public IEnumerator<IXTableCell> GetEnumerator()
        {
            foreach(var col in m_Table.Columns)
            {
                yield return new SwTableCell(m_Table, m_TableRow, col);
            }
        }
    }
}
