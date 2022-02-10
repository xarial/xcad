//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Exceptions
{
    /// <summary>
    /// Error thrown when macro cannot be run
    /// </summary>
    public class MacroRunFailedException : Exception, IUserException
    {
        /// <summary>
        /// Macro file path
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Application specific error code
        /// </summary>
        public int ErrorCode { get; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="path">Path to the macro</param>
        /// <param name="errorCode">Application specific error code</param>
        /// <param name="err">User friendly error description</param>
        public MacroRunFailedException(string path, int errorCode, string err) : base(err)
        {
            Path = path;
            ErrorCode = errorCode;
        }
    }
}
