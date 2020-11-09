using NUnit.Framework;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.SolidWorks.Documents;

namespace SolidWorks.Tests.Integration
{
    public class DocumentsTest : IntegrationTests
    {
        [Test]
        public void OpenDocumentPreCreateTest()
        {
            var doc = m_App.Documents.PreCreate<ISwDocument>();

            doc.Path = GetFilePath("Features1.SLDPRT");
            doc.ReadOnly = true;
            doc.Silent = true;

            doc.Commit();

            doc = (ISwDocument)(doc as IXUnknownDocument).GetSpecific();

            var isReadOnly = doc.Model.IsOpenedReadOnly();
            var isPart = doc.Model is IPartDoc;
            var isInCollection = m_App.Documents.Contains(doc);
            var type = doc.GetType();

            doc.Close();

            Assert.That(isReadOnly);
            Assert.That(isPart);
            Assert.That(isInCollection);
            Assert.That(typeof(ISwPart).IsAssignableFrom(type));
        }

        [Test]
        public void OpenDocumentExtensionTest()
        {
            var doc1 = (ISwDocument)m_App.Documents.Open(GetFilePath("Assembly1\\TopAssem1.SLDASM"), viewOnly: true);

            var isViewOnly1 = doc1.Model.IsOpenedViewOnly();
            var isAssm1 = doc1.Model is IAssemblyDoc;
            var isInCollection1 = m_App.Documents.Contains(doc1);
            var type1 = doc1.GetType();

            doc1.Close();

            var doc2 = (ISwDocument)m_App.Documents.Open(GetFilePath("Sheets1.SLDDRW"), rapid: true);

            var isDrw2 = doc2.Model is IDrawingDoc;
            var isInCollection2 = m_App.Documents.Contains(doc2);
            var type2 = doc2.GetType();

            doc2.Close();

            Assert.That(isViewOnly1);
            Assert.That(isAssm1);
            Assert.That(isInCollection1);
            Assert.That(typeof(ISwAssembly).IsAssignableFrom(type1));

            Assert.That(isDrw2);
            Assert.That(isInCollection2);
            Assert.That(typeof(ISwDrawing).IsAssignableFrom(type2));
        }

        [Test]
        public void OpenForeignDocumentTest()
        {
            var doc = (ISwDocument)m_App.Documents.Open(GetFilePath("foreign.IGS"));

            var isPart = doc.Model is IPartDoc;
            var isInCollection = m_App.Documents.Contains(doc);
            var bodiesCount = ((doc.Model as IPartDoc).GetBodies2((int)swBodyType_e.swSolidBody, true) as object[]).Length;
            var type = doc.GetType();

            doc.Close();

            Assert.That(isPart);
            Assert.That(isInCollection);
            Assert.AreEqual(1, bodiesCount);
            Assert.That(typeof(ISwPart).IsAssignableFrom(type));
        }

        [Test]
        public void UserOpenCloseDocumentTest() 
        {
            int errs = -1;
            int warns = -1;
            
            var model = m_App.Sw.OpenDoc6(GetFilePath("Configs1.SLDPRT"),
                (int)swDocumentTypes_e.swDocPART, 
                (int)swOpenDocOptions_e.swOpenDocOptions_Silent, 
                "", ref errs, ref warns);
            
            var count = m_App.Documents.Count;
            var activeDocType = m_App.Documents.Active.GetType();
            var activeDocPath = m_App.Documents.Active.Path;

            m_App.Sw.CloseDoc(model.GetTitle());

            var count1 = m_App.Documents.Count;

            Assert.AreEqual(1, count);
            Assert.That(typeof(ISwPart).IsAssignableFrom(activeDocType));
            Assert.AreEqual(GetFilePath("Configs1.SLDPRT"), activeDocPath);
            Assert.AreEqual(0, count1);
        }

