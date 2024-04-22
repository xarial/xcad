//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
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
using System.Windows.Documents;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
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

    internal class SwTable : SwAnnotation, ISwTable
    {
        IXTableRowRepository IXTable.Rows => Rows;
        IXTableColumnRepository IXTable.Columns => Columns;

        public ITableAnnotation TableAnnotation { get; }

        public SwTableColumnRepository Columns => new SwTableColumnRepository(this, m_ColumnsChangeTracker);

        public SwTableRowRepository Rows => CreateRows(m_RowsChangeTracker);

        private readonly ChangeTracker m_ColumnsChangeTracker;
        private readonly ChangeTracker m_RowsChangeTracker;

        internal SwTable(ITableAnnotation tableAnn, SwDocument doc, SwApplication app) : base(tableAnn?.GetAnnotation(), doc, app)
        {
            m_ColumnsChangeTracker = new ChangeTracker();
            m_RowsChangeTracker = new ChangeTracker();
            TableAnnotation = tableAnn;
        }

        protected virtual SwTableRowRepository CreateRows(ChangeTracker changeTracker)
            => new SwTableRowRepository(this, changeTracker);
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

        internal SwBomTable(ITableAnnotation tableAnn, SwDocument doc, SwApplication app) : base(tableAnn, doc, app)
        {
        }

        protected override SwTableRowRepository CreateRows(ChangeTracker changeTracker)
            => new SwBomTableRowRepository(this, changeTracker);
    }
}
