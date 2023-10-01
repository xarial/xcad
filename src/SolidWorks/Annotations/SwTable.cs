using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Annotations;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks.Annotations
{
    public interface ISwTable : IXTable 
    {
        ITableAnnotation TableAnnotation { get; }
    }

    internal class SwTable : SwAnnotation, ISwTable
    {
        private class TableAnnotationReader : IDataReader
        {
            private readonly ITableAnnotation m_TableAnn;

            private bool m_IsClosed;
            private int m_CurrentRowIndex;

            public TableAnnotationReader(ITableAnnotation tableAnn)
            {
                m_TableAnn = tableAnn;

                m_IsClosed = false;
                m_CurrentRowIndex = m_TableAnn.GetHeaderCount() - 1;
            }

            public object this[int i] => throw new NotImplementedException();

            public object this[string name] => throw new NotImplementedException();

            public int Depth => throw new NotImplementedException();

            public bool IsClosed => m_IsClosed;

            public int RecordsAffected => throw new NotImplementedException();

            public int FieldCount => m_TableAnn.ColumnCount;

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
                for (int i = 0; i < m_TableAnn.ColumnCount; i++) 
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

            private string GetColumnTitle(int columnIndex) => m_TableAnn.GetColumnTitle2(columnIndex, false);
            private string ReadCellValue(int columnIndex) => m_TableAnn.Text2[m_CurrentRowIndex, columnIndex, false];

            public bool IsDBNull(int i)
            {
                throw new NotImplementedException();
            }

            public bool NextResult() => false;

            public bool Read()
            {
                m_CurrentRowIndex++;
                return m_CurrentRowIndex < m_TableAnn.RowCount;
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

        public ITableAnnotation TableAnnotation { get; }

        internal SwTable(ITableAnnotation tableAnn, SwDocument doc, SwApplication app) : base(tableAnn?.GetAnnotation(), doc, app)
        {
            TableAnnotation = tableAnn;
        }

        public IDataReader CreateReader() => new TableAnnotationReader(TableAnnotation);
    }
}
