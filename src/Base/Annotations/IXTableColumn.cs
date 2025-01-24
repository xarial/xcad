//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;

namespace Xarial.XCad.Annotations
{
    /// <summary>
    /// Represents the table column
    /// </summary>
    public interface IXTableColumn : IXTransaction
    {
        /// <summary>
        /// Index of the column
        /// </summary>
        int Index { get; set; }

        /// <summary>
        /// Gets or sets the visibility of the column
        /// </summary>
        bool Visible { get; set; }

        /// <summary>
        /// Gets or sets the title of the column
        /// </summary>
        string Title { get; set; }
    }
}
