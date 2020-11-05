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
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.SolidWorks.Documents.EventHandlers;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwConfigurationCollection : IXConfigurationRepository, IDisposable
    {
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

        public int Count => (m_Doc.Model.GetConfigurationNames() as string[]).Length;

        public IXConfiguration Active 
            => SwObject.FromDispatch<SwConfiguration>(m_Doc.Model.ConfigurationManager.ActiveConfiguration, m_Doc);

        public void AddRange(IEnumerable<IXConfiguration> ents)
        {
            throw new NotImplementedException();
        }
        
        public void Dispose()
        {
        }

        public IEnumerator<IXConfiguration> GetEnumerator() => new SwConfigurationEnumerator(m_App, m_Doc);

        public void RemoveRange(IEnumerable<IXConfiguration> ents)
        {
            throw new NotImplementedException();
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
