//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.SolidWorks.Documents.Exceptions
{
    /// <summary>
    /// Indicates that document is already opened
    /// </summary>
    public class DocumentAlreadyOpenedException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">Path to a document</param>
        public DocumentAlreadyOpenedException(string path) : base($"{path} document already opened") 
        {
        }
    }
}
