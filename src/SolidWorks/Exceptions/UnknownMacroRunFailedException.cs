//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Exceptions;

namespace Xarial.XCad.SolidWorks.Exceptions
{
    /// <summary>
    /// This error indicates that macro failed to run with unknown reason
    /// </summary>
    /// <remarks>This error migth indicate that the process now is corrupted</remarks>
    public class UnknownMacroRunFailedException : MacroRunFailedException, ICriticalException, IUserException
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="path">Path to the macro</param>
        public UnknownMacroRunFailedException(string path) : base(path, -1, "Unknown macro run error")
        {
        }
    }
}
