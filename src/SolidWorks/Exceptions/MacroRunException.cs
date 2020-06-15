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
