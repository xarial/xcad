//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.Toolkit.Data;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Annotations
{
    /// <summary>
    /// Represents the SOLIDWORKS table
    /// </summary>
    public interface ISwTable : IXTable, ISwAnnotation
    {
        /// <summary>
        /// Pointer to the table annotation
        /// </summary>
        ITableAnnotation TableAnnotation { get; }
    }

    internal class SwTable : SwAnnotation, ISwTable, IReadable
    {
        internal static SwTable New(ITableAnnotation tableAnn, SwDocument doc, SwApplication app)
        {
            if (doc is IXDrawing)
            {
                return SwDrawingTable.New(tableAnn, (SwDrawing)doc, app);
            }
            else
            {
                return new SwTable(tableAnn, doc, app);
            }
        }

        private class SwTableDataReader : IDataReader
        {
            private readonly ITableAnnotation m_TableAnn;

            private bool m_IsClosed;
            private int m_CurrentRowIndex;

            private readonly int m_ColumnsCount;
            private readonly int m_RowsCount;
            private readonly bool m_VisOnly;

            public SwTableDataReader(ITableAnnotation tableAnn, bool visOnly)
            {
                m_TableAnn = tableAnn;

                m_VisOnly = visOnly;
                m_IsClosed = false;

                m_RowsCount = visOnly ? tableAnn.RowCount : tableAnn.TotalRowCount;
                m_ColumnsCount = visOnly ? tableAnn.ColumnCount : tableAnn.TotalColumnCount;

                m_CurrentRowIndex = m_TableAnn.GetHeaderCount() - 1;
            }

            public bool IsClosed => m_IsClosed;
            public int FieldCount => m_ColumnsCount;

            public object this[int i] => throw new NotImplementedException();
            public object this[string name] => throw new NotImplementedException();
            public int Depth => throw new NotImplementedException();
            public int RecordsAffected => throw new NotImplementedException();
            public bool GetBoolean(int i) => throw new NotImplementedException();
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

            public Type GetFieldType(int i) => typeof(string);

            public string GetName(int i) => GetColumnTitle(i);

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

            private string GetColumnTitle(int columnIndex) => m_TableAnn.GetColumnTitle2(columnIndex, !m_VisOnly);
            private string ReadCellValue(int columnIndex) => m_TableAnn.Text2[m_CurrentRowIndex, columnIndex, !m_VisOnly];

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

        IXTableRowRepository IXTable.Rows => Rows;
        IXTableColumnRepository IXTable.Columns => Columns;

        public ITableAnnotation TableAnnotation { get; }

        public SwTableColumnRepository Columns => new SwTableColumnRepository(this, m_ColumnsChangeTracker);

        public SwTableRowRepository Rows => CreateRows(m_RowsChangeTracker);

        private readonly ChangeTracker m_ColumnsChangeTracker;
        private readonly ChangeTracker m_RowsChangeTracker;

        protected SwTable(ITableAnnotation tableAnn, SwDocument doc, SwApplication app) : base(tableAnn?.GetAnnotation(), doc, app)
        {
            m_ColumnsChangeTracker = new ChangeTracker();
            m_RowsChangeTracker = new ChangeTracker();
            TableAnnotation = tableAnn;
        }

        protected virtual SwTableRowRepository CreateRows(ChangeTracker changeTracker)
            => new SwTableRowRepository(this, changeTracker);

        public IDataReader ExecuteReader(bool visibleOnly) 
            => new SwTableDataReader(TableAnnotation, visibleOnly);
    }

    internal class SwDrawingTable : SwTable, IXDrawingTable
    {
        internal static SwDrawingTable New(ITableAnnotation tableAnn, SwDrawing drw, SwApplication app)
            => new SwDrawingTable(tableAnn, drw, app);

        public IXObject Owner
        {
            get => m_DrwAnnWrapper.Owner;
            set => m_DrwAnnWrapper.Owner = value;
        }

        private readonly SwDrawingAnnotationWrapper m_DrwAnnWrapper;

        protected SwDrawingTable(ITableAnnotation tableAnn, SwDrawing drw, SwApplication app) : base(tableAnn, drw, app)
        {
            m_DrwAnnWrapper = new SwDrawingAnnotationWrapper(this);
        }
    }

    /// <summary>
    /// Represents the SOLIDWORKS BOM Table
    /// </summary>
    public interface ISwBomTable : ISwTable, IXBomTable 
    {
        /// <summary>
        /// Specific BOM table annotation
        /// </summary>
        IBomTableAnnotation BomTableAnnotation { get; }
        
        /// <summary>
        /// Pointer to the BOM feature
        /// </summary>
        IBomFeature BomFeature { get; }
    }

    internal class SwBomTable : SwTable, ISwBomTable
    {
        internal static new SwBomTable New(ITableAnnotation tableAnn, SwDocument doc, SwApplication app) 
        {
            if (doc is IXDrawing)
            {
                return SwDrawingBomTable.New(tableAnn, (SwDrawing)doc, app);
            }
            else
            {
                return new SwBomTable(tableAnn, doc, app);
            }
        }

        IXBomTableRowRepository IXBomTable.Rows => (IXBomTableRowRepository)base.Rows;

        public IBomTableAnnotation BomTableAnnotation => (IBomTableAnnotation)TableAnnotation;

        public IBomFeature BomFeature => BomTableAnnotation.BomFeature;

        public IXDocument3D ReferencedDocument 
        {
            get
            {
                var docPath = BomFeature.GetReferencedModelName();
                
                if (!OwnerApplication.Documents.TryGet(docPath, out var doc)) 
                {
                    doc = OwnerApplication.Documents.PreCreateFromPath(docPath);
                }

                return (IXDocument3D)doc;
            }
        }

        public IXConfiguration ReferencedConfiguration 
        {
            get 
            {
                object vis = null;
                var confName = ((string[])BomFeature.GetConfigurations(true, ref vis))?.FirstOrDefault();

                if (!string.IsNullOrEmpty(confName))
                {
                    var refDoc = ReferencedDocument;

                    IXConfiguration conf = null;

                    if (refDoc.IsCommitted)
                    {
                        refDoc.Configurations.TryGet(confName, out conf);
                    }

                    if (conf == null)
                    {
                        switch (refDoc)
                        {
                            case SwPart part:
                                conf = new SwPartConfiguration(null, part, OwnerApplication, false)
                                {
                                    Name = confName
                                };
                                break;

                            case SwAssembly assm:
                                conf = new SwAssemblyConfiguration(null, assm, OwnerApplication, false)
                                {
                                    Name = confName
                                };
                                break;

                            default:
                                throw new NotSupportedException();
                        }
                    }

                    return conf;
                }
                else
                {
                    return null;
                }
            }
        }

        protected SwBomTable(ITableAnnotation tableAnn, SwDocument doc, SwApplication app) : base(tableAnn, doc, app)
        {
        }

        protected override SwTableRowRepository CreateRows(ChangeTracker changeTracker)
            => new SwBomTableRowRepository(this, changeTracker);
    }

    internal class SwDrawingBomTable : SwBomTable, IXDrawingBomTable
    {
        internal static SwDrawingBomTable New(ITableAnnotation tableAnn, SwDrawing drw, SwApplication app)
            => new SwDrawingBomTable(tableAnn, drw, app);

        public IXObject Owner
        {
            get => m_DrwAnnWrapper.Owner;
            set => m_DrwAnnWrapper.Owner = value;
        }

        private readonly SwDrawingAnnotationWrapper m_DrwAnnWrapper;

        private SwDrawingBomTable(ITableAnnotation tableAnn, SwDrawing drw, SwApplication app) : base(tableAnn, drw, app)
        {
            m_DrwAnnWrapper = new SwDrawingAnnotationWrapper(this);
        }
    }
}
