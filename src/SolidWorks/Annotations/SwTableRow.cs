//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swconst;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Documents;
using Xarial.XCad.Annotations;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Toolkit.Data;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Annotations
{
    [DebuggerDisplay("[{" + nameof(Index) + "}]")]
    internal class SwTableRow : TableElement, IXTableRow
    {
        public bool Visible 
        {
            get
            {
                CheckDeleted();
                return !m_Table.TableAnnotation.RowHidden[Index + m_Table.Rows.RowIndexOffset];
            }
            set
            {
                CheckDeleted();
                m_Table.TableAnnotation.RowHidden[Index + m_Table.Rows.RowIndexOffset] = !value;
            }
        }

        protected int VisibleIndex => GetVisibleIndex(Index);

        private int GetVisibleIndex(int index) 
        {
            var prevHiddenRowsCount = 0;

            for (int i = 0; i < index; i++)
            {
                if (!m_Table.Rows[i].Visible)
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

        internal SwTableRow(SwTable table, int? index, ChangeTracker changeTracker) : base(index, changeTracker)
        {
            m_Table = table;

            m_Cells = new SwTableCells(table, this);
        }

        protected override void Move(int to)
        {
            if (Visible && m_Table.Rows[to].Visible)
            {
                var srcIndex = VisibleIndex + m_Table.Rows.RowIndexOffset;
                var targIndex = GetVisibleIndex(to) + m_Table.Rows.RowIndexOffset;

                if (srcIndex != targIndex)
                {
                    if (!m_Table.TableAnnotation.MoveRow(srcIndex, (int)swTableItemInsertPosition_e.swTableItemMovePosition_Relative, targIndex - srcIndex))
                    {
                        throw new TableRowOperationException($"Failed to move the row {Index} to {to}");
                    }

                    m_ChangeTracker.Move(Index, to);
                }
            }
            else 
            {
                throw new TableRowOperationException("Only visible rows can be moved");
            }
        }

        protected override void CreateElement(int index, CancellationToken cancellationToken)
        {
            var targIndex = GetVisibleIndex(index) + m_Table.Rows.RowIndexOffset;

            swTableItemInsertPosition_e pos;

            if (targIndex == m_Table.Rows.RowIndexOffset)
            {
                pos = swTableItemInsertPosition_e.swTableItemInsertPosition_First;
            }
            else if (targIndex == m_Table.TableAnnotation.RowCount)
            {
                pos = swTableItemInsertPosition_e.swTableItemInsertPosition_Last;
            }
            else
            {
                pos = swTableItemInsertPosition_e.swTableItemInsertPosition_Before;

                if (!m_Table.Rows[index].Visible)
                {
                    throw new TableRowOperationException("Cannot create row at the hidden position");
                }
            }

            if (m_Table.TableAnnotation.InsertRow((int)pos, targIndex))
            {
                m_ChangeTracker.Insert(index);
            }
            else
            {
                throw new Exception("Failed to insert row");
            }
        }
    }

    internal class SwBomTableRow : SwTableRow, IXBomTableRow
    {
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        internal SwBomTableRow(SwTable table, int? index, ChangeTracker changeTracker) : base(table, index, changeTracker)
        {
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
                        throw new TableRowOperationException($"Only automatic item number is supported. Use {nameof(BomItemNumber)}.{nameof(BomItemNumber.Auto)}");
                    }
                }

                if (TryGetItemNumer(out _) != hasItemNumber)
                {
                    if (Visible)
                    {
                        var selData = m_Table.OwnerDocument.Model.ISelectionManager.CreateSelectData();

                        var visIndex = VisibleIndex + m_Table.Rows.RowIndexOffset;

                        selData.SetCellRange(visIndex, visIndex, 0, 0);

                        if (m_Table.Annotation.Select3(false, selData))
                        {
                            const int WM_COMMAND = 0x0111;
                            const int HIDE_ITEM_NUMBER = 24229;

                            var hWnd = m_Table.OwnerApplication.WindowHandle;

                            SendMessage(hWnd, WM_COMMAND, new IntPtr(HIDE_ITEM_NUMBER), IntPtr.Zero);

                            if (TryGetItemNumer(out _) != hasItemNumber)
                            {
                                throw new TableRowOperationException($"Failed to set has item number");
                            }
                        }
                        else
                        {
                            throw new TableRowOperationException($"Failed to select row to hide item number");
                        }
                    }
                    else
                    {
                        throw new TableRowOperationException("Cannot set item number on the invisible row");
                    }
                }
            }
        }

        private bool TryGetItemNumer(out int? itemNumber)
        {
            var itemNumberTxt = Cells[m_Table.Columns.ItemNumberColumn.Index].Value;
            
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
