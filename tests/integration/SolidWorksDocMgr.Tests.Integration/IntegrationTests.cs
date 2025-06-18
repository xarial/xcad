#define TESTING_MODE

using NUnit.Framework;
using System;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.SwDocumentManager;
using Xarial.XCad.SwDocumentManager.Documents;
using Xarial.XCad.Tests.Common;

namespace SolidWorksDocMgr.Tests.Integration
{
    [TestFixture]
    [RequiresThread(System.Threading.ApartmentState.STA)]
    public abstract class IntegrationTests
    {
        private SwVersion_e? SW_VERSION = SwVersion_e.Sw2025;

        public ISwDmApplication Application => m_TestManager.Application;

        private TestManager<ISwDmApplication, ISwDmDocument> m_TestManager;
        private Lazy<SwStarter> m_SwApp;

        [OneTimeSetUp]
        public void Setup()
        {
            const string SW_DM_KEY = "SW_DM_KEY";

            var dmKey = Environment.GetEnvironmentVariable(SW_DM_KEY, EnvironmentVariableTarget.User);

            if (string.IsNullOrEmpty(dmKey))
            {
                dmKey = Environment.GetEnvironmentVariable(SW_DM_KEY, EnvironmentVariableTarget.Machine);
            }

            var dmApp = SwDmApplicationFactory.Create(dmKey);

            m_TestManager = new TestManager<ISwDmApplication, ISwDmDocument>(dmApp);

            m_SwApp = new Lazy<SwStarter>(() => 
            {
#if TESTING_MODE
                return new SwStarter(SW_VERSION);
#else
                return new SwStarter(-1);
#endif
            });
        }

        protected void CopyDirectory(string srcPath, string destPath) => m_TestManager.CopyDirectory(srcPath, destPath);

        protected DataFile GetDataFile(string name) => m_TestManager.GetDataFile(name);

        protected DataDocument<ISwDmDocument> OpenDataDocument(string name, DocumentState_e state = DocumentState_e.ReadOnly) 
            => m_TestManager.OpenDataDocument(name, state);

        protected void UpdateSwReferences(string filePath, string workDir) => m_SwApp.Value.UpdateReferences(filePath, workDir);

        [TearDown]
        public void TearDown()
        {
        }

        [OneTimeTearDown]
        public void FinalTearDown()
        {
            m_TestManager?.Dispose();

            if (m_SwApp?.IsValueCreated == true) 
            {
                m_SwApp.Value.Dispose();
            }
        }
    }
}
