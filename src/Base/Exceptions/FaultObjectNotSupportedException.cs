//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Exceptions
{
    /// <summary>
    /// Exception is thrown for all properties and methods of <see cref="IFaultObject"/>
    /// </summary>
    public class FaultObjectNotSupportedException : NotSupportedException
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public FaultObjectNotSupportedException() : base("Accessing methods and properties of a fault object is not supported") 
        {
        }
    }
}
