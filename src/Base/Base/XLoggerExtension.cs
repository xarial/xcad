//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Xarial.XCad.Base.Enums;

namespace Xarial.XCad.Base
{
    /// <summary>
    /// Additional extension methods for the logger
    /// </summary>
    public static class XLoggerExtension
    {
        /// <summary>
        /// Prints the message to trace
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="msg">Message to trace</param>
        /// <param name="category">Trace category</param>
        /// <param name="severity"></param>
        /// <param name="singleLine">True to merge multiline into a single line</param>
        public static void Trace(this IXLogger logger, string msg, string category, LoggerMessageSeverity_e severity, bool singleLine = false)
        {
            if (singleLine)
            {
                msg = Regex.Replace(msg, @"\r\n?|\n", " :: ");
            }

            System.Diagnostics.Trace.WriteLine($"[{severity}]{msg}", category);
        }

        /// <summary>
        /// Logs error
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="ex">Exception</param>
        /// <param name="stackTrace">True to log stack trace</param>
        /// <param name="severity">Severity of the message</param>
        public static void Log(this IXLogger logger, Exception ex, bool stackTrace = true, LoggerMessageSeverity_e severity = LoggerMessageSeverity_e.Error)
        {
            var msg = new StringBuilder();
            var stackTraceMsg = new StringBuilder();

            ParseExceptionLog(ex, msg, stackTraceMsg, stackTrace);

            logger.Log(msg.ToString(), severity);

            if (stackTrace) 
            {
                logger.Log(stackTraceMsg.ToString(), LoggerMessageSeverity_e.Debug);
            }
        }

        private static void ParseExceptionLog(Exception ex, StringBuilder exMsg, StringBuilder stackTraceMsg, bool logCallStack)
        {
            exMsg.AppendLine("Exception: " + ex?.Message);

            if (logCallStack)
            {
                stackTraceMsg.AppendLine(ex?.StackTrace);
            }

            if (ex?.InnerException != null)
            {
                ParseExceptionLog(ex.InnerException, exMsg, stackTraceMsg, logCallStack);
            }
        }
    }
}
