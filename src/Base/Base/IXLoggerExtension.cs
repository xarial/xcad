//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Base
{
    public static class ILoggerExtension
    {
        /// <summary>
        /// Logs error
        /// </summary>
        /// <param name="ex">Exception</param>
        public static void Log(this IXLogger logger, Exception ex, bool stackTrace = true)
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
