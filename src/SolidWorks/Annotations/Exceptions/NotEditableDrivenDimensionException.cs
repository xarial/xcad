//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Exceptions;

namespace Xarial.XCad.SolidWorks.Annotations.Exceptions
{
    /// <summary>
    /// Indicates that the value of the driven dimension cannot be changed
    /// </summary>
    public class NotEditableDrivenDimensionException : Exception, IUserException
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public NotEditableDrivenDimensionException() : base("Value of the driven dimension cannot be changed") 
        {
        }
    }
}
