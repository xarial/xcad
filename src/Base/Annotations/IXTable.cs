//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;

namespace Xarial.XCad.Annotations
{

    /// <summary>
    /// Represents the table annotation
    /// </summary>
    public interface IXTable : IXAnnotation
    {
        /// <summary>
        /// Columns which belong to this table
        /// </summary>
        IXTableColumnRepository Columns { get; }

        /// <summary>
        /// Rows which belong to this table
        /// </summary>
        IXTableRowRepository Rows { get; }
    }

    /// <summary>
    /// Represents the Bill Of Materials table
    /// </summary>
    public interface IXBomTable : IXTable 
    {
        /// <summary>
        /// Referenced document of this BOM feature
        /// </summary>
        IXDocument3D ReferencedDocument { get; }

        /// <summary>
        /// Referenced configuration of this BOM feature
        /// </summary>
        IXConfiguration ReferencedConfiguration { get; }

        /// <summary>
        /// BOM rows which belong to this table
        /// </summary>
        new IXBomTableRowRepository Rows { get; }
    }

    /// <summary>
    /// Adds additional methods for the <see cref="IXTable"/>
    /// </summary>
    public static class TableExtension
    {
        private class TableDataReader : IDataReader
        {
            private readonly IXTable m_Table;

            private bool m_IsClosed;
            private int m_CurrentRowIndex;

            private readonly bool m_VisibleOnly;

            private readonly IXTableColumnRepository m_Columns;
            private readonly IXTableRowRepository m_Rows;

            private readonly int m_ColumnsCount;
            private readonly int m_RowsCount;

            private readonly int[] m_ColumnsPositionMap;

            public TableDataReader(IXTable table, bool visibleOnly)
            {
                m_Table = table;

                m_Columns = table.Columns;
                m_Rows = table.Rows;

                m_VisibleOnly = visibleOnly;

                m_IsClosed = false;
                m_CurrentRowIndex = -1;

                if (visibleOnly)
                {
                    m_ColumnsPositionMap = m_Columns.Where(c => c.Visible).Select(c => c.Index).ToArray();
                }
                else
                {
                    m_ColumnsPositionMap = Enumerable.Range(0, m_Columns.Count).ToArray();
                }

                m_ColumnsCount = m_ColumnsPositionMap.Length;
                m_RowsCount = m_Rows.Count;
            }

            public object this[int i] => throw new NotImplementedException();

            public object this[string name] => throw new NotImplementedException();

            public int Depth => throw new NotImplementedException();

            public bool IsClosed => m_IsClosed;

            public int RecordsAffected => throw new NotImplementedException();

            public int FieldCount => m_ColumnsCount;

            public bool GetBoolean(int i)
            {
                throw new NotImplementedException();
            }

            public byte GetByte(int i)
            {
                throw new NotImplementedException();
            }

            public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
            {
                throw new NotImplementedException();
            }

            public char GetChar(int i)
            {
                throw new NotImplementedException();
            }

            public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
            {
                throw new NotImplementedException();
            }

            public IDataReader GetData(int i)
            {
                throw new NotImplementedException();
            }

            public string GetDataTypeName(int i)
            {
                throw new NotImplementedException();
            }

            public DateTime GetDateTime(int i)
            {
                throw new NotImplementedException();
            }

            public decimal GetDecimal(int i)
            {
                throw new NotImplementedException();
            }

            public double GetDouble(int i)
            {
                throw new NotImplementedException();
            }

            public Type GetFieldType(int i) => typeof(string);

            public float GetFloat(int i)
            {
                throw new NotImplementedException();
            }

            public Guid GetGuid(int i)
            {
                throw new NotImplementedException();
            }

            public short GetInt16(int i)
            {
                throw new NotImplementedException();
            }

            public int GetInt32(int i)
            {
                throw new NotImplementedException();
            }

            public long GetInt64(int i)
            {
                throw new NotImplementedException();
            }

            public string GetName(int i) => GetColumnTitle(i);

            public int GetOrdinal(string name)
            {
                for (int i = 0; i < FieldCount; i++)
                {
                    if (string.Equals(GetColumnTitle(i), name, StringComparison.OrdinalIgnoreCase))
                    {
                        return i;
                    }
                }

                return -1;
            }

            public DataTable GetSchemaTable() => null;

            public string GetString(int i) => ReadCellValue(i);

            public object GetValue(int i) => ReadCellValue(i);

            public int GetValues(object[] values)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = ReadCellValue(i);
                }

                return values.Length;
            }

            private string GetColumnTitle(int columnIndex) => m_Columns[m_ColumnsPositionMap[columnIndex]].Title;

            private string ReadCellValue(int columnIndex)
            {
                while (m_CurrentRowIndex < m_RowsCount)
                {
                    var row = m_Rows[m_CurrentRowIndex];

                    if (!m_VisibleOnly || row.Visible)
                    {
                        return row.Cells[m_ColumnsPositionMap[columnIndex]].Value;
                    }

                    m_CurrentRowIndex++;
                }

                throw new IndexOutOfRangeException();
            }

            public bool IsDBNull(int i)
            {
                throw new NotImplementedException();
            }

            public bool NextResult() => false;

            public bool Read()
            {
                m_CurrentRowIndex++;
                return m_CurrentRowIndex < m_RowsCount;
            }

            public void Close()
            {
                m_IsClosed = true;
            }

            public void Dispose()
            {
                Close();
                m_IsClosed = true;
            }
        }

        /// <summary>
        /// Get the cell
        /// </summary>
        /// <param name="table">Table to get cell from</param>
        /// <param name="colIndex">Index of the column</param>
        /// <param name="rowIndex">Index of the row</param>
        /// <returns>Cell</returns>
        public static IXTableCell GetCell(this IXTable table, int colIndex, int rowIndex)
            => table.Rows[rowIndex].Cells[colIndex];

        /// <summary>
        /// Returns the table data reader
        /// </summary>
        /// <param name="table">Table to read from</param>
        /// <param name="visibleOnly">Only read visible data</param>
        /// <returns>Data reader</returns>
        public static IDataReader ExecuteReader(this IXTable table, bool visibleOnly = true)
        {
            if (table is IReadable)
            {
                return ((IReadable)table).ExecuteReader(visibleOnly);
            }
            else 
            {
                return new TableDataReader(table, visibleOnly);
            }
        }

        /// <summary>
        /// Reads the content of the table
        /// </summary>
        /// <param name="table">Table to read from</param>
        /// <param name="visibleOnly">Only read visible data</param>
        /// <returns>Data table</returns>
        public static DataTable Read(this IXTable table, bool visibleOnly = true) 
        {
            var dataTable = new DataTable();
            dataTable.Load(table.ExecuteReader(visibleOnly));
            return dataTable;
        }
    }
}
