using Moq;
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
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Exceptions;
using Xarial.XCad.Documents.Extensions;
using Xarial.XCad.Documents.Services;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Documents.Exceptions;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.Utils;

namespace SolidWorks.Tests.Integration
{
    public class DocumentsTest : IntegrationTests
    {
        [Test]
        public void EqualsTest()
        {
            bool e1, e2, e3, e4, e5, e6, e7, e8, e9;

            var part1_1 = m_App.Documents.PreCreate<ISwPart>();
            part1_1.Path = GetFilePath(@"Drawing9\Part1.sldprt");

            var part1_3 = m_App.Documents.PreCreate<ISwPart>();
            part1_3.Path = GetFilePath(@"Drawing9\Part1.sldprt");

            IXDocument part1_2;
            IXDocument part2_1;
            IXDocument part2_2;

            using (OpenDataDocument(@"Drawing9\Part1.sldprt"))
            {
                part1_2 = m_App.Documents.Active;

                e1 = part1_1.Equals(part1_2);
                e2 = part1_2.Equals(part1_2);
                e3 = part1_1.Equals(part1_3);

                using (OpenDataDocument("Sketch1.sldprt"))
                {
                    part2_1 = m_App.Documents.Active;
                    part2_2 = m_App.Documents["Sketch1.sldprt"];

                    e4 = part2_1.Equals(part2_2);
                    e5 = part1_2.Equals(part2_1);
                }

                e6 = part2_1.Equals(part2_2);
                e7 = part1_2.Equals(part2_1);
            }

            e8 = part1_1.Equals(part1_2);
            e9 = part1_2.Equals(part2_1);

            Assert.IsFalse(e1);
            Assert.IsTrue(e2);
            Assert.IsTrue(e3);
            Assert.IsTrue(e4);
            Assert.IsFalse(e5);
            Assert.IsTrue(e6);
            Assert.IsFalse(e7);
            Assert.IsFalse(e8);
            Assert.IsFalse(e9);
        }

        [Test]
        public void OpenDocumentPreCreateUnknownTest()
        {
            var doc = m_App.Documents.PreCreate<ISwDocument>();

            doc.Path = GetFilePath("Features1.SLDPRT");
            doc.State = DocumentState_e.ReadOnly | DocumentState_e.Silent;

            doc.Commit();

            doc = (ISwDocument)(doc as IXUnknownDocument).GetSpecific();

            var isReadOnly = doc.Model.IsOpenedReadOnly();
            var isPart = doc.Model is IPartDoc;
            var isInCollection = m_App.Documents.Contains(doc, new XObjectEqualityComparer<IXDocument>());
            var type = doc.GetType();
            var contains1 = m_App.Documents.Contains(doc, new XObjectEqualityComparer<IXDocument>());

            doc.Close();

            var contains2 = m_App.Documents.Contains(doc, new XObjectEqualityComparer<IXDocument>());

            Assert.That(isReadOnly);
            Assert.That(isPart);
            Assert.That(isInCollection);
            Assert.That(typeof(ISwPart).IsAssignableFrom(type));
            Assert.IsTrue(contains1);
            Assert.IsFalse(contains2);
        }

        [Test]
        public void OpenDocumentPreCreateTest()
        {
            var doc = m_App.Documents.PreCreate<ISwPart>();
            doc.Path = GetFilePath("Features1.SLDPRT");

            doc.Commit();

            var contains1 = m_App.Documents.Contains(doc, new XObjectEqualityComparer<IXDocument>());

            doc.Close();

            var contains2 = m_App.Documents.Contains(doc, new XObjectEqualityComparer<IXDocument>());

            Assert.IsTrue(contains1);
            Assert.IsFalse(contains2);
        }

