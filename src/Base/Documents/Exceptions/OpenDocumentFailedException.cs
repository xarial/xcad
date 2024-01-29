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

namespace Xarial.XCad.Documents.Exceptions
{
    /// <summary>
    /// Exception thrown when file cannot be opened
    /// </summary>
    public class OpenDocumentFailedException : Exception, IUserException
    {
        /// <summary>
        /// Path to the file
        /// </summary>
        public string Path { get; }
        
        /// <summary>
        /// Application specific error code
        /// </summary>
        public int ErrorCode { get; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public OpenDocumentFailedException(string path, int errorCode, string err) : base(err)
        {
            Path = path;
            ErrorCode = errorCode;
        }
    }
}
