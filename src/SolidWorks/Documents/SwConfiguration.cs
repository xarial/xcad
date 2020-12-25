//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Threading;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XCad.SolidWorks.Data;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwConfiguration : ISwObject, IXConfiguration, IDisposable
    {
        new ISwCustomPropertiesCollection Properties { get; }
    }

    internal class SwConfiguration : SwObject, ISwConfiguration
    {
        private readonly IConfiguration m_Conf;
        
        private readonly SwDocument m_Doc;

        public string Name => m_Conf.Name;

        IXPropertyRepository IPropertiesOwner.Properties => Properties;

        public ISwCustomPropertiesCollection Properties => m_PropertiesLazy.Value;

        private readonly Lazy<ISwCustomPropertiesCollection> m_PropertiesLazy;

        //TODO: implement creation of new configurations
        public bool IsCommitted => true;

        public IXCutListItem[] CutLists => throw new NotImplementedException();

        internal SwConfiguration(SwDocument doc, IConfiguration conf) : base(conf)
        {
            m_Doc = doc;
            m_Conf = conf;

            m_PropertiesLazy = new Lazy<ISwCustomPropertiesCollection>(
                () => new SwConfigurationCustomPropertiesCollection(m_Doc, Name));
        }

        public void Commit(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            if (m_PropertiesLazy.IsValueCreated) 
            {
                m_PropertiesLazy.Value.Dispose();
            }
        }
    }
}