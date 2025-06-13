using System;
using System.Threading;
using Xarial.XCad.Annotations;
using Xarial.XCad.Documents;
using Xarial.XCad.Toolkit.Exceptions;
using SolidWorks.Interop.swdocumentmgr;
using System.Linq;
using Xarial.XCad.SwDocumentManager.Documents;

namespace Xarial.XCad.SwDocumentManager.Annotations
{
    internal class SwDmTableRow : IXTableRow
    {
        public int Index { get => m_Index; set => throw new NotImplementedException(); }
        public bool Visible { get => true; set => throw new NotImplementedException(); }

        internal int ActualIndex 
        {
            get
            {
                var headerRowIndex = m_Table.GetHeaderRowIndex();

                var offset = headerRowIndex <= m_Index ? 1 : 0;

                return m_Index + offset;
            }
        }

        public IXTableCells Cells => new SwDmTableCells(this, m_Table);

        public bool IsCommitted => true;

        public void Commit(CancellationToken cancellationToken) => throw new ElementAlreadyCommittedException();

        private readonly int m_Index;
        private readonly SwDmTable m_Table;

        internal SwDmTableRow(int index, SwDmTable table)
        {
            m_Index = index;
            m_Table = table;
        }
    }

    internal class SwDmBomTableRow : SwDmTableRow, IXBomTableRow
    {
        private readonly SwDmBomTable m_BomTable;

        internal SwDmBomTableRow(int index, SwDmBomTable table) : base(index, table)
        {
            m_BomTable = table;
        }

        public string ItemNumber
        {
            get 
            {

                for (int i = 0; i < m_BomTable.Table.GetColumnCount(); i++) 
                {
                    if (m_BomTable.Table.GetColumnType(i) == SwDmColumnType.swDmColumnTypeItemNumber) 
                    {
                        var itemNo = Cells[i].Value;

                        if (string.IsNullOrEmpty(itemNo)) 
                        {
                            itemNo = BomItemNumber.None;
                        }

                        return itemNo;
                    }
                }

                return BomItemNumber.None;

            }
            set => throw new NotImplementedException(); 
        }

        public IXComponent[] Components 
        {
            get 
            {
                var err = m_BomTable.Table.GetComponentRep(ActualIndex, out var compRep);

                if (err == SwDmTableError.SwDmTableErrorNone)
                {
                    var rootAssm = (SwDmAssembly)m_BomTable.ReferencedDocument;

                    var compParts = compRep.Split('/');

                    for (int i = 1; i < compParts.Length; i++) 
                    {
                        var confNameStartIndex = compParts[i].LastIndexOf("<");

                        var confName = compParts[i].Substring(confNameStartIndex + 1, compParts[i].LastIndexOf(">") - confNameStartIndex - 1);

                        var compName = compParts[i].Substring(0, confNameStartIndex);
                    }

                    return Array.Empty<IXComponent>();
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
