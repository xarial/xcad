using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Documents.Exceptions
{
    /// <summary>
    /// Indicates that the path of <see cref="IXDocument"/> cannot be set to the specific type of the document
    /// </summary>
    public class DocumentPathMismatchException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public DocumentPathMismatchException(IXDocument doc, string reason) : base($"Incorrect path for the document of type '{doc.GetType()}': {reason}")
        {
        }
    }
}
