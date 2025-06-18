using NUnit.Framework;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Exceptions;
using Xarial.XCad.Documents.Extensions;
using Xarial.XCad.Enums;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.SwDocumentManager;
using Xarial.XCad.SwDocumentManager.Documents;
using Xarial.XCad.Tests.Common;

namespace SolidWorksDocMgr.Tests.Integration
{
    public class DocumentWrapper : IDisposable
    {
        private readonly ISwDmApplication m_App;
        public ISwDmDocument Document { get; }

        private bool m_IsDisposed;

        internal DocumentWrapper(ISwDmApplication app, ISwDmDocument model)
        {
            m_App = app;
            Document = model;
            m_IsDisposed = false;
        }

        public void Dispose()
        {
            if (!m_IsDisposed)
            {
                Document.Close();
                m_IsDisposed = true;
            }
        }
    }

    [TestFixture]
    [RequiresThread(System.Threading.ApartmentState.STA)]
    public abstract class IntegrationTests
    {
        private SwVersion_e? SW_VERSION = SwVersion_e.Sw2024;

        public ISwDmApplication Application => m_TestManager.Application;

        private TestManager<ISwDmApplication, ISwDmDocument> m_TestManager;
        private Lazy<SwStarter> m_SwApp;

        [OneTimeSetUp]
        public void Setup()
        {
            const string SW_DM_KEY = "SW_DM_KEY";
            const string XCAD_TEST_DATA = "XCAD_TEST_DATA";

            var dmKey = Environment.GetEnvironmentVariable(SW_DM_KEY, EnvironmentVariableTarget.User);

            if (string.IsNullOrEmpty(dmKey))
            {
                dmKey = Environment.GetEnvironmentVariable(SW_DM_KEY, EnvironmentVariableTarget.Machine);
            }

            var dataArchiveFile = Environment.GetEnvironmentVariable(XCAD_TEST_DATA, EnvironmentVariableTarget.User);

            if (string.IsNullOrEmpty(dataArchiveFile))
            {
                dataArchiveFile = Environment.GetEnvironmentVariable(XCAD_TEST_DATA, EnvironmentVariableTarget.Machine);
            }

            var dmApp = SwDmApplicationFactory.Create(dmKey);

            m_TestManager = new TestManager<ISwDmApplication, ISwDmDocument>(dmApp, dataArchiveFile);

            m_SwApp = new Lazy<SwStarter>(() => 
            {
                //return new SwStarter(SW_VERSION);
                return new SwStarter(-1);
            });
        }

        protected void CopyDirectory(string srcPath, string destPath) => m_TestManager.CopyDirectory(srcPath, destPath);

        protected DataFile GetDataFile(string name) => m_TestManager.GetDataFile(name);

        protected DataDocument<ISwDmDocument> OpenDataDocument(string name, bool readOnly = true) 
            => m_TestManager.OpenDataDocument(name, readOnly);

        protected void UpdateSwReferences(string filePath, string workDir)
        {
            using (var doc = (ISwDocument)m_SwApp.Value.Application.Documents.Open(filePath))
            {
                foreach (ISwDocument dep in doc.Dependencies) 
                {
                    if (dep.IsCommitted)
                    {
                        RebuildAndSave(dep);
                    }
                }

                RebuildAndSave(doc);

                var deps = (doc.Model.Extension.GetDependencies(true, false, false, false, false) as string[]).Where((item, index) => index % 2 != 0).ToArray();

                if (!deps.All(d => d.Contains("^") || d.StartsWith(workDir, StringComparison.CurrentCultureIgnoreCase)))
                {
                    throw new Exception("Failed to setup source assemblies");
                }
            }
        }

        private void RebuildAndSave(ISwDocument dep)
        {
            dep.Model.ForceRebuild3(false);

            int errs = -1;
            int warns = -1;

            if (!dep.Model.Save3((int)(swSaveAsOptions_e.swSaveAsOptions_Silent | swSaveAsOptions_e.swSaveAsOptions_SaveReferenced), ref errs, ref warns))
            {
                throw new Exception();
            }
        }

        [TearDown]
        public void TearDown()
        {
        }

        [OneTimeTearDown]
        public void FinalTearDown()
        {
            m_TestManager?.Dispose();

            if (m_SwApp.IsValueCreated) 
            {
                m_SwApp.Value.Dispose();
            }
        }
    }
}
