//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.SolidWorks.Exceptions
{
    public class MacroRunException : Exception
    {
        internal MacroRunException(string path, swRunMacroError_e err) : base($"Failed to run macro '{path}'. Error Code: {err}") 
        {
        }
    }
}
