//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base;

namespace Xarial.XCad.Exceptions
{
    /// <summary>
    /// Exception indicates that element cannot be accessed as <see cref="IXTransaction.IsCommitted"/> is False
    /// </summary>
    public class NonCommittedElementAccessException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public NonCommittedElementAccessException()
            : base("This is a template feature and has not been created yet. Commit this feature by adding to the feature collection")
        {
        }
    }
}
