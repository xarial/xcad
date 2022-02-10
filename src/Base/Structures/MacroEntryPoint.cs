//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Structures
{
    /// <summary>
    /// Represents the entty point of the macro
    /// </summary>
    public class MacroEntryPoint
    {
        /// <summary>
        /// Module name for the entry point
        /// </summary>
        public string ModuleName { get; }

        /// <summary>
        /// Name of the procedure defined as an entry point
        /// </summary>
        public string ProcedureName { get; }

        /// <summary>
        /// Default constructor for entry point
        /// </summary>
        /// <param name="moduleName">Module name</param>
        /// <param name="procName">Procedure name</param>
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
