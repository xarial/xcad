//#define TESTING_MODE

using NUnit.Framework;
using SolidWorks.Interop.swconst;
using System;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.Tests.Common;

namespace SolidWorks.Tests.Integration
{
    [TestFixture]
    [RequiresThread(System.Threading.ApartmentState.STA)]
    public abstract class IntegrationTests
    {
        private SwVersion_e? SW_VERSION = SwVersion_e.Sw2025;

        public ISwApplication Application => m_TestManager.Application;

        private TestManager<ISwApplication, ISwDocument> m_TestManager;

        private SwStarter m_SwStarter;

        [OneTimeSetUp]
        public void Setup()
        {

#if TESTING_MODE
            m_SwStarter = new SwStarter(SW_VERSION);
#else
            m_SwStarter = new SwStarter(-1);
#endif

            m_TestManager = new TestManager<ISwApplication, ISwDocument>(m_SwStarter.Application);
        }

        protected DataFile GetDataFile(string name) => m_TestManager.GetDataFile(name);

        protected DataDocument<ISwDocument> OpenDataDocument(string name, DocumentState_e state = DocumentState_e.ReadOnly)
            => m_TestManager.OpenDataDocument(name, state);

        protected void CopyDirectory(string srcPath, string destPath) => m_TestManager.CopyDirectory(srcPath, destPath);

        protected void UpdateSwReferences(string filePath, string workDir) => m_SwStarter.UpdateReferences(filePath, workDir);

        protected DataDocument<ISwDocument> NewDataDocument(swDocumentTypes_e docType) 
        {
            switch (docType) 
            {
                case swDocumentTypes_e.swDocPART:
                    return m_TestManager.NewDocument<ISwPart>();

                case swDocumentTypes_e.swDocASSEMBLY:
                    return m_TestManager.NewDocument<ISwAssembly>();

                case swDocumentTypes_e.swDocDRAWING:
                    return m_TestManager.NewDocument<ISwDrawing>();

                default:
                    throw new NotSupportedException();
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
            try
            {
                Application.Sw.CloseAllDocuments(true);
            }
            catch 
            {
            }
        }

        [OneTimeTearDown]
        public void FinalTearDown()
        {
            m_TestManager?.Dispose();
            m_SwStarter?.Dispose();
        }
    }
}
