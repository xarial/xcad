//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Exceptions;

namespace Xarial.XCad.Geometry.Exceptions
{
    /// <summary>
    /// Indicates that mass property cannot be created
    /// </summary>
    public class EvaluationFailedException : NullReferenceException, IUserException
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public EvaluationFailedException() : base("Cannot perform the evaluation for this model. Make sure that model contains the valid geometry")
        {
        }

        /// <summary>
        /// Specific evaluation exception
        /// </summary>
        /// <param name="error">Error description</param>
        public EvaluationFailedException(string error) : base(error)
        {
        }
    }
}
