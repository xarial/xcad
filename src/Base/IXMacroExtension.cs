using System.Linq;
using Xarial.XCad.Enums;
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
            macro.Run(macro.EntryPoints.First(), opts);
        }

        public static void Run(this IXMacro macro, MacroEntryPoint entryPoint)
        {
            macro.Run(entryPoint, MacroRunOptions_e.Default);
        }
    }
}