        [Test]
        public void OpenDocumentExtensionTest()
        {
            var doc1 = (ISwDocument)m_App.Documents.Open(GetFilePath("Assembly1\\TopAssem1.SLDASM"), DocumentState_e.ViewOnly);

            var isViewOnly1 = doc1.Model.IsOpenedViewOnly();
            var isAssm1 = doc1.Model is IAssemblyDoc;
            var isInCollection1 = m_App.Documents.Contains(doc1, new XObjectEqualityComparer<IXDocument>());
            var type1 = doc1.GetType();

            doc1.Close();

            var doc2 = (ISwDocument)m_App.Documents.Open(GetFilePath("Sheets1.SLDDRW"), DocumentState_e.Rapid);

            var isDrw2 = doc2.Model is IDrawingDoc;
            var isInCollection2 = m_App.Documents.Contains(doc2, new XObjectEqualityComparer<IXDocument>());
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
        public void OpenDocumentWithReferencesTest()
        {
            if (m_App.Documents.Count > 0)
            {
                throw new Exception("Documents already opened");
            }

            string[] paths;
            bool r1;
            int r2;
            int r3;

            var doc = m_App.Documents.PreCreateFromPath(GetFilePath(@"Assembly2\TopAssem.SLDASM"));
            doc.State = DocumentState_e.Silent | DocumentState_e.ReadOnly;
            doc.Commit();

            paths = m_App.Documents.Select(d => d.Path).ToArray();

            var part = m_App.Documents[GetFilePath(@"Assembly2\Part1.SLDPRT")];
            part.Close();

            r1 = part.IsAlive;
            r2 = m_App.Documents.Count;

            doc.Close();
            r3 = m_App.Documents.Count;

            Assert.AreEqual(5, paths.Length);

            CollectionAssert.AreEquivalent(new string[]
            {
                GetFilePath(@"Assembly2\TopAssem.SLDASM"),
                GetFilePath(@"Assembly2\Part4-1 (XYZ).SLDPRT"),
                GetFilePath(@"Assembly2\Part3.SLDPRT"),
                GetFilePath(@"Assembly2\Assem2.SLDASM"),
                GetFilePath(@"Assembly2\Part1.SLDPRT")
            }, paths);

            Assert.IsTrue(r1);
            Assert.AreEqual(5, r2);
            Assert.AreEqual(0, r3);
        }

        [Test]
        public void OpenUserDocumentWithReferencesTest()
        {
            if (m_App.Documents.Count > 0)
            {
                throw new Exception("Documents already opened");
            }

            string[] paths;
            int r1;
            int r2;

            var spec1 = (IDocumentSpecification)m_App.Sw.GetOpenDocSpec(GetFilePath(@"AssmCutLists1\AssmCutLists1.SLDASM"));
            spec1.ReadOnly = true;
            var model1 = m_App.Sw.OpenDoc7(spec1);

            var doc = m_App.Documents[model1];

            paths = m_App.Documents.Select(d => d.Path).ToArray();

            r1 = m_App.Documents.Count;
            doc.Close();

            r2 = m_App.Documents.Count;

            Assert.AreEqual(2, paths.Length);

            CollectionAssert.AreEquivalent(new string[]
            {
                GetFilePath(@"AssmCutLists1\AssmCutLists1.SLDASM"),
                GetFilePath(@"AssmCutLists1\CutListConfs1.SLDPRT")
            }, paths);

            Assert.AreEqual(2, r1);
            Assert.AreEqual(0, r2);
        }

        [Test]
        public void OpenForeignDocumentTest()
        {
            var doc = (ISwDocument)m_App.Documents.Open(GetFilePath("foreign.IGS"));

            var isPart = doc.Model is IPartDoc;
            var isInCollection = m_App.Documents.Contains(doc, new XObjectEqualityComparer<IXDocument>());
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
            try
            {
                var createdDocs = new List<IXDocument>();
                string[] createdDocsTitles;
                var d1ClosingCount = 0;
                var d2ClosingCount = 0;

                m_App.Documents.DocumentLoaded += (d) =>
                {
                    createdDocs.Add(d);
                };

                var doc1 = (ISwDocument)m_App.Documents.Open(GetFilePath("foreign.IGS"));

                doc1.Closing += (d, t) =>
                {
                    if (t == DocumentCloseType_e.Destroy)
                    {
                        if (d != doc1)
                        {
                            throw new Exception("doc1 is invalid");
                        }

                        d1ClosingCount++;
                    }
                };

                int errs = -1;
                int warns = -1;

                var model2 = m_App.Sw.OpenDoc6(GetFilePath("Assembly1\\SubSubAssem1.SLDASM"),
                    (int)swDocumentTypes_e.swDocASSEMBLY,
                    (int)swOpenDocOptions_e.swOpenDocOptions_Silent,
                    "", ref errs, ref warns);

                var doc2 = m_App.Documents[model2];

                doc2.Closing += (d, t) =>
                {
                    if (t == DocumentCloseType_e.Destroy)
                    {
                        if (d != doc2)
                        {
                            throw new Exception("doc2 is invalid");
                        }

                        d2ClosingCount++;
                    }
                };

                var activeDocTitle = Path.GetFileNameWithoutExtension(m_App.Documents.Active.Title).ToLower();

                m_App.Documents.Active = doc1;

                var activeDocTitle1 = Path.GetFileNameWithoutExtension(m_App.Documents.Active.Title).ToLower();

                createdDocsTitles = createdDocs.Select(d => Path.GetFileNameWithoutExtension(d.Title).ToLower()).ToArray();

                m_App.Sw.CloseAllDocuments(true);

                Assert.That(createdDocsTitles.OrderBy(d => d)
                    .SequenceEqual(new string[] { "part1", "part3", "part4", "foreign", "subsubassem1" }.OrderBy(d => d)));
                Assert.AreEqual(d1ClosingCount, 1);
                Assert.AreEqual(d2ClosingCount, 1);
                Assert.AreEqual(activeDocTitle, "subsubassem1");
                Assert.AreEqual(activeDocTitle1, "foreign");
            }
            finally
            {
                m_App.Sw.CloseAllDocuments(true);
            }
        }

        [Test]
        public void DocumentHidingTest()
        {
            var hiddenDocs = new List<string>();

            void OnHiding(IXDocument d, DocumentCloseType_e t)
            {
                if (t == DocumentCloseType_e.Hide)
                {
                    hiddenDocs.Add(Path.GetFileName(d.Path));
                }
            }

            var docs = m_App.Documents;

            using (var doc = OpenDataDocument(GetFilePath("Assembly1\\TopAssem1.SLDASM")))
            {
                var assm = docs.Active;
                assm.Closing += OnHiding;

                foreach (var dep in assm.Dependencies.TryIterateAll())
                {
                    dep.Closing += OnHiding;
                }

                m_App.Documents.Active = (ISwDocument)docs[GetFilePath("Assembly1\\Part1.sldprt")];
                m_App.Documents.Active.Close();
                m_App.Documents.Active = (ISwDocument)docs[GetFilePath("Assembly1\\SubAssem1.SLDASM")];
                m_App.Documents.Active.Close();
                m_App.Documents.Active = (ISwDocument)docs[GetFilePath("Assembly1\\Part3.sldprt")];
                m_App.Documents.Active.Close();
            }

            Assert.AreEqual(3, hiddenDocs.Count);
            Assert.That(string.Equals("Part1.sldprt", hiddenDocs[0], StringComparison.CurrentCultureIgnoreCase));
            Assert.That(string.Equals("SubAssem1.SLDASM", hiddenDocs[1], StringComparison.CurrentCultureIgnoreCase));
            Assert.That(string.Equals("Part3.sldprt", hiddenDocs[2], StringComparison.CurrentCultureIgnoreCase));
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

                part.Configurations.ConfigurationActivated += (d, c) =>
                {
                    confName = c.Name;
                    confActiveCount++;
                };

                part.Model.ShowConfiguration2("Conf1");

                part.Rebuilt += (d) =>
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
                        using (var stream = d.OpenStream(STREAM_NAME, true))
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

                using (var stream = part.OpenStream(STREAM_NAME, false))
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
        public void ActivateDocumentTest()
        {
            var activateDocsList = new List<string>();

            var handler = new DocumentEventDelegate((IXDocument doc) =>
            {
                activateDocsList.Add(Path.GetFileName(doc.Path));
            });

            m_App.Documents.DocumentActivated += handler;

            var results = new List<bool>();

            var spec1 = (IDocumentSpecification)m_App.Sw.GetOpenDocSpec(GetFilePath(@"Configs1.SLDPRT"));
            spec1.ReadOnly = true;
            var model1 = m_App.Sw.OpenDoc7(spec1);
            var spec2 = (IDocumentSpecification)m_App.Sw.GetOpenDocSpec(GetFilePath(@"AssmCutLists1\AssmCutLists1.SLDASM"));
            spec2.ReadOnly = true;
            var model2 = m_App.Sw.OpenDoc7(spec2);
            var newDoc = NewDocument(swDocumentTypes_e.swDocDRAWING);
            var model3 = m_App.Sw.IActiveDoc2;
            newDoc.Dispose();
            m_App.Sw.CloseDoc(model1.GetTitle());

            //keep document otherwise sw closes automatically
            var testPart = m_App.Documents.PreCreate<ISwPart>();
            testPart.State = DocumentState_e.Hidden;
            testPart.Commit();
            //

            m_App.Sw.CloseDoc(model2.GetTitle());

            Assert.AreEqual(4, activateDocsList.Count);
            Assert.AreEqual("Configs1.SLDPRT", activateDocsList[0]);
            Assert.AreEqual("AssmCutLists1.SLDASM", activateDocsList[1]);
            Assert.AreEqual("", activateDocsList[2]);
            Assert.AreEqual("AssmCutLists1.SLDASM", activateDocsList[3]);
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

                    using (var storage = part.OpenStorage(path[0], true))
                    {
                        using (var subStorage = storage.OpenStorage(path[1], true))
                        {
                            using (var str = subStorage.OpenStream(STREAM1_NAME, true))
                            {
                                var buffer = Encoding.UTF8.GetBytes("Test2");
                                str.Write(buffer, 0, buffer.Length);
                            }

                            using (var str = subStorage.OpenStream(STREAM2_NAME, true))
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

                using (var storage = part.OpenStorage(path[0], false))
                {
                    using (var subStorage = storage.OpenStorage(path[1], false))
                    {
                        subStreamsCount = subStorage.SubStreamNames.Count();

                        using (var str = subStorage.OpenStream(STREAM1_NAME, false))
                        {
                            var buffer = new byte[str.Length];

                            str.Read(buffer, 0, buffer.Length);

                            txt = Encoding.UTF8.GetString(buffer);
                        }

                        using (var str = subStorage.OpenStream(STREAM2_NAME, false))
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

            var deps = assm.Dependencies.ToArray();

            var dir = Path.GetDirectoryName(assm.Path);

            Assert.AreEqual(4, deps.Length);
            Assert.That(deps.All(d => !d.IsCommitted));
            Assert.That(deps.Any(d => string.Equals(d.Path, Path.Combine(dir, "Part4-1 (XYZ).SLDPRT"))));
            Assert.That(deps.Any(d => string.Equals(d.Path, Path.Combine(dir, "Assem1.SLDASM"))));
            Assert.That(deps.Any(d => string.Equals(d.Path, Path.Combine(dir, "Assem2.SLDASM"))));
            Assert.That(deps.Any(d => string.Equals(d.Path, Path.Combine(dir, "Part1.SLDPRT"))));
        }

        //NOTE: SW 2024 - 3D interconnect files are not listed in the dependencies (including Find References from the UI)
        [Test]
        public void DocumentDependencies3DInterconnect()
        {
            Dictionary<string, bool> r1;

            using (var assm = OpenDataDocument(@"Assembly9\Assem1.SLDASM"))
            {
                var deps = m_App.Documents.Active.Dependencies.TryIterateAll().ToArray();
                r1 = deps.ToDictionary(d => Path.GetFileName(d.Path), d => d.IsCommitted, StringComparer.CurrentCultureIgnoreCase);
            }

            Assert.AreEqual(2, r1.Count);
            Assert.That(r1.ContainsKey("Part2.sldprt"));
            Assert.That(r1.ContainsKey("Part1.prt.sldprt"));
            Assert.IsTrue(r1["Part2.sldprt"]);
            Assert.IsTrue(r1["Part1.prt.sldprt"]);
        }

        [Test]
        public void DocumentDependencies3DInterconnectUnloaded()
        {
            Dictionary<string, bool> r1;

            var assm = m_App.Documents.PreCreate<ISwAssembly>();
            assm.Path = GetFilePath(@"Assembly9\Assem1.SLDASM");

            var deps = assm.Dependencies.TryIterateAll().ToArray();
            r1 = deps.ToDictionary(d => Path.GetFileName(d.Path), d => d.IsCommitted, StringComparer.CurrentCultureIgnoreCase);

            Assert.AreEqual(2, r1.Count);
            Assert.That(r1.ContainsKey("Part2.sldprt"));
            Assert.That(r1.ContainsKey("Part1.prt.sldprt"));
            Assert.IsFalse(r1["Part2.sldprt"]);
            Assert.IsFalse(r1["Part1.prt.sldprt"]);
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

            Assert.AreEqual(4, depsData.Count);
            Assert.IsTrue(depsData[Path.Combine(dir, "Part4-1 (XYZ).SLDPRT")]);
            Assert.IsFalse(depsData[Path.Combine(dir, "Assem1.SLDASM")]);
            Assert.IsTrue(depsData[Path.Combine(dir, "Assem2.SLDASM")]);
            Assert.IsTrue(depsData[Path.Combine(dir, "Part1.SLDPRT")]);
        }

        [Test]
        public void DocumentDependenciesViewOnlyTest()
        {
            string dir = "";
            Dictionary<string, bool> depsData;

            var spec = m_App.Sw.GetOpenDocSpec(GetFilePath(@"Assembly2\TopAssem.SLDASM")) as IDocumentSpecification;
            spec.ViewOnly = true;
            var model = m_App.Sw.OpenDoc7(spec);

            var doc = m_App.Documents[model];

            var deps = doc.Dependencies;
            depsData = deps.ToDictionary(d => d.Path, d => d.IsCommitted, StringComparer.CurrentCultureIgnoreCase);

            dir = Path.GetDirectoryName(doc.Path);

            doc.Close();

            Assert.AreEqual(4, depsData.Count);
            Assert.That(depsData.All(d => !d.Value));
            depsData.ContainsKey(Path.Combine(dir, "Part4-1 (XYZ).SLDPRT"));
            depsData.ContainsKey(Path.Combine(dir, "Assem1.SLDASM"));
            depsData.ContainsKey(Path.Combine(dir, "Assem2.SLDASM"));
            depsData.ContainsKey(Path.Combine(dir, "Part1.SLDPRT"));
        }

        [Test]
        public void DocumentDependenciesCachedTest()
        {
            var assm = m_App.Documents.PreCreate<ISwAssembly>();
            assm.Path = GetFilePath(@"MovedNonOpenedAssembly1\TopAssembly.SLDASM");

            var deps = assm.Dependencies.ToArray();

            var dir = Path.GetDirectoryName(assm.Path);

            Assert.AreEqual(1, deps.Length);
            Assert.That(deps.All(d => !d.IsCommitted));
            Assert.That(deps.Any(d => string.Equals(d.Path, Path.Combine(dir, "Assemblies\\Assem1.SLDASM"), StringComparison.CurrentCultureIgnoreCase)));
        }

        [Test]
        public void DocumentDependenciesCachedExtFolderTest()
        {
            var assm = m_App.Documents.PreCreate<ISwAssembly>();
            assm.Path = GetFilePath(@"Assembly3\Assemblies\Assem1.SLDASM");

            var deps = assm.Dependencies.ToArray();

            var dir = GetFilePath("Assembly3");

            Assert.AreEqual(1, deps.Length);
            Assert.That(deps.All(d => !d.IsCommitted));
            Assert.That(deps.Any(d => string.Equals(d.Path, Path.Combine(dir, "Parts\\Part1.SLDPRT"), StringComparison.CurrentCultureIgnoreCase)));
        }

        [Test]
        public void DocumentAllDependenciesMissingAndVirtualTest()
        {
            using (var doc = OpenDataDocument(@"Assembly6\Assem1.SLDASM"))
            {
                var assm = m_App.Documents.Active;

                var deps = assm.Dependencies.TryIterateAll().ToArray();

                var dir = Path.GetDirectoryName(assm.Path);

                var d1 = deps.FirstOrDefault(d => string.Equals(Path.GetFileName(d.Path), "Part1^Assem1.sldprt",
                    StringComparison.CurrentCultureIgnoreCase));
                var d2 = deps.FirstOrDefault(d => string.Equals(Path.GetFileName(d.Path), "Part2.sldprt",
                    StringComparison.CurrentCultureIgnoreCase));
                var d3 = deps.FirstOrDefault(d => string.Equals(Path.GetFileName(d.Path), "Assem2.sldasm",
                    StringComparison.CurrentCultureIgnoreCase));
                var d4 = deps.FirstOrDefault(d => string.Equals(Path.GetFileName(d.Path), "Assem3^Assem1.sldasm",
                    StringComparison.CurrentCultureIgnoreCase));
                var d5 = deps.FirstOrDefault(d => string.Equals(Path.GetFileName(d.Path), "Part4.sldprt",
                    StringComparison.CurrentCultureIgnoreCase));
                var d6 = deps.FirstOrDefault(d => string.Equals(Path.GetFileName(d.Path), "Part5^Assem3_Assem1.sldprt",
                    StringComparison.CurrentCultureIgnoreCase));
                var d7 = deps.FirstOrDefault(d => string.Equals(Path.GetFileName(d.Path), "Assem4.sldasm",
                    StringComparison.CurrentCultureIgnoreCase));
                var d8 = deps.FirstOrDefault(d => string.Equals(Path.GetFileName(d.Path), "Part1^Assem4.sldprt",
                    StringComparison.CurrentCultureIgnoreCase));

                Assert.AreEqual(8, deps.Length);
                Assert.IsTrue(d1.IsAlive);
                Assert.IsTrue(d1.IsCommitted);
                Assert.Throws<OpenDocumentFailedException>(() => d2.Commit());
                Assert.Throws<OpenDocumentFailedException>(() => d3.Commit());
                Assert.IsTrue(d4.IsAlive);
                Assert.IsTrue(d4.IsCommitted);
                Assert.IsTrue(d5.IsAlive);
                Assert.IsTrue(d5.IsCommitted);
                Assert.That(string.Equals(d5.Path, Path.Combine(dir, "Part4.SLDPRT"), StringComparison.CurrentCultureIgnoreCase));
                Assert.IsTrue(d6.IsAlive);
                Assert.IsTrue(d6.IsCommitted);
                Assert.IsTrue(d7.IsCommitted);
                Assert.That(string.Equals(d7.Path, Path.Combine(dir, "Assem4.sldasm"), StringComparison.CurrentCultureIgnoreCase));
                Assert.IsTrue(d8.IsAlive);
                Assert.IsTrue(d8.IsCommitted);
            }
        }

        [Test]
        public void DocumentAllDependenciesMovedPath()
        {
            var destPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            Directory.CreateDirectory(destPath);

            var srcPath = GetFilePath(@"Assembly6");

            foreach (var srcFile in Directory.GetFiles(srcPath, "*.*"))
            {
                File.Copy(srcFile, Path.Combine(destPath, Path.GetFileName(srcFile)));
            }

            using (var doc = OpenDataDocument(Path.Combine(destPath, "Assem1.SLDASM")))
            {
                var assm = m_App.Documents.Active;

                var deps = assm.Dependencies.TryIterateAll().ToArray();

                var d1 = deps.FirstOrDefault(d => string.Equals(Path.GetFileName(d.Path), "Part1^Assem1.sldprt",
                    StringComparison.CurrentCultureIgnoreCase));
                var d2 = deps.FirstOrDefault(d => string.Equals(Path.GetFileName(d.Path), "Part2.sldprt",
                    StringComparison.CurrentCultureIgnoreCase));
                var d3 = deps.FirstOrDefault(d => string.Equals(Path.GetFileName(d.Path), "Assem2.sldasm",
                    StringComparison.CurrentCultureIgnoreCase));
                var d4 = deps.FirstOrDefault(d => string.Equals(Path.GetFileName(d.Path), "Assem3^Assem1.sldasm",
                    StringComparison.CurrentCultureIgnoreCase));
                var d5 = deps.FirstOrDefault(d => string.Equals(Path.GetFileName(d.Path), "Part4.sldprt",
                    StringComparison.CurrentCultureIgnoreCase));
                var d6 = deps.FirstOrDefault(d => string.Equals(Path.GetFileName(d.Path), "Part5^Assem3_Assem1.sldprt",
                    StringComparison.CurrentCultureIgnoreCase));
                var d7 = deps.FirstOrDefault(d => string.Equals(Path.GetFileName(d.Path), "Assem4.sldasm",
                    StringComparison.CurrentCultureIgnoreCase));
                var d8 = deps.FirstOrDefault(d => string.Equals(Path.GetFileName(d.Path), "Part1^Assem4.sldprt",
                    StringComparison.CurrentCultureIgnoreCase));

                Assert.AreEqual(8, deps.Length);

                Assert.IsNotNull(d1);
                Assert.IsNotNull(d2);
                Assert.IsNotNull(d3);
                Assert.IsNotNull(d4);
                Assert.IsNotNull(d5);
                Assert.IsNotNull(d6);
                Assert.IsNotNull(d7);
                Assert.IsNotNull(d8);

                Assert.IsTrue(d1.IsCommitted);
                Assert.IsFalse(d2.IsCommitted);
                Assert.IsFalse(d3.IsCommitted);
                Assert.IsTrue(d4.IsCommitted);
                Assert.IsTrue(d5.IsCommitted);
                Assert.IsTrue(d6.IsCommitted);
                Assert.IsTrue(d7.IsCommitted);
                Assert.IsTrue(d8.IsCommitted);

                Assert.That(string.Equals(Path.GetFileName(d2.Path), "Part2.SLDPRT", StringComparison.CurrentCultureIgnoreCase));
                Assert.That(string.Equals(Path.GetFileName(d3.Path), "Assem2.sldasm", StringComparison.CurrentCultureIgnoreCase));
                Assert.Throws<OpenDocumentFailedException>(() => d2.Commit());
                Assert.Throws<OpenDocumentFailedException>(() => d3.Commit());
                //Assert.That(string.Equals(d5.Path, Path.Combine(destPath, "Part4.SLDPRT"), StringComparison.CurrentCultureIgnoreCase));//NOTE: SOLIDWORKS does not follow the path resolution for the components of virtual component
                Assert.That(string.Equals(d7.Path, Path.Combine(destPath, "Assem4.sldasm"), StringComparison.CurrentCultureIgnoreCase));
            }

            try
            {
                Directory.Delete(destPath, true);
            }
            catch 
            {
            }
        }

        [Test]
        public void DocumentDependenciesCopiedFilesUnloadedTest()
        {
            var tempPath = Path.Combine(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));

            Dictionary<string, bool> refs;

            var destPath = Path.Combine(tempPath, "_Assembly11");
            var tempSrcAssmPath = Path.Combine(tempPath, "Assembly11");

            try
            {
                var srcPath = GetFilePath("Assembly11");

                CopyDirectory(srcPath, tempSrcAssmPath);
                UpdateSwReferences(tempSrcAssmPath, "TopLevel\\Assem1.sldasm", "SubAssemblies\\Assem3.SLDASM", "SubAssemblies\\A\\Assem2.SLDASM");

                CopyDirectory(tempSrcAssmPath, destPath);

                File.Delete(Path.Combine(destPath, "Parts\\Part4.sldprt"));
                File.Delete(Path.Combine(destPath, "SubAssemblies\\Part2.sldprt"));
                File.Delete(Path.Combine(tempSrcAssmPath, "Parts\\Part4.sldprt"));

                var assm = m_App.Documents.PreCreate<ISwAssembly>();
                assm.Path = Path.Combine(destPath, "TopLevel\\Assem1.sldasm");

                var deps = assm.Dependencies.TryIterateAll().ToArray();

                refs = deps.ToDictionary(x => x.Path, x => x.IsCommitted, StringComparer.CurrentCultureIgnoreCase);

                foreach (var refDoc in assm.Dependencies.TryIterateAll().ToArray())
                {
                    if (refDoc.IsCommitted && refDoc.IsAlive)
                    {
                        refDoc.Close();
                    }
                }
            }
            finally
            {
                try
                {
                    Directory.Delete(tempPath, true);
                }
                catch //folder can be locked by SW while files can be deleted
                {
                    foreach (var file in Directory.GetFiles(tempPath, "*.*", SearchOption.AllDirectories))
                    {
                        File.Delete(file);
                    }
                }
            }

            Assert.AreEqual(8, refs.Count);

            var virtComp = refs.FirstOrDefault(x => x.Key.EndsWith("Part6^Assem1.sldprt", StringComparison.CurrentCultureIgnoreCase));

            Assert.AreEqual(refs[Path.Combine(destPath, @"SubAssemblies\A\Assem2.SLDASM")], false);
            Assert.AreEqual(refs[Path.Combine(destPath, @"Parts\Part1.SLDPRT")], false);
            Assert.AreEqual(refs[Path.Combine(destPath, @"SubAssemblies\Assem3.SLDASM")], false);
            Assert.That(!string.IsNullOrEmpty(virtComp.Key));
            Assert.AreEqual(virtComp.Value, false);
            Assert.AreEqual(refs[Path.Combine(destPath, @"SubAssemblies\A\Part3.SLDPRT")], false);
            Assert.AreEqual(refs[Path.Combine(tempSrcAssmPath, @"Parts\Part4.SLDPRT")], false);
            Assert.AreEqual(refs[Path.Combine(tempSrcAssmPath, @"SubAssemblies\Part2.SLDPRT")], false);
            Assert.AreEqual(refs[Path.Combine(destPath, @"SubAssemblies\Part5.SLDPRT")], false);
        }

        protected void UpdateSwReferences(string destPath, params string[] assmRelPaths)
        {
            foreach (var assmPath in assmRelPaths)
            {
                using (var doc = (ISwDocument)m_App.Documents.Open(Path.Combine(destPath, assmPath)))
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

            m_App.Sw.CloseAllDocuments(true);
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

        [Test]
        public void OpenConflictTest()
        {
            var filePath = GetFilePath(@"Assembly1\Part1.SLDPRT");

            using (var doc = OpenDataDocument(filePath))
            {
                var p0 = m_App.Documents.Active;

                var p1 = m_App.Documents.PreCreate<ISwPart>();
                p1.Path = filePath;

                p1.Commit();

                var p2 = m_App.Documents.PreCreate<IXUnknownDocument>();
                p2.Path = filePath;
                p2.Commit();

                var p3 = p2.GetSpecific();

                Assert.IsTrue(p1.IsCommitted);
                Assert.That(string.Equals(p1.Path, filePath, StringComparison.CurrentCultureIgnoreCase));
                Assert.AreEqual(p0, p3);
            }
        }

        [Test]
        public void SaveAsTest()
        {
            var tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".sldprt");

            var curDocFilePath = "";

            using (var doc = NewDocument(swDocumentTypes_e.swDocPART))
            {
                var part = m_App.Documents.Active;
                part.SaveAs(tempFilePath);

                curDocFilePath = m_App.Sw.IActiveDoc2.GetPathName();
            }

            var exists = File.Exists(tempFilePath);

            File.Delete(tempFilePath);

            Assert.IsTrue(exists);
            Assert.That(string.Equals(tempFilePath, curDocFilePath, StringComparison.CurrentCultureIgnoreCase));
        }

        [Test]
        public void SaveTest()
        {
            var tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".sldprt");

            File.Copy(GetFilePath("Configs1.SLDPRT"), tempFilePath);

            bool isDirty;

            using (var doc = OpenDataDocument(tempFilePath, false))
            {
                var part = m_App.Documents.Active;

                part.Model.SetSaveFlag();

                part.Save();

                isDirty = part.Model.GetSaveFlag();
            }

            Exception saveEx1 = null;

            using (var doc = NewDocument(swDocumentTypes_e.swDocPART))
            {
                var part = m_App.Documents.Active;
                try
                {
                    part.Save();
                }
                catch (Exception ex)
                {
                    saveEx1 = ex;
                }
            }

            Exception saveEx2 = null;

            using (var doc = OpenDataDocument(tempFilePath, true))
            {
                var part = m_App.Documents.Active;
                try
                {
                    part.Save();
                }
                catch (Exception ex)
                {
                    saveEx2 = ex;
                }
            }

            File.Delete(tempFilePath);

            Assert.IsFalse(isDirty);
            Assert.That(saveEx1 is SaveNeverSavedDocumentException);
            Assert.That(saveEx2 is SaveDocumentFailedException);
            Assert.That(((swFileSaveError_e)(saveEx2 as SaveDocumentFailedException).ErrorCode).HasFlag(swFileSaveError_e.swReadOnlySaveError));
        }

        [Test]
        public void NewDocumentTest()
        {
            var part1 = m_App.Documents.PreCreate<ISwPart>();
            part1.Template = GetFilePath("Template_2020.prtdot");
            part1.Commit();

            var contains1 = m_App.Documents.Contains(part1, new XObjectEqualityComparer<IXDocument>());

            var featName = part1.Model.Extension.GetLastFeatureAdded().Name;

            part1.Close();

            var contains2 = m_App.Documents.Contains(part1, new XObjectEqualityComparer<IXDocument>());

            var part2 = m_App.Documents.PreCreate<ISwPart>();
            part2.Commit();

            var contains3 = m_App.Documents.Contains(part2, new XObjectEqualityComparer<IXDocument>());

            var model = part2.Model;

            part2.Close();

            var contains4 = m_App.Documents.Contains(part2, new XObjectEqualityComparer<IXDocument>());

            var part3unk = m_App.Documents.PreCreate<IXUnknownDocument>();
            part3unk.Template = GetFilePath("Template_2020.prtdot");
            part3unk.Commit();
            var part3 = part3unk.GetSpecific();

            var contains5 = m_App.Documents.Contains(part3, new XObjectEqualityComparer<IXDocument>());

            part3.Close();

            var contains6 = m_App.Documents.Contains(part3, new XObjectEqualityComparer<IXDocument>());

            Assert.AreEqual("__TemplateSketch__", featName);
            Assert.IsNotNull(model);
            Assert.IsTrue(contains1);
            Assert.IsFalse(contains2);
            Assert.IsTrue(contains3);
            Assert.IsFalse(contains4);
            Assert.IsTrue(contains5);
            Assert.IsFalse(contains6);
        }

        [Test]
        public void DocumentLoadingEventsTest()
        {
            ISwAssembly assm = null;
            ISwPart part = null;

            var openEvents = new List<Tuple<string, int>>();

            const int LOADED = 0;
            const int NEW = 1;
            const int OPENED = 2;

            string newTitle = "";

            void OnDocumentLoaded(IXDocument x) { openEvents.Add(new Tuple<string, int>(string.IsNullOrEmpty(x.Path) ? x.Title : x.Path, LOADED)); };
            void OnDocumentOpened(IXDocument x) { openEvents.Add(new Tuple<string, int>(x.Path, OPENED)); };
            void OnNewDocumentCreated(IXDocument x) { newTitle = x.Title; openEvents.Add(new Tuple<string, int>(x.Title, NEW)); };

            try
            {
                m_App.Documents.DocumentLoaded += OnDocumentLoaded;
                m_App.Documents.DocumentOpened += OnDocumentOpened;
                m_App.Documents.NewDocumentCreated += OnNewDocumentCreated;

                assm = m_App.Documents.PreCreate<ISwAssembly>();
                assm.Path = GetFilePath(@"Assembly1\TopAssem1.SLDASM");
                assm.Commit();

                part = m_App.Documents.PreCreate<ISwPart>();
                part.Commit();

                Assert.AreEqual(11, openEvents.Count);
                Assert.AreEqual(1, openEvents.Where(x => string.Equals(x.Item1, GetFilePath(@"Assembly1\Part1.SLDPRT"), StringComparison.CurrentCultureIgnoreCase) && x.Item2 == LOADED).Count());
                Assert.AreEqual(1, openEvents.Where(x => string.Equals(x.Item1, GetFilePath(@"Assembly1\Part2.SLDPRT"), StringComparison.CurrentCultureIgnoreCase) && x.Item2 == LOADED).Count());
                Assert.AreEqual(1, openEvents.Where(x => string.Equals(x.Item1, GetFilePath(@"Assembly1\Part3.SLDPRT"), StringComparison.CurrentCultureIgnoreCase) && x.Item2 == LOADED).Count());
                Assert.AreEqual(1, openEvents.Where(x => string.Equals(x.Item1, GetFilePath(@"Assembly1\Part4.SLDPRT"), StringComparison.CurrentCultureIgnoreCase) && x.Item2 == LOADED).Count());
                Assert.AreEqual(1, openEvents.Where(x => string.Equals(x.Item1, GetFilePath(@"Assembly1\SubAssem1.SLDASM"), StringComparison.CurrentCultureIgnoreCase) && x.Item2 == LOADED).Count());
                Assert.AreEqual(1, openEvents.Where(x => string.Equals(x.Item1, GetFilePath(@"Assembly1\SubAssem2.SLDASM"), StringComparison.CurrentCultureIgnoreCase) && x.Item2 == LOADED).Count());
                Assert.AreEqual(1, openEvents.Where(x => string.Equals(x.Item1, GetFilePath(@"Assembly1\SubSubAssem1.SLDASM"), StringComparison.CurrentCultureIgnoreCase) && x.Item2 == LOADED).Count());
                Assert.AreEqual(1, openEvents.Where(x => string.Equals(x.Item1, GetFilePath(@"Assembly1\TopAssem1.SLDASM"), StringComparison.CurrentCultureIgnoreCase) && x.Item2 == LOADED).Count());
                Assert.AreEqual(1, openEvents.Where(x => string.Equals(x.Item1, GetFilePath(@"Assembly1\TopAssem1.SLDASM"), StringComparison.CurrentCultureIgnoreCase) && x.Item2 == OPENED).Count());
                Assert.AreEqual(1, openEvents.Where(x => string.Equals(x.Item1, newTitle, StringComparison.CurrentCultureIgnoreCase) && x.Item2 == NEW).Count());
                Assert.AreEqual(1, openEvents.Where(x => string.Equals(x.Item1, newTitle, StringComparison.CurrentCultureIgnoreCase) && x.Item2 == LOADED).Count());
                Assert.AreEqual(1, openEvents.Where(x => x.Item2 == OPENED).Count());
                Assert.AreEqual(1, openEvents.Where(x => x.Item2 == NEW).Count());
                Assert.That(openEvents.FindIndex(x => string.Equals(x.Item1, GetFilePath(@"Assembly1\TopAssem1.SLDASM"), StringComparison.CurrentCultureIgnoreCase) && x.Item2 == OPENED) > openEvents.FindIndex(x => string.Equals(x.Item1, GetFilePath(@"Assembly1\TopAssem1.SLDASM"), StringComparison.CurrentCultureIgnoreCase) && x.Item2 == LOADED));
                Assert.That(openEvents.FindIndex(x => string.Equals(x.Item1, newTitle, StringComparison.CurrentCultureIgnoreCase) && x.Item2 == NEW) > openEvents.FindIndex(x => string.Equals(x.Item1, newTitle, StringComparison.CurrentCultureIgnoreCase) && x.Item2 == LOADED));
            }
            finally
            {
                if (assm?.IsCommitted == true)
                {
                    assm.Close();
                }

                if (part?.IsCommitted == true)
                {
                    part.Close();
                }

                m_App.Documents.DocumentLoaded -= OnDocumentLoaded;
                m_App.Documents.DocumentOpened -= OnDocumentOpened;
                m_App.Documents.NewDocumentCreated -= OnNewDocumentCreated;
            }
        }

        [Test]
        public void DeadPointerTest()
        {
            var isAlive1 = false;
            var isAlive2 = false;

            var part1 = m_App.Documents.PreCreate<ISwPart>();
            part1.Path = GetFilePath("Configs1.SLDPRT");
            part1.Commit();
            part1.Close();
            isAlive1 = part1.IsAlive;

            var part2 = m_App.Documents.PreCreate<ISwPart>();
            part2.Commit();
            isAlive2 = part2.IsAlive;

            Assert.That(() => { var doc = m_App.Documents[part1.Model]; }, Throws.Exception);
            Assert.IsFalse(isAlive1);
            Assert.IsTrue(isAlive2);

            part2.Close();
        }

        [Test]
        public void VersionTest()
        {
            var part1 = m_App.Documents.PreCreate<ISwPart>();
            part1.Path = GetFilePath("Part_2020.sldprt");

            var v1 = part1.Version;
            ISwVersion v2;
            ISwVersion v3;
            ISwVersion v4;
            ISwVersion v5;

            using (var doc = OpenDataDocument("Part_2020.sldprt"))
            {
                var part2 = m_App.Documents.Active;
                v2 = part2.Version;
            }

            using (var doc = NewDocument(swDocumentTypes_e.swDocPART))
            {
                var part3 = m_App.Documents.Active;
                v3 = part3.Version;
            }

            using (var doc = OpenDataDocument("Part_2019.sldprt"))
            {
                var part4 = m_App.Documents.Active;
                v4 = part4.Version;
            }

            var part5 = m_App.Documents.PreCreate<ISwPart>();
            part5.Path = GetFilePath("Part_2019.sldprt");
            v5 = part5.Version;

            Assert.AreEqual(SwVersion_e.Sw2020, v1.Major);
            Assert.AreEqual(SwVersion_e.Sw2020, v2.Major);
            Assert.AreEqual(m_App.Version.Major, v3.Major);
            Assert.AreEqual(SwVersion_e.Sw2019, v4.Major);
            Assert.AreEqual(SwVersion_e.Sw2019, v5.Major);
        }

        [Test]
        public void SerializationTest()
        {
            var isCylFace = false;
            var areEqual = false;

            using (var doc = OpenDataDocument("Selections1.SLDPRT"))
            {
                var part = (ISwPart)m_App.Documents.Active;

                var face = part.CreateObjectFromDispatch<ISwFace>(
                    part.Part.GetEntityByName("Face2", (int)swSelectType_e.swSelFACES) as IFace2);

                byte[] bytes;

                using (var memStr = new MemoryStream())
                {
                    face.Serialize(memStr);
                    bytes = memStr.ToArray();
                }

                using (var memStr = new MemoryStream(bytes))
                {
                    var face1 = part.DeserializeObject<ISwFace>(memStr);
                    isCylFace = face is ISwCylindricalFace;
                    areEqual = face1.Dispatch == face.Dispatch;
                }
            }

            Assert.IsTrue(isCylFace);
            Assert.IsTrue(areEqual);
        }

        [Test]
        public void QuantityTest()
        {
            double q1;
            double q2;
            double q3;
            double q4;
            double q5;

            using (var doc = OpenDataDocument("PartQty.SLDPRT"))
            {
                var part = (IXDocument3D)m_App.Documents.Active;

                q1 = part.Configurations["Conf1"].Quantity;
                q2 = part.Configurations["Conf2"].Quantity;
                q3 = part.Configurations["Conf3"].Quantity;
                q4 = part.Configurations["Conf4"].Quantity;
                q5 = part.Configurations["Conf5"].Quantity;
            }

            Assert.AreEqual(2, q1);
            Assert.AreEqual(1, q2);
            Assert.AreEqual(3, q3);
            Assert.AreEqual(2, q4);
            Assert.AreEqual(1, q5);
        }

        [Test]
        public void OpenNativeTest()
        {
            bool r1;
            bool r2;
            bool r3;
            bool r4;
            bool r5;
            bool r6;
            bool r7;
            bool r8;

            var a1 = m_App.Documents.PreCreateFromPath(GetFilePath(@"Native\Assembly.SLDASM"));
            a1.State = DocumentState_e.ReadOnly;
            a1.Commit();
            r1 = a1.IsAlive;
            a1.Close();

            var b1 = m_App.Documents.PreCreateFromPath(GetFilePath(@"Native\Block.SLDBLK"));
            b1.State = DocumentState_e.ReadOnly;
            b1.Commit();
            r2 = b1.IsAlive;
            b1.Close();

            var d1 = m_App.Documents.PreCreateFromPath(GetFilePath(@"Native\Drawing.SLDDRW"));
            d1.State = DocumentState_e.ReadOnly;
            d1.Commit();
            r3 = d1.IsAlive;
            d1.Close();

            var l1 = m_App.Documents.PreCreateFromPath(GetFilePath(@"Native\LibFeatPart.SLDLFP"));
            l1.State = DocumentState_e.ReadOnly;
            l1.Commit();
            r4 = l1.IsAlive;
            l1.Close();

            var p1 = m_App.Documents.PreCreateFromPath(GetFilePath(@"Native\Part.SLDPRT"));
            p1.State = DocumentState_e.ReadOnly;
            p1.Commit();
            r5 = p1.IsAlive;
            p1.Close();

            var at1 = m_App.Documents.PreCreateFromPath(GetFilePath(@"Native\TemplateAssembly.ASMDOT"));
            at1.State = DocumentState_e.ReadOnly;
            at1.Commit();
            r6 = at1.IsAlive;
            at1.Close();

            var dt1 = m_App.Documents.PreCreateFromPath(GetFilePath(@"Native\TemplateDrawing.DRWDOT"));
            dt1.State = DocumentState_e.ReadOnly;
            dt1.Commit();
            r7 = dt1.IsAlive;
            dt1.Close();

            var pt1 = m_App.Documents.PreCreateFromPath(GetFilePath(@"Native\TemplatePart.PRTDOT"));
            pt1.State = DocumentState_e.ReadOnly;
            pt1.Commit();
            r8 = pt1.IsAlive;
            pt1.Close();

            Assert.IsInstanceOf<IXAssembly>(a1);
            Assert.IsInstanceOf<IXPart>(b1);
            Assert.IsInstanceOf<IXDrawing>(d1);
            Assert.IsInstanceOf<IXPart>(l1);
            Assert.IsInstanceOf<IXPart>(p1);
            Assert.IsInstanceOf<IXAssembly>(at1);
            Assert.IsInstanceOf<IXDrawing>(dt1);
            Assert.IsInstanceOf<IXPart>(pt1);

            Assert.IsTrue(r1);
            Assert.IsTrue(r2);
            Assert.IsTrue(r3);
            Assert.IsTrue(r4);
            Assert.IsTrue(r5);
            Assert.IsTrue(r6);
            Assert.IsTrue(r7);
            Assert.IsTrue(r8);
        }

        [Test]
        public void OpenNativeUnknownTest()
        {
            bool r1;
            bool r2;
            bool r3;
            bool r4;
            bool r5;
            bool r6;
            bool r7;
            bool r8;

            var a1 = m_App.Documents.PreCreate<IXDocument>();
            a1.Path = GetFilePath(@"Native\Assembly.SLDASM");
            a1.State = DocumentState_e.ReadOnly;
            a1.Commit();
            var a1_1 = ((IXUnknownDocument)a1).GetSpecific();
            r1 = a1_1.IsAlive;
            a1.Close();

            var b1 = m_App.Documents.PreCreate<IXDocument>();
            b1.Path = GetFilePath(@"Native\Block.SLDBLK");
            b1.State = DocumentState_e.ReadOnly;
            b1.Commit();
            var b1_1 = ((IXUnknownDocument)b1).GetSpecific();
            r2 = b1_1.IsAlive;
            b1.Close();

            var d1 = m_App.Documents.PreCreate<IXDocument>();
            d1.Path = GetFilePath(@"Native\Drawing.SLDDRW");
            d1.State = DocumentState_e.ReadOnly;
            d1.Commit();
            var d1_1 = ((IXUnknownDocument)d1).GetSpecific();
            r3 = d1_1.IsAlive;
            d1.Close();

            var l1 = m_App.Documents.PreCreate<IXDocument>();
            l1.Path = GetFilePath(@"Native\LibFeatPart.SLDLFP");
            l1.State = DocumentState_e.ReadOnly;
            l1.Commit();
            var l1_1 = ((IXUnknownDocument)l1).GetSpecific();
            r4 = l1_1.IsAlive;
            l1.Close();

            var p1 = m_App.Documents.PreCreate<IXDocument>();
            p1.Path = GetFilePath(@"Native\Part.SLDPRT");
            p1.State = DocumentState_e.ReadOnly;
            p1.Commit();
            var p1_1 = ((IXUnknownDocument)p1).GetSpecific();
            r5 = p1_1.IsAlive;
            p1.Close();

            var at1 = m_App.Documents.PreCreate<IXDocument>();
            at1.Path = GetFilePath(@"Native\TemplateAssembly.ASMDOT");
            at1.State = DocumentState_e.ReadOnly;
            at1.Commit();
            var at1_1 = ((IXUnknownDocument)at1).GetSpecific();
            r6 = at1_1.IsAlive;
            at1.Close();

            var dt1 = m_App.Documents.PreCreate<IXDocument>();
            dt1.Path = GetFilePath(@"Native\TemplateDrawing.DRWDOT");
            dt1.State = DocumentState_e.ReadOnly;
            dt1.Commit();
            var dt1_1 = ((IXUnknownDocument)dt1).GetSpecific();
            r7 = dt1_1.IsAlive;
            dt1.Close();

            var pt1 = m_App.Documents.PreCreate<IXDocument>();
            pt1.Path = GetFilePath(@"Native\TemplatePart.PRTDOT");
            pt1.State = DocumentState_e.ReadOnly;
            pt1.Commit();
            var pt1_1 = ((IXUnknownDocument)pt1).GetSpecific();
            r8 = pt1_1.IsAlive;
            pt1.Close();

            Assert.IsInstanceOf<IXAssembly>(a1_1);
            Assert.IsInstanceOf<IXPart>(b1_1);
            Assert.IsInstanceOf<IXDrawing>(d1_1);
            Assert.IsInstanceOf<IXPart>(l1_1);
            Assert.IsInstanceOf<IXPart>(p1_1);
            Assert.IsInstanceOf<IXAssembly>(at1_1);
            Assert.IsInstanceOf<IXDrawing>(dt1_1);
            Assert.IsInstanceOf<IXPart>(pt1_1);

            Assert.IsTrue(r1);
            Assert.IsTrue(r2);
            Assert.IsTrue(r3);
            Assert.IsTrue(r4);
            Assert.IsTrue(r5);
            Assert.IsTrue(r6);
            Assert.IsTrue(r7);
            Assert.IsTrue(r8);
        }

        [Test]
        public void OpenAssemblyLightweight()
        {
            int lightweightCompsCount1;
            int lightweightCompsCount2;

            var autoLoadLw = m_App.Sw.GetUserPreferenceToggle((int)swUserPreferenceToggle_e.swAutoLoadPartsLightweight);

            try
            {
                m_App.Sw.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swAutoLoadPartsLightweight, true);

                var assm1 = m_App.Documents.PreCreate<ISwAssembly>();
                assm1.Path = GetFilePath(@"Assembly2\TopAssem.SLDASM");
                assm1.State = DocumentState_e.Default;
                assm1.Commit();
                lightweightCompsCount1 = assm1.Assembly.GetLightWeightComponentCount();
                assm1.Close();

                var assm2 = m_App.Documents.PreCreate<ISwAssembly>();
                m_App.Sw.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swAutoLoadPartsLightweight, false);
                assm2.Path = GetFilePath(@"Assembly2\TopAssem.SLDASM");
                assm2.State = DocumentState_e.Lightweight;
                assm2.Commit();
                lightweightCompsCount2 = assm2.Assembly.GetLightWeightComponentCount();
                assm2.Close();
            }
            finally
            {
                m_App.Sw.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swAutoLoadPartsLightweight, autoLoadLw);
            }

            Assert.AreEqual(0, lightweightCompsCount1);
            Assert.AreNotEqual(0, lightweightCompsCount2);
        }

        [Test]
        public void CommitCachedFeatures()
        {
            var doc = m_App.Documents.PreCreatePart();

            try
            {
                var dumbBodyFeat = doc.Features.PreCreateDumbBody();
                dumbBodyFeat.BaseBody = m_App.MemoryGeometryBuilder.CreateSolidBox(new Point(0, 0, 0), new Vector(1, 0, 0), new Vector(0, 1, 0), 0.1, 0.2, 0.3).Bodies.First();
                doc.Features.Add(dumbBodyFeat);
                doc.Commit();

                var bodyCount = doc.Bodies.Count;
                var vol = doc.Bodies.OfType<IXSolidBody>().First().Volume;

                Assert.AreEqual(1, bodyCount);
                Assert.That(vol, Is.EqualTo(0.006).Within(0.00000000001).Percent);
            }
            finally
            {
                doc.Dispose();
            }
        }

        [Test]
        public void OperationGroupTest()
        {
            string lastFeatName;
            int featsCount;

            using (var doc = OpenDataDocument(@"Drawing9\Part1.SLDPRT"))
            {
                var part = (ISwPart)m_App.Documents.Active;

                using (var oper = part.CreateOperationGroup("_Temp", true))
                {
                    var feat1 = part.Features.Last();

                    part.Features.Remove(feat1);
                    var feat2 = part.Features.PreCreate<IXDumbBody>();
                    feat2.BaseBody = m_App.MemoryGeometryBuilder.CreateSolidBox(new Point(0, 0, 0), new Vector(1, 0, 0), new Vector(0, 1, 0), 0.01, 0.01, 0.01).Bodies.First();
                    feat2.Commit();
                    var feat3 = part.Features.PreCreate<IXDumbBody>();
                    feat3.BaseBody = m_App.MemoryGeometryBuilder.CreateSolidBox(new Point(0.2, 0.2, 0.2), new Vector(1, 0, 0), new Vector(0, 1, 0), 0.01, 0.01, 0.01).Bodies.First();
                    feat3.Commit();
                }

                featsCount = part.Features.Count;
                lastFeatName = part.Features.Last().Name;
            }

            Assert.AreEqual("3DSketch2", lastFeatName);
            Assert.AreEqual(28, featsCount);
        }

        [Test]
        public void CreateFeatureEventTest()
        {
            var res1 = new List<Tuple<string, string>>();
            var res2 = new List<Tuple<string, string>>();

            string f1;
            string f2;

            const int swCommands_3DSketch = 89;

            using (var doc = OpenDataDocument(@"Drawing9\Part1.sldprt"))
            {
                var part = (ISwPart)m_App.Documents.Active;

                part.Features.FeatureCreated += (d, f) =>
                {
                    res1.Add(new Tuple<string, string>(d.Path, f.Name));
                };

                part.Model.ClearSelection2(true);

                m_App.Sw.RunCommand(swCommands_3DSketch, "");

                part.Model.SketchManager.AddToDB = true;

                part.Model.SketchManager.CreateLine(0, 0, 0, 0.1, 0.1, 0.1);

                f1 = ((IFeature)part.Model.SketchManager.ActiveSketch).Name;

                m_App.Sw.RunCommand(swCommands_3DSketch, "");
            }

            using (var doc = OpenDataDocument("Assembly1\\TopAssem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                assm.Features.FeatureCreated += (d, f) =>
                {
                    res2.Add(new Tuple<string, string>(d.Path, f.Name));
                };

                assm.Model.ClearSelection2(true);

                m_App.Sw.RunCommand(swCommands_3DSketch, "");

                assm.Model.SketchManager.AddToDB = true;

                assm.Model.SketchManager.CreateLine(0, 0, 0, 0.1, 0.1, 0.1);

                f2 = ((IFeature)assm.Model.SketchManager.ActiveSketch).Name;

                m_App.Sw.RunCommand(swCommands_3DSketch, "");
            }

            Assert.AreEqual(1, res1.Count);
            Assert.AreEqual(GetFilePath(@"Drawing9\Part1.sldprt").ToLower(), res1[0].Item1.ToLower());
            Assert.AreEqual(f1, res1[0].Item2);

            Assert.AreEqual(1, res2.Count);
            Assert.AreEqual(GetFilePath("Assembly1\\TopAssem1.SLDASM").ToLower(), res2[0].Item1.ToLower());
            Assert.AreEqual(f2, res2[0].Item2);
        }

        private class DocumentHandlerMock : IDocumentHandler
        {
            private readonly List<string> m_InitList;
            private readonly List<string> m_DisposeList;

            private IXDocument m_Doc;

            internal DocumentHandlerMock(List<string> initList, List<string> disposeList) 
            {
                m_InitList = initList;
                m_DisposeList = disposeList;
            }

            public void Init(IXApplication app, IXDocument model)
            {
                m_InitList.Add(model.Path);
                m_Doc = model;
            }

            public void Dispose()
            {
                m_DisposeList.Add(m_Doc.Path);
            }
        }

        [Test]
        public void DocumentHandlerTest()
        {
            var initList = new List<string>();
            var disposeList = new List<string>();

            int initCount1;
            int dispCount1;
            int dispCount2;

            var part1 = m_App.Documents.Open(GetFilePath("Assembly4\\Part1.sldprt"));

            m_App.Documents.RegisterHandler(() => new DocumentHandlerMock(initList, disposeList));

            initCount1 = initList.Count;

            var assm = m_App.Documents.Open(GetFilePath("Assembly4\\Assembly1.SLDASM"));

            var part2 = m_App.Documents[GetFilePath("Assembly4\\Part2.sldprt")];

            part1.Close();
            dispCount1 = disposeList.Count;
            part2.Close();
            dispCount2 = disposeList.Count;
            assm.Close();

            var expRefList = new string[]
            {
                GetFilePath("Assembly4\\Part1.sldprt").ToLower(),
                GetFilePath("Assembly4\\Part2.sldprt").ToLower(),
                GetFilePath("Assembly4\\Assembly1.SLDASM").ToLower(),
                GetFilePath("Assembly4\\SubAssem1.SLDASM").ToLower(),
                GetFilePath("Assembly4\\SubSubAssem1.SLDASM").ToLower()
            };

            Assert.AreEqual(1, initCount1);
            Assert.AreEqual(0, dispCount1);
            Assert.AreEqual(0, dispCount2);
            Assert.AreEqual(5, initList.Count);
            Assert.AreEqual(5, disposeList.Count);

            CollectionAssert.AreEquivalent(expRefList, initList.Select(x => x.ToLower()));
            CollectionAssert.AreEquivalent(expRefList, disposeList.Select(x => x.ToLower()));
        }

        [Test]
        public void IdTest()
        {
            var part1IdExp = new byte[]
            {
                200, 234, 22, 103, 0, 0, 0, 0
            };

            var part2IdExp = new byte[]
            {
                63, 235, 22, 103, 0, 0, 0, 0
            };

            byte[] id1;
            byte[] id2;
            byte[] id3;
            byte[] id4;
            byte[] id5;
            byte[] id6;

            using (var doc = OpenDataDocument(@"Id\IdPart1.SLDPRT"))
            {
                var part = (IXDocument3D)m_App.Documents.Active;

                id1 = part.Id.Thumbprint;
            }

            using (var doc = OpenDataDocument(@"Id\IdPart1Copy.SLDPRT"))
            {
                var part = (IXDocument3D)m_App.Documents.Active;

                id2 = part.Id.Thumbprint;
            }

            using (var doc = OpenDataDocument(@"Id\IdPart1CopyModified.SLDPRT"))
            {
                var part = (IXDocument3D)m_App.Documents.Active;

                id3 = part.Id.Thumbprint;
            }

            using (var doc = OpenDataDocument(@"Id\IdPart1SaveAs.SLDPRT"))
            {
                var part = (IXDocument3D)m_App.Documents.Active;

                id4 = part.Id.Thumbprint;
            }

            using (var doc = OpenDataDocument(@"Id\IdPart2.SLDPRT"))
            {
                var part = (IXDocument3D)m_App.Documents.Active;

                id5 = part.Id.Thumbprint;
            }

            using (var doc = OpenDataDocument(@"Id\IdPart2Copy.SLDPRT"))
            {
                var part = (IXDocument3D)m_App.Documents.Active;

                id6 = part.Id.Thumbprint;
            }

            CollectionAssert.AreEqual(part1IdExp, id1);
            CollectionAssert.AreEqual(part1IdExp, id2);
            CollectionAssert.AreEqual(part1IdExp, id3);
            CollectionAssert.AreEqual(part1IdExp, id4);
            CollectionAssert.AreEqual(part2IdExp, id5);
            CollectionAssert.AreEqual(part2IdExp, id6);
        }
    }
}
