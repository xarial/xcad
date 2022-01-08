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
using Xarial.XCad.Geometry.Exceptions;

namespace Xarial.XCad.SolidWorks.Geometry.Exceptions
{
    /// <summary>
    /// Exception indicates that calculation of mass properties has failed
    /// </summary>
    public class InvalidMassPropertyCalculationException : EvaluationFailedException, IUserException
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public InvalidMassPropertyCalculationException() : base("Invalid mass properties calculation. Make sure that model contains the valid geometry or try rebuilding the model") 
        {
        }
    }
}
