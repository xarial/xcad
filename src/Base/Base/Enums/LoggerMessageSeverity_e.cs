//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Base.Enums
{
    /// <summary>
    /// Type of the logger message
    /// </summary>
    public enum LoggerMessageSeverity_e
    {
        /// <summary>
        /// Information message
        /// </summary>
        Information,

        /// <summary>
        /// Warning message
        /// </summary>
        Warning,

        /// <summary>
        /// Error message
        /// </summary>
        Error,

        /// <summary>
        /// Represents the fatal error
        /// </summary>
        Fatal,

        /// <summary>
        /// Represents debug information
        /// </summary>
        Debug
    }
}
