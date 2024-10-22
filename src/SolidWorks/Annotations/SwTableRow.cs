//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Documents;
using Xarial.XCad.Annotations;
using Xarial.XCad.Documents;
using Xarial.XCad.Exceptions;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.Toolkit.Data;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Annotations
{
    [DebuggerDisplay("[{" + nameof(Index) + "}]")]
    internal class SwTableRow : TableElement, IXTableRow
    {
        protected class SwTableVisibleRow : IDisposable 
        {
            private readonly bool m_IsVisible;

            private readonly IXTableRow m_Row;

            internal SwTableVisibleRow(IXTableRow row) 
            {
                m_Row = row;
                m_IsVisible = m_Row.Visible;

                if (!m_IsVisible)
                {
                    m_Row.Visible = true;
                }
            }

            public void Dispose()
            {
                if (!m_IsVisible)
                {
                    m_Row.Visible = false;
                }
            }
        }

        public bool Visible 
        {
            get
            {
                CheckDeleted();
                return !m_Table.TableAnnotation.RowHidden[Index + m_Rows.RowIndexOffset];
            }
            set
            {
                CheckDeleted();
                m_Table.TableAnnotation.RowHidden[Index + m_Rows.RowIndexOffset] = !value;
            }
        }

        protected int VisibleIndex => GetVisibleIndex(Index);

        private int GetVisibleIndex(int index) 
        {
            var prevHiddenRowsCount = 0;

            for (int i = 0; i < index; i++)
            {
                if (!m_Rows[i].Visible)
                {
                    prevHiddenRowsCount++;
                }
            }

            return index - prevHiddenRowsCount;
        }

        public IXTableCells Cells 
        {
            get 
            {
                CheckDeleted();
                return m_Cells;
            }
        }

        protected readonly SwTable m_Table;
        private readonly SwTableCells m_Cells;

        protected readonly SwTableRowRepository m_Rows;

        internal SwTableRow(SwTable table, int? index, SwTableRowRepository rows, SwTableColumnRepository columns, ChangeTracker changeTracker) : base(index, changeTracker)
        {
            m_Table = table;
            m_Rows = rows;
            m_Cells = new SwTableCells(table, this, rows, columns);
        }

        protected override void Move(int to)
        {
            using (new SwTableVisibleRow(this)) 
            {
                using (new SwTableVisibleRow(m_Rows[to])) 
                {
                    var srcIndex = VisibleIndex + m_Rows.RowIndexOffset;
                    var targIndex = GetVisibleIndex(to) + m_Rows.RowIndexOffset;

                    if (srcIndex != targIndex)
                    {
                        if (!m_Table.TableAnnotation.MoveRow(srcIndex, (int)swTableItemInsertPosition_e.swTableItemMovePosition_Relative, targIndex - srcIndex))
                        {
                            throw new TableElementOperationException($"Failed to move the row {Index} to {to}");
                        }

                        m_ChangeTracker.Move(Index, to);
                    }
                }
            }
        }

        protected override void CreateElement(int index, CancellationToken cancellationToken)
        {
            var targIndex = GetVisibleIndex(index) + m_Rows.RowIndexOffset;

            swTableItemInsertPosition_e pos;

            IXTableRow targRow;

            if (targIndex == m_Rows.RowIndexOffset)
            {
                pos = swTableItemInsertPosition_e.swTableItemInsertPosition_First;
                targRow = m_Rows.FirstOrDefault();
            }
            else if (targIndex == m_Table.TableAnnotation.RowCount)
            {
                pos = swTableItemInsertPosition_e.swTableItemInsertPosition_Last;
                targRow = m_Rows.LastOrDefault(); 
            }
            else
            {
                pos = swTableItemInsertPosition_e.swTableItemInsertPosition_Before;
                targRow = m_Rows[index];
            }

            using (targRow != null ? new SwTableVisibleRow(targRow) : null) 
            {
                if (m_Table.TableAnnotation.InsertRow((int)pos, targIndex))
                {
                    m_ChangeTracker.Insert(index);
                }
                else
                {
                    throw new TableElementOperationException("Failed to insert row");
                }
            }
        }

        internal void Delete()
        {
            CheckDeleted();

            ValidateCanDelete();

            if (m_Table.OwnerApplication.IsVersionNewerOrEqual(SwVersion_e.Sw2018))
            {
                var totalRowsCount = m_Table.TableAnnotation.TotalRowCount;

                if (m_Table.TableAnnotation.DeleteRow2(Index + m_Rows.RowIndexOffset, true))
                {
                    if (m_Table.TableAnnotation.TotalRowCount != totalRowsCount - 1) 
                    {
                        throw new TableElementOperationException("Row is deleted but total number of table rows is not changed");
                    }
                }
                else 
                {
                    throw new TableElementOperationException("Failed to delete row");
                }
            }
            else
            {
                using (new SwTableVisibleRow(this)) 
                {
                    if (!m_Table.TableAnnotation.DeleteRow(VisibleIndex + m_Rows.RowIndexOffset))
                    {
                        throw new TableElementOperationException("Failed to delete row");
                    }
                }
            }

            m_ChangeTracker.Delete(Index);
        }

        protected virtual void ValidateCanDelete() 
        {
        }
    }

    internal class SwBomTableRow : SwTableRow, IXBomTableRow
    {
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private readonly SwTableColumnRepository m_Columns;

        private readonly SwBomTable m_BomTable;

        internal SwBomTableRow(SwBomTable table, int? index, SwTableRowRepository rows, SwTableColumnRepository columns, ChangeTracker changeTracker) 
            : base(table, index, rows, columns, changeTracker)
        {
            m_BomTable = table;
            m_Columns = columns;
        }

        public int? ItemNumber
        {
            get
            {
                CheckDeleted();
                TryGetItemNumer(out var itemNumber);
                return itemNumber;
            }
            set
            {
                CheckDeleted();

                var hasItemNumber = value.HasValue;

                if (hasItemNumber)
                {
                    if (value != BomItemNumber.Auto)
                    {
                        throw new TableElementOperationException($"Only automatic item number is supported. Use {nameof(BomItemNumber)}.{nameof(BomItemNumber.Auto)}");
                    }
                }

                if (TryGetItemNumer(out _) != hasItemNumber)
                {
                    using(new SwTableVisibleRow(this))
                    {
                        var selData = m_Table.OwnerDocument.Model.ISelectionManager.CreateSelectData();

                        var visIndex = VisibleIndex + m_Rows.RowIndexOffset;

                        selData.SetCellRange(visIndex, visIndex, 0, 0);

                        if (m_Table.Annotation.Select3(false, selData))
                        {
                            const int WM_COMMAND = 0x0111;
                            const int HIDE_ITEM_NUMBER = 24229;

                            var hWnd = m_Table.OwnerApplication.WindowHandle;

                            SendMessage(hWnd, WM_COMMAND, new IntPtr(HIDE_ITEM_NUMBER), IntPtr.Zero);

                            if (TryGetItemNumer(out _) != hasItemNumber)
                            {
                                throw new TableElementOperationException($"Failed to set has item number");
                            }
                        }
                        else
                        {
                            throw new TableElementOperationException($"Failed to select row to hide item number");
                        }
                    }
                }
            }
        }

        public IXComponent[] Components 
        {
            get 
            {
                var visible = Visible;

                if (!visible) 
                {
                    Visible = true;
                }

                try
                {
                    var confName = m_BomTable.ReferencedConfiguration?.Name;

                    var rowIndex = VisibleIndex + m_Rows.RowIndexOffset;

                    var comps = (object[])m_BomTable.BomTableAnnotation.GetComponents2(rowIndex, confName);

                    if (comps == null) 
                    {
                        //NOTE: in some cases IBomTableAnnotation::GetComponents2 returns null, while compnent is available, while obsolete API returns valid results
                        comps = (object[])m_BomTable.BomTableAnnotation.GetComponents(rowIndex);
                    }

                    if (comps != null)
                    {
                        var refDoc = (ISwDocument)m_BomTable.ReferencedDocument;

                        return comps.Select(c => m_BomTable.OwnerApplication.CreateObjectFromDispatch<ISwComponent>(c, refDoc)).ToArray();
                    }
                    else
                    {
                        return null;
                    }
                }
                finally 
                {
                    if (!visible) 
                    {
                        Visible = false;
                    }
                }
            }
        }

        protected override void ValidateCanDelete()
        {
            if (Components?.Any() == true) 
            {
                throw new TableElementOperationException("BOM row with the components cannnot be deleted from API");
            }
        }

        private bool TryGetItemNumer(out int? itemNumber)
        {
            var itemNumberTxt = Cells[m_Columns.ItemNumberColumn.Index].Value;
            
            if (!string.IsNullOrEmpty(itemNumberTxt))
            {
                itemNumber = int.Parse(itemNumberTxt);
                return true;
            }
            else 
            {
                itemNumber = null;
                return false;
            }
        }
    }
}
