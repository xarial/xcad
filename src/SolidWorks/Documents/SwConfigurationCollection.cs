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
        private readonly IModelDoc2 m_Model;

        internal SwConfigurationCollection(ISldWorks app, IModelDoc2 model) 
        {
            m_App = app;
            m_Model = model;
        }

        public IXConfiguration this[string name] => new SwConfiguration(m_App, m_Model, (IConfiguration)m_Model.GetConfigurationByName(name));

        public int Count => (m_Model.GetConfigurationNames() as string[]).Length;

        public IXConfiguration Active => new SwConfiguration(m_App, m_Model, m_Model.ConfigurationManager.ActiveConfiguration);

        public void AddRange(IEnumerable<IXConfiguration> ents)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }

        public IEnumerator<IXConfiguration> GetEnumerator() => new SwConfigurationEnumerator(m_App, m_Model);

        public void RemoveRange(IEnumerable<IXConfiguration> ents)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal class SwConfigurationEnumerator : IEnumerator<IXConfiguration>
    {
        public IXConfiguration Current =>
            new SwConfiguration(m_App, m_Model, (IConfiguration)m_Model.GetConfigurationByName(m_ConfNames[m_CurConfIndex]));

        object IEnumerator.Current => Current;

        private int m_CurConfIndex;

        private readonly ISldWorks m_App;
        private readonly IModelDoc2 m_Model;

        private string[] m_ConfNames;

        internal SwConfigurationEnumerator(ISldWorks app, IModelDoc2 model)
        {
            m_App = app;
            m_Model = model;

            m_CurConfIndex = -1;
            m_ConfNames = (string[])m_Model.GetConfigurationNames();
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
