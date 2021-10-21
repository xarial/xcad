using NUnit.Framework;
using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents;
using Xarial.XCad.SwDocumentManager;
using Xarial.XCad.SwDocumentManager.Documents;

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
        private const string DATA_FOLDER = @"C:\Users\artem\OneDrive\xCAD\TestData";

        protected ISwDmApplication m_App;
        private ISwDMApplication m_SwDmApp;

        private List<IDisposable> m_Disposables;

        [OneTimeSetUp]
        public void Setup()
        {
            var dmKey = Environment.GetEnvironmentVariable("SW_DM_KEY", EnvironmentVariableTarget.Machine);

            m_App = SwDmApplicationFactory.Create(dmKey);
            m_SwDmApp = m_App.SwDocMgr;

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

        protected DocumentWrapper OpenDataDocument(string name, bool readOnly = true)
        {
            var filePath = GetFilePath(name);

            var doc = (ISwDmDocument)m_App.Documents.Open(filePath, 
                readOnly 
                ? Xarial.XCad.Documents.Enums.DocumentState_e.ReadOnly 
                : Xarial.XCad.Documents.Enums.DocumentState_e.Default);

            if (doc != null)
            {
                var docWrapper = new DocumentWrapper(m_App, doc);
                m_Disposables.Add(docWrapper);
                return docWrapper;
            }
            else
            {
                throw new NullReferenceException($"Failed to open the the data document at '{filePath}'");
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

            m_Disposables.Clear();

            while (m_App.Documents.Any())
            {
                m_App.Documents.First().Close();
            }
        }

        [OneTimeTearDown]
        public void FinalTearDown()
        {
        }
    }
}
