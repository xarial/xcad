//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;

namespace Xarial.XCad.Annotations
{
    /// <summary>
    /// Represents cells of the <see cref="IXTableRow"/>
    /// </summary>
    public interface IXTableCells : IEnumerable<IXTableCell> 
    {
        /// <summary>
        /// Returns cell by column index
        /// </summary>
        /// <param name="colIndex">Column index</param>
        /// <returns>Cell</returns>
        IXTableCell this[int colIndex] { get; }
    }
}
