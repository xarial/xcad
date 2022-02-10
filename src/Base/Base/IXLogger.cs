//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base.Enums;

namespace Xarial.XCad.Base
{
    /// <summary>
    /// Logs the trace messages
    /// </summary>
    public interface IXLogger
    {
        /// <summary>
        /// Logs message
        /// </summary>
        /// <param name="msg">Message</param>
        /// <param name="severity">Type of the message</param>
        void Log(string msg, LoggerMessageSeverity_e severity = LoggerMessageSeverity_e.Information);
    }
}
