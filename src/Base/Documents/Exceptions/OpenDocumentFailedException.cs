using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Documents.Exceptions
{
    /// <summary>
    /// Exception thrown when file cannot be opened
    /// </summary>
    public class OpenDocumentFailedException : Exception
    {
        /// <summary>
        /// Path to the file
        /// </summary>
        public string Path { get; }
        
        /// <summary>
        /// Application specific error code
        /// </summary>
        public int ErrorCode { get; }

        public OpenDocumentFailedException(string path, int errorCode, string err) : base(err)
        {
            Path = path;
            ErrorCode = errorCode;
        }
    }
}
