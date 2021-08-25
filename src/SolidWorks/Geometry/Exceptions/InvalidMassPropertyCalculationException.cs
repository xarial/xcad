using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Exceptions;

namespace Xarial.XCad.SolidWorks.Geometry.Exceptions
{
    public class InvalidMassPropertyCalculationException : Exception, IUserException
    {
        public InvalidMassPropertyCalculationException() : base("Invalid mass properties calculation") 
        {
        }
    }
}
