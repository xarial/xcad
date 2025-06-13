using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.SwDocumentManager.Documents;
using Xarial.XCad.SwDocumentManager.Services;
using Xarial.XCad.Toolkit.Exceptions;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SwDocumentManager.Annotations
{
    /// <summary>
    /// SOLIDWORKS Document Manager specific table
    /// </summary>
    public interface ISwDmTable : ISwDmAnnotation, IXTable
    {
        /// <summary>
        /// Pointer to native table
        /// </summary>
        ISwDMTable Table { get; }
    }

    internal class SwDmTable : SwDmSelObject, ISwDmTable, IReadable
    {
        private class SwDmTableDataReader : IDataReader
        {
            private readonly ISwDMTable m_Table;

            private bool m_IsClosed;
            private int m_CurrentRowIndex;

            private int m_ColumnsCount;
            private int m_RowsCount;

            private readonly int m_HeaderRowIndex;

            public SwDmTableDataReader(ISwDMTable table, int headerRowIndex, bool visOnly)
            {
                if (!visOnly) 
                {
                    throw new NotSupportedException("Hidden rows and columns are not supported");
                }

                m_Table = table;

                m_HeaderRowIndex = headerRowIndex;
                
                m_IsClosed = false;

                m_RowsCount = table.GetRowCount();
                m_ColumnsCount = table.GetColumnCount();

                m_CurrentRowIndex = -1 + (m_HeaderRowIndex == 0 ? 1 : 0);
            }

            public bool IsClosed => m_IsClosed;
            public int FieldCount => m_ColumnsCount;

            public object this[int i] => throw new NotImplementedException();
            public object this[string name] => throw new NotImplementedException();
            public int Depth => throw new NotImplementedException();
            public int RecordsAffected => throw new NotImplementedException();
            public bool GetBoolean(int i)=> throw new NotImplementedException();
            public byte GetByte(int i) => throw new NotImplementedException();
            public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) => throw new NotImplementedException();
            public char GetChar(int i) => throw new NotImplementedException();
            public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) => throw new NotImplementedException();
            public IDataReader GetData(int i) => throw new NotImplementedException();
            public string GetDataTypeName(int i) => throw new NotImplementedException();
            public DateTime GetDateTime(int i) => throw new NotImplementedException();
            public decimal GetDecimal(int i) => throw new NotImplementedException();
            public double GetDouble(int i) => throw new NotImplementedException();
            public float GetFloat(int i) => throw new NotImplementedException();
            public Guid GetGuid(int i) => throw new NotImplementedException();
            public short GetInt16(int i) => throw new NotImplementedException();
            public int GetInt32(int i) => throw new NotImplementedException();
            public long GetInt64(int i) => throw new NotImplementedException();
            public bool IsDBNull(int i) => throw new NotImplementedException();

            public string GetName(int i) => GetColumnTitle(i);
            public Type GetFieldType(int i) => typeof(string);

            public int GetOrdinal(string name)
            {
                for (int i = 0; i < m_ColumnsCount; i++)
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

            private string GetColumnTitle(int columnIndex)
            {
                if (m_HeaderRowIndex != -1)
                {
                    return GetCellTextAt(m_HeaderRowIndex, columnIndex);
                }
                else
                {
                    return "";
                }
            }

            public bool NextResult() => false;

            public bool Read()
            {
                m_CurrentRowIndex++;

                if (m_CurrentRowIndex == m_HeaderRowIndex) 
                {
                    m_CurrentRowIndex++;
                }

                return m_CurrentRowIndex < m_RowsCount;
            }

            private string ReadCellValue(int columnIndex) => GetCellTextAt(m_CurrentRowIndex, columnIndex);

            private string GetCellTextAt(int rowIndex, int columnIndex)
            {
                var err = m_Table.GetCellText(rowIndex, columnIndex, out var cellText);

                if (err == SwDmTableError.SwDmTableErrorNone)
                {
                    return cellText;
                }
                else
                {
                    throw new Exception($"Failed to read cell text at {m_CurrentRowIndex}:{columnIndex}. Error code: {err}");
                }
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

        public ISwDMTable Table { get; }

        public SwDmTable(ISwDMTable table, SwDmApplication ownerApp, SwDmDocument ownerDoc) : base(table, ownerApp, ownerDoc)
        {
            Table = table;
        }

        public IXTableColumnRepository Columns => new SwDmTableColumnRepository(this);

        public virtual IXTableRowRepository Rows => new SwDmTableRowRepository(this);

        public XCad.Geometry.Structures.Point Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IFont Font { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Color? Color { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IXLayer Layer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        internal int GetHeaderRowIndex()
        {
            int headerRowIndex;

            switch (Table.GetHeaderStyle())
            {
                case SwDmTableHeaderStyle.SwDmTableHeaderStyleNotFound:
                    headerRowIndex = -1;
                    break;

                case SwDmTableHeaderStyle.SwDmTableHeaderStyleTop:
                    headerRowIndex = 0;
                    break;

                case SwDmTableHeaderStyle.SwDmTableHeaderStyleBottom:
                    headerRowIndex = Table.GetRowCount() - 1;
                    break;

                default:
                    headerRowIndex = -1;
                    break;
            }

            return headerRowIndex;
        }

        public IDataReader ExecuteReader(bool visibleOnly) => new SwDmTableDataReader(Table, GetHeaderRowIndex(), visibleOnly);
    }

    internal class SwDmDrawingTable : SwDmTable, IXDrawingTable
    {
        public SwDmDrawingTable(ISwDMTable table, SwDmApplication ownerApp, SwDmDrawing ownerDoc) : base(table, ownerApp, ownerDoc)
        {
        }

        public IXObject Owner 
        {
            get => OwnerDocument.CreateObjectFromDispatch<ISwDmSheet>(((ISwDMTable5)Table).Sheet); 
            set => throw new NotImplementedException(); 
        }
    }

    internal class SwDmBomTable : SwDmTable, IXBomTable
    {
        IXBomTableRowRepository IXBomTable.Rows => (IXBomTableRowRepository)Rows;

        private readonly IFilePathResolver m_FilePathResolver;

        public SwDmBomTable(ISwDMTable table, SwDmApplication ownerApp, SwDmDocument ownerDoc) : base(table, ownerApp, ownerDoc)
        {
            m_FilePathResolver = ownerApp.FilePathResolver;
        }

        public IXDocument3D ReferencedDocument 
        {
            get 
            {
                var docCachedPath = ((ISwDMTable2)Table).ReferencedDocumentName(out var err);

                string docPath;

                try
                {
                    docPath = m_FilePathResolver.ResolvePath(Path.GetDirectoryName(OwnerDocument.Path), docCachedPath);
                }
                catch 
                {
                    docPath = docCachedPath;
                }

                if (err == SwDmTableError.SwDmTableErrorNone)
                {
                    if (!OwnerApplication.Documents.TryGet(docPath, out var doc)) 
                    {
                        doc = (ISwDmDocument3D)OwnerApplication.Documents.PreCreateFromPath(docPath);

                        if (OwnerDocument.State.HasFlag(DocumentState_e.ReadOnly)) 
                        {
                            doc.State = DocumentState_e.ReadOnly;
                        }
                    }

                    return (IXDocument3D)doc;
                }
                else 
                {
                    throw new Exception($"Failed to get referenced document from BOM table. Error code: {err}");
                }
            }
        }

        public IXConfiguration ReferencedConfiguration 
        {
            get 
            {
                var refConfNames = (string[])((ISwDMTable2)Table).ReferencedConfigurationNames(out var err);

                if (err == SwDmTableError.SwDmTableErrorNone)
                {
                    return ReferencedDocument.Configurations[refConfNames.First()];
                }
                else
                {
                    throw new Exception($"Failed to get referenced configuration from BOM table. Error code: {err}");
                }
            }
        }

        public override IXTableRowRepository Rows => new SwDmBomTableRowRepository(this);
        
    }

    internal class SwDmDrawingBomTable : SwDmBomTable, IXDrawingBomTable
    {
        public SwDmDrawingBomTable(ISwDMTable table, SwDmApplication ownerApp, SwDmDrawing ownerDoc) : base(table, ownerApp, ownerDoc)
        {
        }

        public IXObject Owner
        {
            get => OwnerDocument.CreateObjectFromDispatch<ISwDmSheet>(((ISwDMTable5)Table).Sheet);
            set => throw new NotImplementedException();
        }
    }
}
