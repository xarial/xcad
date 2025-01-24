//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;
using Xarial.XCad.Documents;

namespace Xarial.XCad.Annotations
{
    /// <summary>
    /// Represents row of the table
    /// </summary>
    public interface IXTableRow : IXTransaction
    {
        /// <summary>
        /// Index of the row in the table
        /// </summary>
        int Index { get; set; }

        /// <summary>
        /// Visibility of the row
        /// </summary>
        bool Visible { get; set; }

        /// <summary>
        /// Cells of this row
        /// </summary>
        IXTableCells Cells { get; }
    }

    /// <summary>
    /// Represents the row of the <see cref="IXBomTable"/>
    /// </summary>
    public interface IXBomTableRow : IXTableRow 
    {
        /// <summary>
        /// Gets or sets an item number of BOM row
        /// </summary>
        /// <remarks>Use <see cref="BomItemNumber"/> for available values</remarks>
        int? ItemNumber { get; set; }

        /// <summary>
        /// Components of this BOM row
        /// </summary>
        IXComponent[] Components { get; }
    }

    /// <summary>
    /// BOM row item number
    /// </summary>
    public static class BomItemNumber 
    {
        /// <summary>
        /// Automatic item number
        /// </summary>
        public static int? Auto { get; } = -1;

        /// <summary>
        /// No item number in the row
        /// </summary>
        public static int? None { get; } = null;
    }
}
