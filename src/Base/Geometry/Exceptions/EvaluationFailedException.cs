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
        public EvaluationFailedException() : base("Cannot perform the evaluated for this model. Make sure that model contains the valid geometry")
        {
        }
    }
}
