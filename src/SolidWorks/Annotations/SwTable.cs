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

        public SwTableColumnRepository Columns => new SwTableColumnRepository(this, new ChangeTracker());

        public SwTableRowRepository Rows => CreateRows(new ChangeTracker());

        internal SwTable(ITableAnnotation tableAnn, SwDocument doc, SwApplication app) : base(tableAnn?.GetAnnotation(), doc, app)
        {
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
    }

    internal class SwBomTable : SwTable, ISwBomTable
    {
        IXBomTableRowRepository IXBomTable.Rows => (IXBomTableRowRepository)base.Rows;

        internal SwBomTable(ITableAnnotation tableAnn, SwDocument doc, SwApplication app) : base(tableAnn, doc, app)
        {
        }

        protected override SwTableRowRepository CreateRows(ChangeTracker changeTracker)
            => new SwBomTableRowRepository(this, changeTracker);
    }
}
