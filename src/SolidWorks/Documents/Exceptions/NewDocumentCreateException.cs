using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Exceptions;

namespace Xarial.XCad.SolidWorks.Documents.Exceptions
{
    /// <summary>
    /// Exception indicates that new document cannot be created
    /// </summary>
    public class NewDocumentCreateException : Exception, IUserException
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="templateName">Name of the document template</param>
        public NewDocumentCreateException(string templateName) 
            : base($"Failed to create new document from the template: {templateName}")
        {
        }
    }
}
