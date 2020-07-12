//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;

namespace Xarial.XCad.Utils.Diagnostics
{
    public class TraceLogger : IXLogger
    {
        private readonly string m_Category;

        public TraceLogger(string category)
        {
            m_Category = category;
        }

        public void Log(string msg)
        {
            System.Diagnostics.Trace.WriteLine(msg, m_Category);
        }
    }
}