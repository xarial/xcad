using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Xarial.XCad.Exceptions
{
    /// <summary>
    /// Exception indicates an error with serialization and deserialization
    /// </summary>
    public class ObjectSerializationException : SerializationException, IUserException
    {
        /// <summary>
        /// CAD specific error code
        /// </summary>
        public int ErrorCode { get; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="message">User friendly error</param>
        /// <param name="errCode">CAD specific error code</param>
        public ObjectSerializationException(string message, int errCode) : base(message)
        {
            ErrorCode = errCode;
        }
    }
}
