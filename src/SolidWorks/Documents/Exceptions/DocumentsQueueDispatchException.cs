//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.SolidWorks.Documents.Exceptions
{
    /// <summary>
    /// Exception indicates that some documents in the queue were not dispatched
    /// </summary>
    public class DocumentsQueueDispatchException : Exception
    {
        /// <summary>
        /// Dispatch errors
        /// </summary>
        public Exception[] Errors { get; }

        internal DocumentsQueueDispatchException(Exception[] errors) : base("Some documents in the queue were not dispatched")
        {
            Errors = errors;
        }
    }
}
