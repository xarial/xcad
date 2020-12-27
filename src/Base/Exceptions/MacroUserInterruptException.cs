//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Exceptions
{
    /// <summary>
    /// Exception indicates that macro can be forcibly terminated by the user
    /// </summary>
    public class MacroUserInterruptException : MacroRunFailedException, IUserException
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="path">Path to the macro</param>
        /// <param name="errorCode">CAD specific error code</param>
        public MacroUserInterruptException(string path, int errorCode)
            : base(path, errorCode, "User interrupt")
        {
        }
    }
}
