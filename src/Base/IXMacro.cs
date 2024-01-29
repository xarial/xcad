//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Enums;
using Xarial.XCad.Structures;

namespace Xarial.XCad
{
    /// <summary>
    /// Represents the macro
    /// </summary>
    public interface IXMacro
    {
        /// <summary>
        /// Path to the macro
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Available entry points of this macro
        /// </summary>
        MacroEntryPoint[] EntryPoints { get; }

        /// <summary>
        /// Run the macro
        /// </summary>
        /// <param name="entryPoint">Entry point</param>
        /// <param name="opts">Options</param>
        void Run(MacroEntryPoint entryPoint, MacroRunOptions_e opts);
    }
}
