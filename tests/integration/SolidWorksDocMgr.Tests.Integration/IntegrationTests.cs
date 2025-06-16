using NUnit.Framework;
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
using Xarial.XCad.Documents.Extensions;
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

            var app = SwDmApplicationFactory.Create(dmKey);

            m_TestManager = new TestManager<ISwDmApplication, ISwDmDocument>(app, dataArchiveFile);
        }

        protected void UpdateSwReferences(string destPath, params string[] assmRelPaths)
        {
            Process prc;

            using (var app = SwApplicationFactory.Create(SW_VERSION,
                            Xarial.XCad.Enums.ApplicationState_e.Background
                            | Xarial.XCad.Enums.ApplicationState_e.Silent
                            | Xarial.XCad.Enums.ApplicationState_e.Safe))
            {
                prc = app.Process;

                foreach (var assmPath in assmRelPaths)
                {
                    using (var doc = (ISwDocument)app.Documents.Open(Path.Combine(destPath, assmPath)))
                    {
                        doc.Model.ForceRebuild3(false);
                        doc.Save();
                        var deps = (doc.Model.Extension.GetDependencies(false, false, false, false, false) as string[]).Where((item, index) => index % 2 != 0).ToArray();

                        if (!deps.All(d => d.Contains("^") || d.StartsWith(destPath, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            throw new Exception("Failed to setup source assemblies");
                        }
                    }
                }

                app.Close();
            }

            prc.Kill();
        }

        protected void CopyDirectory(string srcPath, string destPath)
        {
            foreach (var srcFile in Directory.GetFiles(srcPath, "*.*", SearchOption.AllDirectories))
            {
                var relPath = srcFile.Substring(srcPath.Length + 1);
                var destFilePath = Path.Combine(destPath, relPath);
                var destDir = Path.GetDirectoryName(destFilePath);

                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                File.Copy(srcFile, destFilePath);
            }
        }

        protected DataFile GetDataFile(string name) => m_TestManager.GetDataFile(name);

        protected DataDocument<ISwDmDocument> OpenDataDocument(string name, bool readOnly = true) 
            => m_TestManager.OpenDataDocument(name, readOnly);

        [TearDown]
        public void TearDown()
        {
        }

        [OneTimeTearDown]
        public void FinalTearDown()
        {
            m_TestManager?.Dispose();
        }
    }
}
