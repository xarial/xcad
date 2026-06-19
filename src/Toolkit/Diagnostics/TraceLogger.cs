//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Runtime.InteropServices;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;

namespace Xarial.XCad.Utils.Diagnostics
{
    /// <summary>
    /// Logger logs messages to trace window
    /// </summary>
    public class TraceLogger : IXLogger
    {
        private readonly string m_Category;
        private readonly bool m_SingleLine;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="category">Logger category</param>
        /// <param name="singleLine">Split multi-line log into single lines</param>
        public TraceLogger(string category, bool singleLine = true)
        {
            m_Category = category;
            m_SingleLine = singleLine;
        }

        /// <inheritdoc/>
        public void Log(string msg, LoggerMessageSeverity_e severity = LoggerMessageSeverity_e.Information)
            => this.Trace(msg, m_Category, severity, m_SingleLine);
    }
}