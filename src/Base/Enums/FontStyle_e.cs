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

namespace Xarial.XCad.Enums
{
    /// <summary>
    /// Style of <see cref="IFont"/>
    /// </summary>
    [Flags]
    public enum FontStyle_e
    {
        /// <summary>
        /// Regular
        /// </summary>
        Regular = 0,

        /// <summary>
        /// Bold
        /// </summary>
        Bold = 1,

        /// <summary>
        /// Italic
        /// </summary>
        Italic = 2,

        /// <summary>
        /// Underline
        /// </summary>
        Underline = 4,

        /// <summary>
        /// Strikeout
        /// </summary>
        Strikeout = 8
    }
}
