using SolidWorks.Interop.swdocumentmgr;
using System;
using Xarial.XCad.Annotations;

namespace Xarial.XCad.SwDocumentManager.Annotations
{
    internal class SwDmTableCell : IXTableCell
    {
        public IXTableRow Row => m_Row;

        public IXTableColumn Column => m_Col;

        public string Value 
        {
            get 
            {
                var err = m_Table.Table.GetCellText(m_Row.ActualIndex, Column.Index, out var cellText);

                if (err == SwDmTableError.SwDmTableErrorNone)
                {
                    return cellText;
                }
                else 
                {
                    throw new Exception($"Failed to read cell value. Error code: {err}");
                }
            }
            set => throw new NotImplementedException(); 
        }

        private readonly SwDmTable m_Table;
        private readonly SwDmTableRow m_Row;
        private readonly SwDmTableColumn m_Col;

        internal SwDmTableCell(SwDmTable table, SwDmTableRow row, SwDmTableColumn col) 
        {
            m_Table = table;
            m_Row = row;
            m_Col = col;
        }
    }
}
