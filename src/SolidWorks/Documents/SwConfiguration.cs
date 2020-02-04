//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using Xarial.XCad.Documents;

namespace Xarial.XCad.SolidWorks.Documents
{
    public class SwConfiguration : IXConfiguration
    {
        private readonly IConfiguration m_Conf;

        public string Name => m_Conf.Name;

        internal SwConfiguration(IConfiguration conf)
        {
            m_Conf = conf;
        }
    }
}