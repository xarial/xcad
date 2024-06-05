﻿//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

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

        public TraceLogger(string category)
        {
            m_Category = category;
        }

        public void Log(string msg, LoggerMessageSeverity_e severity = LoggerMessageSeverity_e.Information)
            => this.Trace(msg, m_Category, severity);
    }
}