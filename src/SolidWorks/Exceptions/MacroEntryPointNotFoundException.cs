//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Structures;

namespace Xarial.XCad.SolidWorks.Exceptions
{
    /// <summary>
    /// Indicates that specified entry point to run the macro is not found
    /// </summary>
    public class MacroEntryPointNotFoundException : Exception, IUserException
    {
        internal MacroEntryPointNotFoundException(string macroPath, MacroEntryPoint entryPoint)
            : base($"Entry point '{entryPoint}' is not available in the macro '{macroPath}'")
        {
        }
    }
}
