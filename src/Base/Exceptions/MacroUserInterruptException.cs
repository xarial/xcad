using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Exceptions
{
    public class MacroUserInterruptException : MacroRunFailedException
    {
        public MacroUserInterruptException(string path, int errorCode)
            : base(path, errorCode, "User interrupt")
        {
        }
    }
}
