using NUnit.Framework;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Xarial.XCad.Enums;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Enums;

namespace SolidWorks.Tests.Integration
{
    [TestFixture]
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

        private const int SW_PRC_ID = 0;
        private const string DATA_FOLDER = @"C:\Users\artem\OneDrive\xCAD\TestData";

        protected ISwApplication m_App;
        private ISldWorks m_SwApp;

        private List<IDisposable> m_Disposables;

        private bool m_CloseSw;

        [OneTimeSetUp]
        public void Setup()
        {
            if (SW_PRC_ID < 0)
            {
                List<string> m_DisabledStartupAddIns;

                SwApplicationFactory.DisableAllAddInsStartup(out m_DisabledStartupAddIns);

                m_App = SwApplicationFactory.Create(null,
                    ApplicationState_e.Background 
                    | ApplicationState_e.Safe 
                    | ApplicationState_e.Silent);

                if (m_DisabledStartupAddIns?.Any() == true)
                {
                    SwApplicationFactory.EnableAddInsStartup(m_DisabledStartupAddIns);
                }

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
                m_App = SwApplicationFactory.FromProcess(prc);
            }

            m_SwApp = m_App.Sw;
            m_Disposables = new List<IDisposable>();
        }

        protected string GetFilePath(string name)
        {
            string filePath;

            if (Path.IsPathRooted(name)) 
            {
                filePath = name;
            }
            else 
            {
                filePath = Path.Combine(DATA_FOLDER, name);
            }

            return filePath;
        }

        protected IDisposable OpenDataDocument(string name, bool readOnly = true, Action<IDocumentSpecification> specEditor = null) 
        {
            var filePath = GetFilePath(name);

            var spec = (IDocumentSpecification)m_SwApp.GetOpenDocSpec(filePath);
            spec.ReadOnly = readOnly;
            spec.LightWeight = false;
            specEditor?.Invoke(spec);

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
            var defTemplatePath = m_SwApp.GetDocumentTemplate(
                (int)docType, "", (int)swDwgPaperSizes_e.swDwgPapersUserDefined, 100, 100);

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

        protected void AssertCompareDoubles(double actual, double expected, int digits = 8)
            => Assert.That(Math.Round(expected, digits), Is.EqualTo(Math.Round(actual, digits)).Within(0.000001).Percent);

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

            m_Disposables.Clear();
        }

        [OneTimeTearDown]
        public void FinalTearDown()
        {
            if (m_CloseSw) 
            {
                m_App.Process.Kill();
            }
        }
    }
}
