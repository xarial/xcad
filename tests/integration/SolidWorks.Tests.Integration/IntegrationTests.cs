using NUnit.Framework;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        private const int SW_PRC_ID = -1;
        private const string DATA_FOLDER = @"C:\Users\artem\OneDrive\xCAD\TestData";

        protected ISwApplication m_App;
        private ISldWorks m_SwApp;

        private List<IDisposable> m_Disposables;

        private bool m_CloseSw;

        [SetUp]
        public void Setup()
        {
            if (SW_PRC_ID < 0)
            {
                m_App = SwApplicationFactory.Start(null, "/b");
                m_CloseSw = true;
            }
            else if (SW_PRC_ID == 0) 
            {
                var prc = Process.GetProcessesByName("SLDWORKS").First();
                m_App = SwApplicationFactory.FromProcess(prc);
            }
            else
            {
                var prc = Process.GetProcessById(SW_PRC_ID);
                prc = Process.GetProcessById(SW_PRC_ID);
            }

            m_SwApp = m_App.Sw;
            m_Disposables = new List<IDisposable>();
        }

        protected string GetFilePath(string name) => Path.Combine(DATA_FOLDER, name);

        protected IDisposable OpenDataDocument(string name, bool readOnly = true) 
        {
            var filePath = GetFilePath(name);

            var spec = (IDocumentSpecification)m_SwApp.GetOpenDocSpec(filePath);
            spec.ReadOnly = readOnly;
            spec.LightWeight = false;
            var model = m_SwApp.OpenDoc7(spec);

            if (model != null)
            {
                if (model is IAssemblyDoc) 
                {
                    (model as IAssemblyDoc).ResolveAllLightWeightComponents(false);
                }

                var docWrapper = new DocumentWrapper(m_SwApp, model);
                m_Disposables.Add(docWrapper);
                return docWrapper;
            }
            else 
            {
                throw new NullReferenceException($"Failed to open the the data document at '{filePath}'");
            }
        }

        protected IDisposable NewDocument(swDocumentTypes_e docType) 
        {
            swUserPreferenceStringValue_e defTemplateType;

            switch (docType) 
            {
                case swDocumentTypes_e.swDocPART:
                    defTemplateType = swUserPreferenceStringValue_e.swDefaultTemplatePart;
                    break;
                case swDocumentTypes_e.swDocASSEMBLY:
                    defTemplateType = swUserPreferenceStringValue_e.swDefaultTemplateAssembly;
                    break;
                case swDocumentTypes_e.swDocDRAWING:
                    defTemplateType = swUserPreferenceStringValue_e.swDefaultTemplateDrawing;
                    break;
                default:
                    throw new NotSupportedException("Document type is not supported");
            }

            var defTemplatePath = m_SwApp.GetUserPreferenceStringValue((int)defTemplateType);

            if (string.IsNullOrEmpty(defTemplatePath)) 
            {
                throw new Exception("Default template is not found");
            }

            var model = (IModelDoc2)m_SwApp.NewDocument(defTemplatePath, (int)swDwgPaperSizes_e.swDwgPapersUserDefined, 100, 100);

            if (model != null)
            {
                var docWrapper = new DocumentWrapper(m_SwApp, model);
                m_Disposables.Add(docWrapper);
                return docWrapper;
            }
            else
            {
                throw new NullReferenceException($"Failed to create new document from '{defTemplatePath}'");
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

            if (m_CloseSw) 
            {
                m_App.Close();
                m_App.Dispose();
            }
        }
    }
}
