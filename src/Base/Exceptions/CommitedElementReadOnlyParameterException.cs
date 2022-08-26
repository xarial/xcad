//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Exceptions
{
    /// <summary>
    /// Exception indicates that parameter of <see cref="Base.IXTransaction"/> cannot be modified after the commit
    /// </summary>
    public class CommitedElementReadOnlyParameterException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public CommitedElementReadOnlyParameterException() : base("Parameter cannot be modified after element is committed")
        {
        }

        /// <summary>
        /// Constructor with custom message
        /// </summary>
        /// <param name="message">Custom message</param>
        public CommitedElementReadOnlyParameterException(string message) : base(message)
        {
        }
    }
}
