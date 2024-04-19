//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;

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
        /// <remarks>Null if row does not have an item number</remarks>
        int? ItemNumber { get; set; }
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
        public static int? NoNumber { get; } = null;
    }
}
