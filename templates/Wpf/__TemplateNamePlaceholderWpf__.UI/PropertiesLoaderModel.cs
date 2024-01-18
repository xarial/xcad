using __TemplateNamePlaceholderConsole__.Base;
using System;
using System.Data;
using Xarial.XCad;
using Xarial.XCad.Base;

namespace __TemplateNamePlaceholderWpf__.UI
{
    public class PropertiesLoaderModel : IDisposable
    {
        private IPropertiesReader m_PrpsReader;

        private readonly Func<IXApplication> m_AppTemplateProvider;

        public PropertiesLoaderModel(Func<IXApplication> appTemplateProvider)
        {
            m_AppTemplateProvider = appTemplateProvider;
        }

        public DataTable Load(IXVersion version, string filePath) 
        {
            if (m_PrpsReader != null && !m_PrpsReader.Application.Version.Equals(version)) 
            {
                m_PrpsReader.Dispose();
                m_PrpsReader = null;
            }

            if (m_PrpsReader == null) 
            {
                var app = m_AppTemplateProvider.Invoke();
                app.Version = version;
                app.Commit();

                m_PrpsReader = new PropertiesReader(app);
            }

            return m_PrpsReader.Load(filePath);
        }

        public void Dispose()
        {
            if (m_PrpsReader != null) 
            {
                m_PrpsReader.Dispose();
            }
        }
    }
}
