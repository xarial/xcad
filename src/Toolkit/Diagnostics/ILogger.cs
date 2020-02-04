//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Text;

namespace Xarial.XCad.Utils.Diagnostics
{
    /// <summary>
    /// Logs the trace messages
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs message
        /// </summary>
        /// <param name="msg">Message</param>
        void Log(string msg);
    }

    public static class ILoggerException
    {
        /// <summary>
        /// Logs error
        /// </summary>
        /// <param name="ex">Exception</param>
        public static void Log(this ILogger logger, Exception ex, bool stackTrace = true)
        {
            var msg = new StringBuilder();

            ParseExceptionLog(ex, msg, stackTrace);

            logger.Log(msg.ToString());
        }

        private static void ParseExceptionLog(Exception ex, StringBuilder exMsg, bool logCallStack)
        {
            exMsg.AppendLine("Exception: " + ex?.Message
                + (logCallStack ? Environment.NewLine + ex?.StackTrace : ""));

            if (ex?.InnerException != null)
            {
                ParseExceptionLog(ex.InnerException, exMsg, logCallStack);
            }
        }
    }
}