//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
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
    /// Exception indicates that path of the unloaded document canot be resolved
    /// </summary>
    public class FilePathResolveFailedException : Exception, IUserException
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="inputPath">Document path</param>
        public FilePathResolveFailedException(string inputPath) : base($"Failed to resolve file path for {inputPath}")
        {
        }
    }
}