        [Test]
        public void DocumentLifecycleEventsTest() 
        {
            var createdDocs = new List<string>();
            var d1ClosingCount = 0;
            var d2ClosingCount = 0;

            m_App.Documents.DocumentCreated += (d)=> 
            {
                createdDocs.Add(Path.GetFileNameWithoutExtension(d.Title).ToLower());
            };

            var doc1 = (ISwDocument)m_App.Documents.Open(GetFilePath("foreign.IGS"));

            doc1.Closing += (d)=> 
            {
                if (d != doc1)
                {
                    throw new Exception("doc1 is invalid");
                }

                d1ClosingCount++;
            };

            int errs = -1;
            int warns = -1;

            var model2 = m_App.Sw.OpenDoc6(GetFilePath("Assembly1\\SubSubAssem1.SLDASM"),
                (int)swDocumentTypes_e.swDocASSEMBLY,
                (int)swOpenDocOptions_e.swOpenDocOptions_Silent,
                "", ref errs, ref warns);

            var doc2 = m_App.Documents[model2];

            doc2.Closing += (d) =>
            {
                if (d != doc2)
                {
                    throw new Exception("doc2 is invalid");
                }

                d2ClosingCount++;
            };

            var activeDocTitle = Path.GetFileNameWithoutExtension(m_App.Documents.Active.Title).ToLower();

            m_App.Documents.Active = doc1;

            var activeDocTitle1 = Path.GetFileNameWithoutExtension(m_App.Documents.Active.Title).ToLower();

            doc1.Close();
            doc2.Close();

            Assert.That(createdDocs.OrderBy(d => d)
                .SequenceEqual(new string[] { "part1", "part3", "part4", "foreign", "subsubassem1" }.OrderBy(d => d)));
            Assert.AreEqual(d1ClosingCount, 1);
            Assert.AreEqual(d2ClosingCount, 1);
            Assert.AreEqual(activeDocTitle, "subsubassem1");
            Assert.AreEqual(activeDocTitle1, "foreign");
        }

        [Test]
        public void PartEventsTest() 
        {
            var rebuildCount = 0;
            var saveCount = 0;
            var confActiveCount = 0;
            var confName = "";

            var tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".sldprt");

            File.Copy(GetFilePath("Configs1.SLDPRT"), tempFilePath);

            using (var doc = OpenDataDocument(tempFilePath, false)) 
            {
                var part = (ISwPart)m_App.Documents.Active;

                part.Configurations.ConfigurationActivated += (d, c)=> 
                {
                    confName = c.Name;
                    confActiveCount++;
                };

                part.Model.ShowConfiguration2("Conf1");

                part.Rebuild += (d) =>
                {
                    rebuildCount++;
                };

                part.Model.ForceRebuild3(false);

                part.Saving += (d, t, a) =>
                {
                    saveCount++;
                };
                
                part.Model.SetSaveFlag();

                const int swCommands_Save = 2;
                m_App.Sw.RunCommand(swCommands_Save, "");
            }

            File.Delete(tempFilePath);

            Assert.AreEqual(1, rebuildCount);
            Assert.AreEqual(1, saveCount);
            Assert.AreEqual(1, confActiveCount);
            Assert.AreEqual("Conf1", confName);
        }

        [Test]
        public void DrawingEventsTest()
        {
            var sheetActiveCount = 0;
            var sheetName = "";

            using (var doc = OpenDataDocument("Sheets1.SLDDRW"))
            {
                var draw = (ISwDrawing)m_App.Documents.Active;

                draw.Sheets.SheetActivated += (d, s) =>
                {
                    sheetName = s.Name;
                    sheetActiveCount++;
                };

                var feat = (IFeature)draw.Drawing.FeatureByName("MySheet");
                feat.Select2(false, -1);
                const int swCommands_Activate_Sheet = 1206;
                m_App.Sw.RunCommand(swCommands_Activate_Sheet, "");
            }

            Assert.AreEqual(1, sheetActiveCount);
            Assert.AreEqual("MySheet", sheetName);
        }
    }
}
