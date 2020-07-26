//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.SolidWorks.Data;

namespace Xarial.XCad.SolidWorks.Documents
{
    public class SwConfiguration : SwObject, IXConfiguration
    {
        private readonly IConfiguration m_Conf;
        private readonly IModelDoc2 m_Model;

        public string Name => m_Conf.Name;

        IXPropertyRepository IXConfiguration.Properties => Properties;

        public SwCustomPropertiesCollection Properties { get; }

        internal SwConfiguration(ISldWorks app, IModelDoc2 model, IConfiguration conf) : base(conf)
        {
            m_Model = model;
            m_Conf = conf;

            Properties = new SwCustomPropertiesCollection(app, m_Model, Name);
        }
    }
}