//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Exceptions;

namespace Xarial.XCad.SolidWorks.Geometry.Exceptions
{
    /// <summary>
    /// Exception indicates that calculation of mass properties has failed
    /// </summary>
    public class InvalidMassPropertyCalculationException : Exception, IUserException
    {
        public InvalidMassPropertyCalculationException() : base("Invalid mass properties calculation. Try rebuilding the model") 
        {
        }
    }
}
