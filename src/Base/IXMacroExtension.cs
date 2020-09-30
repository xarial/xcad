//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Linq;
using Xarial.XCad.Enums;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Structures;

namespace Xarial.XCad
{
    public static class IXMacroExtension
    {
        public static void Run(this IXMacro macro) 
        {
            Run(macro, MacroRunOptions_e.Default);
        }

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

        public static void Run(this IXMacro macro, MacroEntryPoint entryPoint)
        {
            macro.Run(entryPoint, MacroRunOptions_e.Default);
        }
    }
}
