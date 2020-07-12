using System;
using System.Collections.Generic;
using System.Text;

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
        void Log(string msg);
    }
}
