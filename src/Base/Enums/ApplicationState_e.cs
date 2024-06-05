//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Enums
{
    /// <summary>
    /// Represents the state of the application
    /// </summary>
    [Flags]
    public enum ApplicationState_e
    {
        /// <summary>
        /// Default state
        /// </summary>
        Default = 0,

        /// <summary>
        /// Application window is not visible
        /// </summary>
        Hidden = 1,

        /// <summary>
        /// Application runs in the background
        /// </summary>
        Background = 2,

        /// <summary>
        /// Application runs silently
        /// </summary>
        Silent = 4,

        /// <summary>
        /// Application runs in the safe mode
        /// </summary>
        Safe = 8
    }
}
