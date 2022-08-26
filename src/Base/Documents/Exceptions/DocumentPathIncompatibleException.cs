//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Documents.Exceptions
{
    /// <summary>
    /// Indicates that the path of <see cref="IXDocument"/> cannot be set to the specific type of the document
    /// </summary>
    public class DocumentPathIncompatibleException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public DocumentPathIncompatibleException(IXDocument doc) : base($"Incompatible path for the document of type '{doc.GetType()}'")
        {
        }
    }
}
