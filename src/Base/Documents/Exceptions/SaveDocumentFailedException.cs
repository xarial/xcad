using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Documents.Exceptions
{
    /// <summary>
    /// Exception indicates that document failed to save
    /// </summary>
    public class SaveDocumentFailedException : Exception
    {
        /// <summary>
        /// CAD specific error code
        /// </summary>
        public int ErrorCode { get; }

        /// <summary>
        /// Exception constructor
        /// </summary>
        /// <param name="errCode">Error code</param>
        /// <param name="errorDesc">Description</param>
        public SaveDocumentFailedException(int errCode, string errorDesc)
            : base(errorDesc)
        {
            ErrorCode = errCode;
        }
    }
}
