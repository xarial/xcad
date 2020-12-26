//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.SolidWorks.Documents.EventHandlers;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwConfigurationCollection : IXConfigurationRepository, IDisposable
    {
        new ISwConfiguration PreCreate();
        new ISwConfiguration Active { get; set; }
    }

    internal class SwConfigurationCollection : ISwConfigurationCollection
    {
        public event ConfigurationActivatedDelegate ConfigurationActivated 
        {
            add 
            {
                m_ConfigurationActivatedEventsHandler.Attach(value);
            }
            remove 
            {
                m_ConfigurationActivatedEventsHandler.Detach(value);
            }
        }

        IXConfiguration IXConfigurationRepository.Active 
        {
            get => Active;
            set => Active = (ISwConfiguration)value;
        }

        IXConfiguration IXConfigurationRepository.PreCreate() => PreCreate();

        private readonly ISldWorks m_App;
        private readonly SwDocument3D m_Doc;

        private readonly ConfigurationActivatedEventsHandler m_ConfigurationActivatedEventsHandler;

        internal SwConfigurationCollection(ISldWorks app, SwDocument3D doc) 
        {
            m_App = app;
            m_Doc = doc;
            m_ConfigurationActivatedEventsHandler = new ConfigurationActivatedEventsHandler(doc);
        }

        public IXConfiguration this[string name] => this.Get(name);

        public bool TryGet(string name, out IXConfiguration ent)
        {
            var conf = m_Doc.Model.GetConfigurationByName(name);

            if (conf != null)
            {
                ent = SwObject.FromDispatch<SwConfiguration>((IConfiguration)conf, m_Doc);
                return true;
            }
            else 
            {
                ent = null;
                return false;
            }
        }

        public int Count => m_Doc.Model.GetConfigurationCount();

        public ISwConfiguration Active
        {
            get => SwObject.FromDispatch<SwConfiguration>(m_Doc.Model.ConfigurationManager.ActiveConfiguration, m_Doc);
            set 
            {
                if (m_Doc.Model.ConfigurationManager.ActiveConfiguration != value.Configuration)
                {
                    if (!m_Doc.Model.ShowConfiguration2(value.Name))
                    {
                        throw new Exception($"Failed to activate configuration '{value.Name}'");
                    }
                }
            }
        }

        public ISwConfiguration PreCreate() => new SwConfiguration(m_Doc, null, false);

        public void AddRange(IEnumerable<IXConfiguration> ents)
        {
            foreach (var conf in ents) 
            {
                conf.Commit();
            }
        }
        
        public void Dispose()
        {
        }

        public IEnumerator<IXConfiguration> GetEnumerator() => new SwConfigurationEnumerator(m_App, m_Doc);

        public void RemoveRange(IEnumerable<IXConfiguration> ents)
        {
            foreach (var conf in ents) 
            {
                if (conf.IsCommitted)
                {
                    if (Count == 1) 
                    {
                        throw new Exception("Cannot delete the last configuration");
                    }

                    if (string.Equals(Active.Name, conf.Name)) 
                    {
                        Active = (ISwConfiguration)this.First(c => !string.Equals(c.Name, conf.Name, StringComparison.CurrentCultureIgnoreCase));
                    }

                    if (!m_Doc.Model.DeleteConfiguration2(conf.Name)) 
                    {
                        throw new Exception($"Failed to delete configuration '{conf.Name}'");
                    }
                }
                else 
                {
                    throw new Exception($"Cannot delete uncommited configuration '{conf.Name}'");
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal class SwConfigurationEnumerator : IEnumerator<IXConfiguration>
    {
        public IXConfiguration Current
            => SwObject.FromDispatch<SwConfiguration>((IConfiguration) m_Doc.Model.GetConfigurationByName(m_ConfNames[m_CurConfIndex]), m_Doc);

        object IEnumerator.Current => Current;

        private int m_CurConfIndex;

        private readonly ISldWorks m_App;
        private readonly SwDocument3D m_Doc;

        private string[] m_ConfNames;

        internal SwConfigurationEnumerator(ISldWorks app, SwDocument3D doc)
        {
            m_App = app;
            m_Doc = doc;

            m_CurConfIndex = -1;
            m_ConfNames = (string[])m_Doc.Model.GetConfigurationNames();
        }

        public bool MoveNext()
        {
            m_CurConfIndex++;
            return m_CurConfIndex < m_ConfNames.Length;
        }

        public void Reset()
        {
            m_CurConfIndex = -1;
        }

        public void Dispose()
        {
        }
    }
}
