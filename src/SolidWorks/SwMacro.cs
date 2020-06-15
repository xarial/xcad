using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.SolidWorks.Exceptions;

namespace Xarial.XCad.SolidWorks
{
    public class MacroEntryPoint
    {
        public string ModuleName { get; }
        public string ProcedureName { get; }

        internal MacroEntryPoint(string moduleName, string procName) 
        {
            ModuleName = moduleName;
            ProcedureName = procName;
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(ModuleName))
            {
                return $"{ModuleName}.{ProcedureName}";
            }
            else
            {
                return ProcedureName;
            }
        }
    }

    public abstract class SwMacro : IXMacro 
    {
        protected readonly ISldWorks m_App;
        protected readonly string m_Path;

        internal SwMacro(ISldWorks app, string path)
        {
            m_App = app;
            m_Path = path;
        }

        protected abstract MacroEntryPoint EntryPoint { get; }
        protected abstract swRunMacroOption_e Options { get; }

        public void Run()
        {
            int err;
            if (!m_App.RunMacro2(m_Path, EntryPoint.ModuleName, EntryPoint.ProcedureName, (int)Options, out err)) 
            {
                throw new MacroRunException(m_Path, (swRunMacroError_e)err);
            }
        }
    }

    public class SwVbaMacro : SwMacro
    {
        private class MacroEntryPointComparer : IComparer<MacroEntryPoint>
        {
            private const string MAIN_NAME = "main";

            public int Compare(MacroEntryPoint x, MacroEntryPoint y)
            {
                if (object.ReferenceEquals(x, y) || x == null || y == null)
                {
                    return 0;
                }

                if (string.Equals(x.ProcedureName, MAIN_NAME, StringComparison.CurrentCultureIgnoreCase))
                {
                    return -1;
                }
                else if (string.Equals(y.ProcedureName, MAIN_NAME, StringComparison.CurrentCultureIgnoreCase))
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        private class MacroEntryPointEqualityComparer : IEqualityComparer<MacroEntryPoint>
        {
            public bool Equals(MacroEntryPoint x, MacroEntryPoint y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (x == null || y == null)
                {
                    return false;
                }
                
                return string.Equals(x.ModuleName, y.ModuleName, StringComparison.CurrentCultureIgnoreCase)
                    && string.Equals(x.ProcedureName, y.ProcedureName, StringComparison.CurrentCultureIgnoreCase);
            }

            public int GetHashCode(MacroEntryPoint obj)
            {
                return 0;
            }
        }

        private MacroEntryPoint m_EntryPoint;
        private swRunMacroOption_e m_Options;

        public MacroEntryPoint[] EntryPoints { get; }

        internal SwVbaMacro(ISldWorks app, string path) : base(app, path)
        {
            m_Options = swRunMacroOption_e.swRunMacroDefault;
            EntryPoints = GetEntryPoints();
            m_EntryPoint = EntryPoints.First();
        }

        public void Run(swRunMacroOption_e options) 
        {
            Run(EntryPoint, options);
        }

        public void Run(MacroEntryPoint entryPoint)
        {
            Run(entryPoint, m_Options);
        }

        public void Run(MacroEntryPoint entryPoint, swRunMacroOption_e options)
        {
            m_Options = options;

            if (EntryPoints.Contains(entryPoint, new MacroEntryPointEqualityComparer()))
            {
                m_EntryPoint = entryPoint;
                base.Run();
            }
            else 
            {
                throw new Exception($"Entry point '{entryPoint}' is not available in the macro '{m_Path}'");
            }
        }

        protected override swRunMacroOption_e Options => m_Options;

        protected override MacroEntryPoint EntryPoint => m_EntryPoint;

        private MacroEntryPoint[] GetEntryPoints()
        {
            var methods = m_App.GetMacroMethods(m_Path,
                (int)swMacroMethods_e.swMethodsWithoutArguments) as string[];

            if (methods != null)
            {
                return methods.Select(m =>
                {
                    var ep = m.Split('.');
                    return new MacroEntryPoint(ep[0], ep[1]);
                }).OrderBy(e => e, new MacroEntryPointComparer()).ToArray();
            }

            return null;
        }
    }

    public class SwVstaMacro : SwMacro
    {
        internal SwVstaMacro(ISldWorks app, string path) : base(app, path)
        {
        }

        protected override swRunMacroOption_e Options => swRunMacroOption_e.swRunMacroDefault;
        protected override MacroEntryPoint EntryPoint => new MacroEntryPoint("", "Main");
    }
}
