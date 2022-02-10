//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Linq;
using Xarial.XCad.Enums;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Structures;

namespace Xarial.XCad
{
    /// <summary>
    /// Additional methods of <see cref="IXMacro"/>
    /// </summary>
    public static class IXMacroExtension
    {
        /// <summary>
        /// Run macro with default entry point and default options
        /// </summary>
        /// <param name="macro">Macro to run</param>
        public static void Run(this IXMacro macro) 
            => Run(macro, MacroRunOptions_e.Default);

        /// <summary>
        /// Run macro with default entry point and specified options
        /// </summary>
        /// <param name="macro">Macro to run</param>
        /// <param name="opts">macro options</param>
        public static void Run(this IXMacro macro, MacroRunOptions_e opts)
        {
            if (macro.EntryPoints?.Any() == true)
            {
                macro.Run(macro.EntryPoints.First(), opts);
            }
            else 
            {
                throw new MacroRunFailedException(macro.Path, -1, "Macro contains no entry points");
            }
        }

        /// <summary>
        /// Run macro with specirfied entry point and default options
        /// </summary>
        /// <param name="macro">Macro to run</param>
        /// <param name="entryPoint">Entry point</param>
        public static void Run(this IXMacro macro, MacroEntryPoint entryPoint)
            => macro.Run(entryPoint, MacroRunOptions_e.Default);
    }
}
