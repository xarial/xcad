using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Exceptions;

namespace Xarial.XCad.SolidWorks.Geometry.Exceptions
{
    /// <summary>
    /// Indicates that mass property cannot be created
    /// </summary>
    public class MassPropertyNotAvailableException : NullReferenceException, IUserException
    {
        public MassPropertyNotAvailableException() : base("Mass Properties cannot be evaluated for this model. Make sure that model contains geometry")
        {
        }
    }
}
