//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
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
    public interface IXMacro
    {
        string Path { get; }
        MacroEntryPoint[] EntryPoints { get; }
        void Run(MacroEntryPoint entryPoint, MacroRunOptions_e opts);
    }
}
