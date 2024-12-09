//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Geometry.Exceptions
{
    /// <summary>
    /// This exception is thrown as the result of <see cref="IXBody.Add(IXBody)"/>, or <see cref="IXBody.Common(IXBody)"/> or <see cref="IXBody.Subtract(IXBody)"/> if bodies do not intersect
    /// </summary>
    public class BodyBooleanOperationNoIntersectException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public BodyBooleanOperationNoIntersectException() 
            : base("Multiple bodies are produced as the result of boolean operation. This indicates that bodies do not intersect") 
        {
        }
    }
}
