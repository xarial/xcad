//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Diagnostics;
using Xarial.XCad.Annotations;
using Xarial.XCad.SolidWorks.Enums;

namespace Xarial.XCad.SolidWorks.Annotations
{
    [DebuggerDisplay("{" + nameof(Value) + "}")]
    internal class SwTableCell : IXTableCell
    {
        public IXTableRow Row { get; }

        public IXTableColumn Column { get; }

        private readonly SwTable m_Table;

        private readonly SwTableRowRepository m_Rows;

        internal SwTableCell(SwTable table, SwTableRow row, SwTableColumn column, SwTableRowRepository rows)
        {
            m_Table = table;
            Row = row;
            Column = column;
            m_Rows = rows;
        }

        public string Value 
        {
            get
            {
                if (m_Table.OwnerApplication.IsVersionNewerOrEqual(SwVersion_e.Sw2018))
                {
                    return m_Table.TableAnnotation.Text2[Row.Index + m_Rows.RowIndexOffset, Column.Index, true];
                }
                else 
                {
                    return m_Table.TableAnnotation.Text[Row.Index + m_Rows.RowIndexOffset, Column.Index];
                }
            }
            set
            {
                if (Column.Visible)
                {
                    if (m_Table.OwnerApplication.IsVersionNewerOrEqual(SwVersion_e.Sw2018))
                    {
                        m_Table.TableAnnotation.Text2[Row.Index + m_Rows.RowIndexOffset, Column.Index, true] = value;
                    }
                    else
                    {
                        m_Table.TableAnnotation.Text[Row.Index + m_Rows.RowIndexOffset, Column.Index] = value;
                    }
                }
                else 
                {
                    throw new NotSupportedException("Value cannot be set to an invisible column");
                }
            }
        }
    }
}
