using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Geometry.Exceptions
{
    /// <summary>
    /// This exception is thrown as the result of <see cref="IXBody.Add(IXBody)"/>, or <see cref="IXBody.Common(IXBody)"/> or <see cref="IXBody.Substract(IXBody)"/> if bodies do not intersect
    /// </summary>
    public class BodyBooleanOperationNoIntersectException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public BodyBooleanOperationNoIntersectException() 
            : base("Multiple bodies are created as the result of Add operation. This indicates that bodies do not intersect") 
        {
        }
    }
}
