//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Documents.Exceptions
{
    /// <summary>
    /// Exception indicates that document cannot be opened for write access
    /// </summary>
    public class DocumentWriteAccessDeniedException : OpenDocumentFailedException
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public DocumentWriteAccessDeniedException(string path, int errorCode) 
            : base(path, errorCode, "File is read-only and cannot be opened for write access")
        {
        }
    }
}