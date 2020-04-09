using NUnit.Framework;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xarial.XCad.SolidWorks;

namespace SolidWorks.Tests.Integration
{
    public abstract class IntegrationTests
    {
        private class DocumentWrapper : IDisposable
        {
            private readonly ISldWorks m_App;
            private readonly IModelDoc2 m_Model;

            private bool m_IsDisposed;

            internal DocumentWrapper(ISldWorks app, IModelDoc2 model) 
            {
                m_App = app;
                m_Model = model;
                m_IsDisposed = false;
            }

            public void Dispose()
            {
                if (!m_IsDisposed)
                {
                    m_App.CloseDoc(m_Model.GetTitle());
                    m_IsDisposed = true;
                }
            }
        }

        private const int SW_PRC_ID = 18784;
        private const string DATA_FOLDER = @"C:\Users\artem\OneDrive\xCAD\TestData";

        protected SwApplication m_App;
        private ISldWorks m_SwApp;

        private List<IDisposable> m_Disposables;

        [SetUp]
        public void Setup()
        {
            m_App = SwApplication.FromProcess(SW_PRC_ID);
            m_SwApp = m_App.Sw;
            m_Disposables = new List<IDisposable>();
        }

        protected IDisposable OpenDataDocument(string name, bool readOnly = true) 
        {
            var filePath = Path.Combine(DATA_FOLDER, name);

            var spec = (IDocumentSpecification)m_SwApp.GetOpenDocSpec(filePath);
            spec.ReadOnly = readOnly;
            var model = m_SwApp.OpenDoc7(spec);

            if (model != null)
            {
                var docWrapper = new DocumentWrapper(m_SwApp, model);
                m_Disposables.Add(docWrapper);
                return docWrapper;
            }
            else 
            {
                throw new NullReferenceException($"Failed to open the the data document at {filePath}");
            }
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var disp in m_Disposables) 
            {
                try
                {
                    disp.Dispose();
                }
                catch 
                {
                }
            }
        }
    }
}
