//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.UI.PropertyPage.Enums
{
    /// <summary>
    /// Style of the font
    /// </summary>
    [Flags]
    public enum FontStyle_e
    {
        /// <summary>
        /// Default style
        /// </summary>
        Default = 0,

        /// <summary>
        /// Bold font
        /// </summary>
        Bold = 1,

        /// <summary>
        /// Italic font
        /// </summary>
        Italic = 2,

        /// <summary>
        /// Underline font
        /// </summary>
        Underline = 4
    }
}
