//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Enums
{
    /// <summary>
    /// Options for running the macro via <see cref="IXMacro.Run(Structures.MacroEntryPoint, MacroRunOptions_e)"/>
    /// </summary>
    public enum MacroRunOptions_e
    {
        /// <summary>
        /// Default options
        /// </summary>
        Default,

        /// <summary>
        /// Unload macro from memory after run
        /// </summary>
        UnloadAfterRun
    }
}
