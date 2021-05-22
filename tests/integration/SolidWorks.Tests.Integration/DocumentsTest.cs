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
using Xarial.XCad.Documents.Exceptions;
using Xarial.XCad.Geometry;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Documents.Exceptions;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.SolidWorks.Geometry;

namespace SolidWorks.Tests.Integration
{
    public class DocumentsTest : IntegrationTests
    {
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
            var isInCollection = m_App.Documents.Contains(doc);
            var type = doc.GetType();
            var contains1 = m_App.Documents.Contains(doc);

            doc.Close();

            var contains2 = m_App.Documents.Contains(doc);

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

            var contains1 = m_App.Documents.Contains(doc);

            doc.Close();

            var contains2 = m_App.Documents.Contains(doc);

            Assert.IsTrue(contains1);
            Assert.IsFalse(contains2);
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

            Assert.AreEqual(4, deps.Length);
            Assert.That(deps.All(d => !d.IsCommitted));
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

            var deps = assm.Dependencies;

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

            var deps = assm.Dependencies;

            var dir = GetFilePath("Assembly3");

            Assert.AreEqual(1, deps.Length);
            Assert.That(deps.All(d => !d.IsCommitted));
            Assert.That(deps.Any(d => string.Equals(d.Path, Path.Combine(dir, "Parts\\Part1.SLDPRT"), StringComparison.CurrentCultureIgnoreCase)));
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

            var contains1 = m_App.Documents.Contains(part1);

            var featName = part1.Model.Extension.GetLastFeatureAdded().Name;

            part1.Close();

            var contains2 = m_App.Documents.Contains(part1);

            var part2 = m_App.Documents.PreCreate<ISwPart>();
            part2.Commit();

            var contains3 = m_App.Documents.Contains(part2);

            var model = part2.Model;

            part2.Close();

            var contains4 = m_App.Documents.Contains(part2);

            var part3unk = m_App.Documents.PreCreate<IXUnknownDocument>();
            part3unk.Template = GetFilePath("Template_2020.prtdot");
            part3unk.Commit();
            var part3 = part3unk.GetSpecific();

            var contains5 = m_App.Documents.Contains(part3);

            part3.Close();

            var contains6 = m_App.Documents.Contains(part3);

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
            
            Assert.Throws<KeyNotFoundException>(() => { var doc = m_App.Documents[part1.Model]; });
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

                var face = SwObjectFactory.FromDispatch<ISwFace>(
                    part.Part.GetEntityByName("Face2", (int)swSelectType_e.swSelFACES) as IFace2, part);

                byte[] bytes;

                using (var memStr = new MemoryStream()) 
                {
                    face.Serialize(memStr);
                    bytes = memStr.ToArray();
                }

                using (var memStr = new MemoryStream(bytes))
                {
                    var face1 = part.DeserializeObject(memStr);
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
        public void BodyVolumeTest()
        {
            double v1;

            using (var doc = OpenDataDocument("Features1.SLDPRT"))
            {
                var part = (IXPart)m_App.Documents.Active;

                v1 = ((IXSolidBody)part.Bodies["Boss-Extrude2"]).Volume;
            }

            Assert.That(2.3851693679806192E-05, Is.EqualTo(v1).Within(0.001).Percent);
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
    }
}
