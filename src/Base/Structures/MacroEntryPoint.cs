//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Structures
{
    public class MacroEntryPoint
    {
        public string ModuleName { get; }
        public string ProcedureName { get; }

        public MacroEntryPoint(string moduleName, string procName)
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
}
