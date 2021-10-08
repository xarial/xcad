//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Documents.Enums
{
    /// <summary>
    /// Represents the state of the document
    /// </summary>
    [Flags]
    public enum DocumentState_e
    {
        /// <summary>
        /// Default state of the document
        /// </summary>
        Default = 0,

        /// <summary>
        /// Checks if document is hidden
        /// </summary>
        Hidden = 1,

        /// <summary>
        /// Opens document in read-only mode
        /// </summary>
        ReadOnly = 2,

        /// <summary>
        /// Opens document in view only mode
        /// </summary>
        ViewOnly = 4,

        /// <summary>
        /// Opens document without displaying any popup messages
        /// </summary>
        Silent = 8,

        /// <summary>
        /// Opens document in the rapid mode
        /// </summary>
        /// <remarks>This mode significantly improves the performance of opening but certain functionality and API migth not be available</remarks>
        Rapid = 16,

        /// <summary>
        /// Opens document in lightweigth mode
        /// </summary>
        Lightweight = 32
    }
}
