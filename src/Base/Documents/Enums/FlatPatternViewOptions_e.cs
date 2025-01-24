//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad.Documents.Enums
{
    /// <summary>
    /// Options of the <see cref="IXFlatPatternDrawingView"/> drawing view
    /// </summary>
    [Flags]
    public enum FlatPatternViewOptions_e
    {
        /// <summary>
        /// Empty flat pattern view
        /// </summary>
        None = 0,

        /// <summary>
        /// Shows the bend lines in the drawing view
        /// </summary>
        BendLines = 1,

        /// <summary>
        /// Shows the bending notes in the drawing view
        /// </summary>
        BendNotes = 2
    }
}
