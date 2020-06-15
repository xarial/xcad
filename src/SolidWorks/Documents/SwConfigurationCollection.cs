using SolidWorks.Interop.sldworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents;

namespace Xarial.XCad.SolidWorks.Documents
{
    public class SwConfigurationCollection : IXConfigurationRepository, IDisposable
    {
        private readonly ISldWorks m_App;
        private readonly SwDocument3D m_Doc;

        internal SwConfigurationCollection(ISldWorks app, SwDocument3D doc) 
        {
            m_App = app;
            m_Doc = doc;
        }

        public IXConfiguration this[string name]
            => SwObject.FromDispatch<SwConfiguration>((IConfiguration)m_Doc.Model.GetConfigurationByName(name), m_Doc);

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
