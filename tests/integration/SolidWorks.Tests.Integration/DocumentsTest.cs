using NUnit.Framework;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Xarial.XCad.Base;
using Xarial.XCad.Data.Enums;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
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
            doc.State = DocumentState_e.ReadOnly | DocumentState_e.Silent;

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
            var doc1 = (ISwDocument)m_App.Documents.Open(GetFilePath("Assembly1\\TopAssem1.SLDASM"), DocumentState_e.ViewOnly);

            var isViewOnly1 = doc1.Model.IsOpenedViewOnly();
            var isAssm1 = doc1.Model is IAssemblyDoc;
            var isInCollection1 = m_App.Documents.Contains(doc1);
            var type1 = doc1.GetType();

            doc1.Close();

            var doc2 = (ISwDocument)m_App.Documents.Open(GetFilePath("Sheets1.SLDDRW"), DocumentState_e.Rapid);

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

            m_App.Sw.CloseAllDocuments(true);

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

        public class TestData 
        {
            public string Text { get; set; }
            public int Number { get; set; }
        }

        [Test]
        public void ThirdPartyStreamTest() 
        {
            const string STREAM_NAME = "_xCadIntegrationTestStream_";

            var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".sldprt");

            using (var doc = NewDocument(swDocumentTypes_e.swDocPART)) 
            {
                var part = m_App.Documents.Active;

                part.StreamWriteAvailable += (d) =>
                    {
                        using (var stream = d.OpenStream(STREAM_NAME, AccessType_e.Write))
                        {
                            var xmlSer = new XmlSerializer(typeof(TestData));

                            var data = new TestData()
                            {
                                Text = "Test1",
                                Number = 15
                            };

                            xmlSer.Serialize(stream, data);
                        }
                    };

                int errs = -1;
                int warns = -1;

                part.Model.Extension.SaveAs(tempFile, (int)swSaveAsVersion_e.swSaveAsCurrentVersion,
                    (int)swSaveAsOptions_e.swSaveAsOptions_Silent, null, ref errs, ref warns);
            }

            TestData result = null;

            using (var doc = OpenDataDocument(tempFile))
            {
                var part = m_App.Documents.Active;

                using (var stream = part.OpenStream(STREAM_NAME, AccessType_e.Read))
                {
                    var xmlSer = new XmlSerializer(typeof(TestData));
                    result = xmlSer.Deserialize(stream) as TestData;
                }
            }

            File.Delete(tempFile);

            Assert.IsNotNull(result);
            Assert.AreEqual("Test1", result.Text);
            Assert.AreEqual(15, result.Number);
        }

        [Test]
        public void ThirdPartyStorageTest() 
        {
            const string SUB_STORAGE_PATH = "_xCadIntegrationTestStorage1_\\SubStorage2";
            const string STREAM1_NAME = "_xCadIntegrationStream1_";
            const string STREAM2_NAME = "_xCadIntegrationStream2_";

            var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".sldprt");

            using (var doc = NewDocument(swDocumentTypes_e.swDocPART))
            {
                var part = m_App.Documents.Active;

                part.StorageWriteAvailable += (d) =>
                {
                    var path = SUB_STORAGE_PATH.Split('\\');

                    using (var storage = part.OpenStorage(path[0], AccessType_e.Write))
                    {
                        using (var subStorage = storage.TryOpenStorage(path[1], true))
                        {
                            using (var str = subStorage.TryOpenStream(STREAM1_NAME, true))
                            {
                                var buffer = Encoding.UTF8.GetBytes("Test2");
                                str.Write(buffer, 0, buffer.Length);
                            }

                            using (var str = subStorage.TryOpenStream(STREAM2_NAME, true))
                            {
                                using (var binWriter = new BinaryWriter(str)) 
                                {
                                    binWriter.Write(25);
                                }
                            }
                        }
                    }
                };

                int errs = -1;
                int warns = -1;

                part.Model.Extension.SaveAs(tempFile, (int)swSaveAsVersion_e.swSaveAsCurrentVersion,
                    (int)swSaveAsOptions_e.swSaveAsOptions_Silent, null, ref errs, ref warns);
            }

            var subStreamsCount = 0;
            var txt = "";
            var number = 0;

            using (var doc = OpenDataDocument(tempFile))
            {
                var part = m_App.Documents.Active;

                var path = SUB_STORAGE_PATH.Split('\\');

                using (var storage = part.TryOpenStorage(path[0], AccessType_e.Read))
                {
                    using (var subStorage = storage.TryOpenStorage(path[1], false))
                    {
                        subStreamsCount = subStorage.GetSubStreamNames().Length;

                        using (var str = subStorage.TryOpenStream(STREAM1_NAME, false))
                        {
                            var buffer = new byte[str.Length];

                            str.Read(buffer, 0, buffer.Length);

                            txt = Encoding.UTF8.GetString(buffer);
                        }

                        using (var str = subStorage.TryOpenStream(STREAM2_NAME, false))
                        {
                            using (var binReader = new BinaryReader(str))
                            {
                                number = binReader.ReadInt32();
                            }
                        }
                    }
                }
            }

            File.Delete(tempFile);

            Assert.AreEqual(2, subStreamsCount);
            Assert.AreEqual("Test2", txt);
            Assert.AreEqual(25, number);
        }

        [Test]
        public void DocumentDependenciesUnloadedTest() 
        {
            var assm = m_App.Documents.PreCreate<ISwAssembly>();
            assm.Path = GetFilePath(@"Assembly2\TopAssem.SLDASM");

            var deps = assm.Dependencies;

            var dir = Path.GetDirectoryName(assm.Path);

            Assert.AreEqual(6, deps.Length);
            Assert.That(deps.All(d => !d.IsCommitted));
            Assert.That(deps.Any(d => string.Equals(d.Path, Path.Combine(dir, "Part2.SLDPRT"))));
            Assert.That(deps.Any(d => string.Equals(d.Path, Path.Combine(dir, "Part3.SLDPRT"))));
            Assert.That(deps.Any(d => string.Equals(d.Path, Path.Combine(dir, "Part4-1 (XYZ).SLDPRT"))));
            Assert.That(deps.Any(d => string.Equals(d.Path, Path.Combine(dir, "Assem1.SLDASM"))));
            Assert.That(deps.Any(d => string.Equals(d.Path, Path.Combine(dir, "Assem2.SLDASM"))));
            Assert.That(deps.Any(d => string.Equals(d.Path, Path.Combine(dir, "Part1.SLDPRT"))));
        }

        [Test]
        public void DocumentDependenciesLoadedTest()
        {
            string dir = "";
            Dictionary<string, bool> depsData;

            using (var assm = OpenDataDocument(@"Assembly2\TopAssem.SLDASM")) 
            {
                var deps = m_App.Documents.Active.Dependencies;
                depsData = deps.ToDictionary(d => d.Path, d => d.IsCommitted, StringComparer.CurrentCultureIgnoreCase);

                dir = Path.GetDirectoryName(m_App.Documents.Active.Path);
            }
            
            Assert.AreEqual(6, depsData.Count);
            Assert.IsFalse(depsData[Path.Combine(dir, "Part2.SLDPRT")]);
            Assert.IsTrue(depsData[Path.Combine(dir, "Part3.SLDPRT")]);
            Assert.IsTrue(depsData[Path.Combine(dir, "Part4-1 (XYZ).SLDPRT")]);
            Assert.IsFalse(depsData[Path.Combine(dir, "Assem1.SLDASM")]);
            Assert.IsTrue(depsData[Path.Combine(dir, "Assem2.SLDASM")]);
            Assert.IsTrue(depsData[Path.Combine(dir, "Part1.SLDPRT")]);
        }

        [Test]
        public void OpenConflictTest()
        {
            var filePath = GetFilePath(@"Assembly1\Part1.SLDPRT");

            using (var doc = OpenDataDocument(filePath))
            {
                var p0 = m_App.Documents.Active;

                var p1 = m_App.Documents.PreCreate<ISwPart>();
                p1.Path = filePath;

                Assert.Throws<DocumentAlreadyOpenedException>(() => p1.Commit());

                var p2 = m_App.Documents.PreCreate<IXUnknownDocument>();
                p2.Path = filePath;
                p2.Commit();

                var p3 = p2.GetSpecific();

                Assert.AreEqual(p0, p3);
            }
        }
    }
}
