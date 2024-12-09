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
using Environment = System.Environment;

namespace SolidWorks.Tests.Integration
{
    [TestFixture]
    [RequiresThread(System.Threading.ApartmentState.STA)]
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

        private readonly string m_DataFolder;
        private SwVersion_e? SW_VERSION = SwVersion_e.Sw2024;

        protected ISwApplication m_App;
        private Process m_Process;
        private ISldWorks m_SwApp;

        private List<IDisposable> m_Disposables;

        private bool m_CloseSw;

        public IntegrationTests() 
        {
            m_DataFolder = Environment.GetEnvironmentVariable("XCAD_TEST_DATA", EnvironmentVariableTarget.User);

            if (string.IsNullOrEmpty(m_DataFolder))
            {
                m_DataFolder = Environment.GetEnvironmentVariable("XCAD_TEST_DATA", EnvironmentVariableTarget.Machine);
            }
        }

        [OneTimeSetUp]
        public void Setup()
        {
            if (SW_PRC_ID < 0)
            {
                IReadOnlyList<string> m_DisabledStartupAddIns;

                SwApplicationFactory.DisableAllAddInsStartup(out m_DisabledStartupAddIns);

                m_App = SwApplicationFactory.Create(SW_VERSION,
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
            m_Process = m_App.Process;
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
                filePath = Path.Combine(m_DataFolder, name);
            }

            return filePath;
        }

        protected IDisposable OpenDataDocument(string name, bool readOnly = true, Action<IDocumentSpecification> specEditor = null) 
        {
            var filePath = GetFilePath(name);

            var spec = (IDocumentSpecification)m_SwApp.GetOpenDocSpec(filePath);
            spec.ReadOnly = readOnly;
            spec.LightWeight = false;
            spec.UseLightWeightDefault = false;
            specEditor?.Invoke(spec);

            var model = m_SwApp.OpenDoc7(spec);

            if (model != null)
            {
                if (!spec.LightWeight)
                {
                    if (model is IAssemblyDoc)
                    {
                        (model as IAssemblyDoc).ResolveAllLightWeightComponents(false);
                    }
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
            var useDefTemplates = m_SwApp.GetUserPreferenceToggle((int)swUserPreferenceToggle_e.swAlwaysUseDefaultTemplates);

            try
            {
                m_SwApp.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swAlwaysUseDefaultTemplates, true);
                
                var defTemplatePath = m_SwApp.GetDocumentTemplate(
                    (int)docType, "", (int)swDwgPaperSizes_e.swDwgPapersUserDefined, 0.1, 0.1);

                if (string.IsNullOrEmpty(defTemplatePath))
                {
                    throw new Exception("Default template is not found");
                }

                var model = (IModelDoc2)m_SwApp.NewDocument(defTemplatePath, (int)swDwgPaperSizes_e.swDwgPapersUserDefined, 0.1, 0.1);

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
            finally
            {
                m_SwApp.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swAlwaysUseDefaultTemplates, useDefTemplates);
            }
        }

        protected void AssertCompareDoubles(double actual, double expected, int digits = 8)
            => Assert.That(Math.Round(actual, digits), Is.EqualTo(Math.Round(expected, digits)).Within(0.000001).Percent);

        protected void AssertCompareDoubleArray(double[] actual, double[] expected, int digits = 8, double percent = 0.000001)
        {
            if (actual.Length == expected.Length)
            {
                for (int i = 0; i < actual.Length; i++) 
                {
                    Assert.That(Math.Round(actual[i], digits), Is.EqualTo(Math.Round(expected[i], digits)).Within(percent).Percent);
                }
            }
            else 
            {
                Assert.Fail("Arrays size mismatch");
            }
        }

        [TearDown]
        public void TearDown() 
        {
            Debug.Print("Unit Tests: Disposing test disposables");

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
            Debug.Print($"Unit Tests: Closing SOLIDWORKS instance: {m_CloseSw}");

            if (m_CloseSw) 
            {
                m_Process.Kill();
            }
        }
    }
}
