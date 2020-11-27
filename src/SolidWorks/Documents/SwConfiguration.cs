//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System.Threading;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.SolidWorks.Data;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwConfiguration : ISwObject, IXConfiguration 
    {
        new ISwCustomPropertiesCollection Properties { get; }
    }

    internal class SwConfiguration : SwObject, ISwConfiguration
    {
        private readonly IConfiguration m_Conf;
        
        private readonly SwDocument m_Doc;

        public string Name => m_Conf.Name;

        IXPropertyRepository IXConfiguration.Properties => Properties;

        public ISwCustomPropertiesCollection Properties { get; }

        //TODO: implement creation of new configurations
        public bool IsCommitted => true;

        internal SwConfiguration(SwDocument doc, IConfiguration conf) : base(conf)
        {
            m_Doc = doc;
            m_Conf = conf;

            Properties = new SwCustomPropertiesCollection(m_Doc, Name);
        }

        public void Commit(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}