//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace Xarial.XCad.Annotations
{
    /// <summary>
    /// Represents the table cell
    /// </summary>
    public interface IXTableCell 
    {
        /// <summary>
        /// Owner row of the cell
        /// </summary>
        IXTableRow Row { get; }

        /// <summary>
        /// Owner column of the cell
        /// </summary>
        IXTableColumn Column { get; }

        /// <summary>
        /// Gets or sets the cell value
        /// </summary>
        string Value { get; set; }
    }
}
