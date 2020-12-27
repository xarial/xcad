//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Linq;
using System.Threading;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Data;
using Xarial.XCad.SolidWorks.Documents.Exceptions;
using Xarial.XCad.SolidWorks.Features;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwConfiguration : ISwObject, IXConfiguration, IDisposable
    {
        IConfiguration Configuration { get; }
        new ISwCustomPropertiesCollection Properties { get; }
    }

    internal class SwConfiguration : SwObject, ISwConfiguration
    {
        public IConfiguration Configuration => m_Creator.Element;

        private readonly SwDocument3D m_Doc;

        public string Name
        {
            get
            {
                if (m_Creator.IsCreated)
                {
                    return Configuration.Name;
                }
                else
                {
                    return m_Creator.CachedProperties.Get<string>();
                }
            }
            set
            {
                if (m_Creator.IsCreated)
                {
                    Configuration.Name = value;
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        IXPropertyRepository IPropertiesOwner.Properties => Properties;

        public ISwCustomPropertiesCollection Properties => m_PropertiesLazy.Value;

        private readonly Lazy<ISwCustomPropertiesCollection> m_PropertiesLazy;

        public bool IsCommitted => m_Creator.IsCreated;

        public IXCutListItem[] CutLists
        {
            get
            {
                var activeConf = m_Doc.Configurations.Active;

                if (activeConf.Configuration != this.Configuration) 
                {
                    throw new InactiveConfigurationCutListPropertiesNotSupportedException();
                };

                return m_Doc.Features.GetCutLists();
            }
        }

        private readonly ElementCreator<IConfiguration> m_Creator;

        internal SwConfiguration(SwDocument3D doc, IConfiguration conf, bool created) : base(conf)
        {
            m_Doc = doc;

            m_Creator = new ElementCreator<IConfiguration>(Create, conf, created);

            m_PropertiesLazy = new Lazy<ISwCustomPropertiesCollection>(
                () => new SwConfigurationCustomPropertiesCollection(m_Doc, Name));
        }

        public void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);

        private IConfiguration Create(CancellationToken cancellationToken) 
        {
            IConfiguration conf;

            if (m_Doc.App.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2018))
            {
                conf = m_Doc.Model.ConfigurationManager.AddConfiguration2(Name, "", "", (int)swConfigurationOptions2_e.swConfigOption_DontActivate, "", "", false);
            }
            else 
            {
                conf = m_Doc.Model.ConfigurationManager.AddConfiguration(Name, "", "", (int)swConfigurationOptions2_e.swConfigOption_DontActivate, "", "");
            }

            if (conf == null) 
            {
                throw new Exception("Failed to create configuration");
            }

            return conf;
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