using NUnit.Framework;
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
using Xarial.XCad.Documents.Extensions;
using Xarial.XCad.SwDocumentManager;
using Xarial.XCad.SwDocumentManager.Documents;

namespace SolidWorksDocMgr.Tests.Integration
{
    public class DocumentTests : IntegrationTests
    {
        [Test]
        public void VersionTest()
        {
            ISwDmVersion v1;
            ISwDmVersion v2;

            using (var doc = OpenDataDocument("Part_2020.sldprt"))
            {
                var part1 = m_App.Documents.Active;
                v1 = part1.Version;
            }
            
            using (var doc = OpenDataDocument("Part_2019.sldprt"))
            {
                var part2 = m_App.Documents.Active;
                v2 = part2.Version;
            }

            Assert.AreEqual(SwDmVersion_e.Sw2020, v1.Major);
            Assert.AreEqual(SwDmVersion_e.Sw2019, v2.Major);
        }

        //This test fails when run in a group
        [Test]
        public void DocumentsTest()
        {
            var c1 = m_App.Documents.Count;
            var activeIsNull = m_App.Documents.Active == null;

            var doc1 = m_App.Documents.Open(GetFilePath("Part_2020.sldprt"), Xarial.XCad.Documents.Enums.DocumentState_e.ReadOnly);

            var c2 = m_App.Documents.Count;
            var activeIsDoc1 = m_App.Documents.Active == doc1;

            var doc2 = m_App.Documents.Open(GetFilePath("Part_2019.sldprt"), Xarial.XCad.Documents.Enums.DocumentState_e.ReadOnly);

            var c3 = m_App.Documents.Count;
            var activeIsDoc2 = m_App.Documents.Active == doc2;

            doc1.Close();
            var c4 = m_App.Documents.Count;
            var activeIsDoc21 = m_App.Documents.Active == doc2;

            doc2.Close();
            var c5 = m_App.Documents.Count;
            var activeIsNull1 = m_App.Documents.Active == null;

            Assert.AreEqual(0, c1);
            Assert.IsTrue(activeIsNull);
            Assert.AreEqual(1, c2);
            Assert.IsTrue(activeIsDoc1);
            Assert.AreEqual(2, c3);
            Assert.IsTrue(activeIsDoc2);
            Assert.AreEqual(1, c4);
            Assert.IsTrue(activeIsDoc21);
            Assert.AreEqual(0, c5);
            Assert.IsTrue(activeIsNull1);
        }

        [Test]
        public void IsAliveTest() 
        {
            bool r1;
            bool r2;
            //bool r3;
            
            var doc1 = m_App.Documents.Open(GetFilePath("Part_2020.sldprt"), Xarial.XCad.Documents.Enums.DocumentState_e.ReadOnly);
            r1 = doc1.IsAlive;
            doc1.Close();
            r2 = doc1.IsAlive;

            //doc1 = m_App.Documents.Open(GetFilePath("Part_2020.sldprt"), Xarial.XCad.Documents.Enums.DocumentState_e.ReadOnly);
            //((ISwDmDocument)doc1).Document.CloseDoc();
            //r3 = doc1.IsAlive;

            Assert.IsTrue(r1);
            Assert.IsFalse(r2);
            //Assert.IsFalse(r3);
        }

        [Test]
        public void DocumentDependenciesTest()
        {
            using (var doc = OpenDataDocument(@"Assembly2\TopAssem.SLDASM"))
            {
                var assm = m_App.Documents.Active;

                var deps = assm.Dependencies.ToArray();

                var dir = Path.GetDirectoryName(assm.Path);

                Assert.AreEqual(4, deps.Length);
                Assert.That(deps.Any(d => string.Equals(d.Path, Path.Combine(dir, "Part4-1 (XYZ).SLDPRT"))));
                Assert.That(deps.Any(d => string.Equals(d.Path, Path.Combine(dir, "Assem1.SLDASM"))));
                Assert.That(deps.Any(d => string.Equals(d.Path, Path.Combine(dir, "Assem2.SLDASM"))));
                Assert.That(deps.Any(d => string.Equals(d.Path, Path.Combine(dir, "Part1.SLDPRT"))));
            }
        }

        [Test]
        public void DocumentAllDependenciesTest()
        {
            using (var doc = OpenDataDocument(@"Assembly2\TopAssem.SLDASM"))
            {
                var assm = m_App.Documents.Active;

                var deps = assm.GetAllDependencies().ToArray();

                var dir = Path.GetDirectoryName(assm.Path);

                Assert.AreEqual(6, deps.Length);
                Assert.That(deps.Any(d => string.Equals(d.Path, Path.Combine(dir, "Part2.SLDPRT"))));
                Assert.That(deps.Any(d => string.Equals(d.Path, Path.Combine(dir, "Part3.SLDPRT"))));
                Assert.That(deps.Any(d => string.Equals(d.Path, Path.Combine(dir, "Part4-1 (XYZ).SLDPRT"))));
                Assert.That(deps.Any(d => string.Equals(d.Path, Path.Combine(dir, "Assem1.SLDASM"))));
                Assert.That(deps.Any(d => string.Equals(d.Path, Path.Combine(dir, "Assem2.SLDASM"))));
                Assert.That(deps.Any(d => string.Equals(d.Path, Path.Combine(dir, "Part1.SLDPRT"))));
            }
        }

        [Test]
        public void DocumentAllDependenciesMissingAndVirtualTest()
        {
            using (var doc = OpenDataDocument(@"Assembly6\Assem1.SLDASM"))
            {
                var assm = m_App.Documents.Active;

                var deps = assm.GetAllDependencies().ToArray();

                var dir = Path.GetDirectoryName(assm.Path);

                var d1 = deps.FirstOrDefault(d => string.Equals(Path.GetFileName(d.Path), "_temp_Part1^Assem1.sldprt",
                    StringComparison.CurrentCultureIgnoreCase));
                var d2 = deps.FirstOrDefault(d => string.Equals(Path.GetFileName(d.Path), "Part2.sldprt",
                    StringComparison.CurrentCultureIgnoreCase));
                var d3 = deps.FirstOrDefault(d => string.Equals(Path.GetFileName(d.Path), "Assem2.sldasm",
                    StringComparison.CurrentCultureIgnoreCase));
                var d4 = deps.FirstOrDefault(d => string.Equals(Path.GetFileName(d.Path), "_temp_Assem3^Assem1.sldasm",
                    StringComparison.CurrentCultureIgnoreCase));
                var d5 = deps.FirstOrDefault(d => string.Equals(Path.GetFileName(d.Path), "Part4.sldprt",
                    StringComparison.CurrentCultureIgnoreCase));
                var d6 = deps.FirstOrDefault(d => string.Equals(Path.GetFileName(d.Path), "_temp_Part5^Assem3_Assem1.sldprt",
                    StringComparison.CurrentCultureIgnoreCase));
                var d7 = deps.FirstOrDefault(d => string.Equals(Path.GetFileName(d.Path), "Assem4.sldasm",
                    StringComparison.CurrentCultureIgnoreCase));
                var d8 = deps.FirstOrDefault(d => string.Equals(Path.GetFileName(d.Path), "_temp_Part1^Assem4.sldprt",
                    StringComparison.CurrentCultureIgnoreCase));

                Assert.AreEqual(8, deps.Length);
                Assert.IsTrue(d1.IsAlive);
                //Assert.IsTrue(d1.IsCommitted);
                Assert.Throws<OpenDocumentFailedException>(() => d2.Commit());
                Assert.Throws<OpenDocumentFailedException>(() => d3.Commit());
                Assert.IsTrue(d4.IsAlive);
                //Assert.IsTrue(d4.IsCommitted);
                //Assert.IsFalse(d5.IsCommitted);
                Assert.That(string.Equals(d5.Path, Path.Combine(dir, "Part4.SLDPRT"), StringComparison.CurrentCultureIgnoreCase));
                Assert.IsTrue(d6.IsAlive);
                //Assert.IsTrue(d6.IsCommitted);
                //Assert.IsFalse(d7.IsCommitted);
                Assert.That(string.Equals(d7.Path, Path.Combine(dir, "Assem4.sldasm"), StringComparison.CurrentCultureIgnoreCase));
                Assert.IsTrue(d8.IsAlive);
                //Assert.IsTrue(d8.IsCommitted);
            }
        }

        [Test]
        public void DocumentAllDependenciesTitleTest()
        {
            string[] titles;

            using (var doc = OpenDataDocument(@"Assembly7\Assem1.SLDASM"))
            {
                var assm = m_App.Documents.Active;

                titles = assm.GetAllDependencies().Select(d => Path.GetFileNameWithoutExtension(d.Title)).ToArray();
            }

            Assert.AreEqual(7, titles.Length);

            CollectionAssert.AreEquivalent(new string[] 
            {
                "Part1^Assem1",
                "__Part2^Assem1",
                "_temp_ABC^Assem1",
                "Assem2^Assem1",
                "Part4^Assem2_Assem1",
                "Assem2",
                "Part5^Assem2"
            }, titles);
        }

        [Test]
        public void DocumentDependencies3DInterconnect()
        {
            Dictionary<string, bool> r1;

            using (var assm = OpenDataDocument(@"Assembly9\Assem1.SLDASM"))
            {
                var deps = m_App.Documents.Active.GetAllDependencies().ToArray();
                r1 = deps.ToDictionary(d => Path.GetFileName(d.Path), d => d.IsCommitted, StringComparer.CurrentCultureIgnoreCase);
            }

            Assert.AreEqual(2, r1.Count);
            Assert.That(r1.ContainsKey("Part2.sldprt"));
            Assert.That(r1.ContainsKey("Part1.prt.sldprt"));
        }

        [Test]
        public void DocumentAllDependenciesReadOnlyState()
        {
            var destPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            Directory.CreateDirectory(destPath);

            foreach (var srcFile in Directory.GetFiles(GetFilePath(@"Assembly6"), "*.*")) 
            {
                File.Copy(srcFile, Path.Combine(destPath, Path.GetFileName(srcFile)));
            }

            using (var doc = OpenDataDocument(Path.Combine(destPath, "Assem1.SLDASM"), false))
            {
                var assm = m_App.Documents.Active;

                var deps = assm.GetAllDependencies().ToArray();

                Assert.AreEqual(8, deps.Length);
                Assert.That(deps.All(d => !d.State.HasFlag(DocumentState_e.ReadOnly)));

                foreach (var dep in deps) 
                {
                    if (dep.IsCommitted)
                    {
                        dep.Close();
                    }
                }
            }

            Directory.Delete(destPath, true);

            using (var doc = OpenDataDocument(@"Assembly6\Assem1.SLDASM", true))
            {
                var assm = m_App.Documents.Active;

                var deps = assm.GetAllDependencies().ToArray();

                Assert.AreEqual(8, deps.Length);
                Assert.That(deps.All(d => d.State.HasFlag(DocumentState_e.ReadOnly)));

                foreach (var dep in deps)
                {
                    if (dep.IsCommitted)
                    {
                        dep.Close();
                    }
                }
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

                var deps = assm.GetAllDependencies().ToArray();

                var d1 = deps.FirstOrDefault(d => string.Equals(Path.GetFileName(d.Path), "_temp_Part1^Assem1.sldprt",
                    StringComparison.CurrentCultureIgnoreCase));
                var d2 = deps.FirstOrDefault(d => string.Equals(Path.GetFileName(d.Path), "Part2.sldprt",
                    StringComparison.CurrentCultureIgnoreCase));
                var d3 = deps.FirstOrDefault(d => string.Equals(Path.GetFileName(d.Path), "Assem2.sldasm",
                    StringComparison.CurrentCultureIgnoreCase));
                var d4 = deps.FirstOrDefault(d => string.Equals(Path.GetFileName(d.Path), "_temp_Assem3^Assem1.sldasm",
                    StringComparison.CurrentCultureIgnoreCase));
                var d5 = deps.FirstOrDefault(d => string.Equals(Path.GetFileName(d.Path), "Part4.sldprt",
                    StringComparison.CurrentCultureIgnoreCase));
                var d6 = deps.FirstOrDefault(d => string.Equals(Path.GetFileName(d.Path), "_temp_Part5^Assem3_Assem1.sldprt",
                    StringComparison.CurrentCultureIgnoreCase));
                var d7 = deps.FirstOrDefault(d => string.Equals(Path.GetFileName(d.Path), "Assem4.sldasm",
                    StringComparison.CurrentCultureIgnoreCase));
                var d8 = deps.FirstOrDefault(d => string.Equals(Path.GetFileName(d.Path), "_temp_Part1^Assem4.sldprt",
                    StringComparison.CurrentCultureIgnoreCase));

                Assert.AreEqual(8, deps.Length);

                Assert.IsNotNull(d1);
                Assert.IsNotNull(d4);
                Assert.IsNotNull(d6);
                Assert.IsNotNull(d8);
                Assert.That(string.Equals(d2.Path, Path.Combine(srcPath, "Part2.SLDPRT"), StringComparison.CurrentCultureIgnoreCase));
                Assert.That(string.Equals(d3.Path, Path.Combine(srcPath, "Assem2.sldasm"), StringComparison.CurrentCultureIgnoreCase));
                Assert.Throws<OpenDocumentFailedException>(() => d2.Commit());
                Assert.Throws<OpenDocumentFailedException>(() => d3.Commit());
                //Assert.That(string.Equals(d5.Path, Path.Combine(destPath, "Part4.SLDPRT"), StringComparison.CurrentCultureIgnoreCase)); - SOLIDWORKS does not follow the path resolution for the components of virtual component
                Assert.That(string.Equals(d7.Path, Path.Combine(destPath, "Assem4.sldasm"), StringComparison.CurrentCultureIgnoreCase));

                d1.Close();
                d4.Close();
                d5.Close();
                d6.Close();
                d7.Close();
                d8.Close();
            }

            Directory.Delete(destPath, true);
        }

        [Test]
        public void DocumentDependenciesCachedTest()
        {
            using (var doc = OpenDataDocument(@"MovedNonOpenedAssembly1\TopAssembly.SLDASM"))
            {
                var assm = m_App.Documents.Active;

                var deps = assm.Dependencies.ToArray();

                var dir = Path.GetDirectoryName(assm.Path);

                Assert.AreEqual(1, deps.Length);
                Assert.That(deps.All(d => !d.IsCommitted));
                Assert.That(deps.Any(d => string.Equals(d.Path, Path.Combine(dir, "Assemblies\\Assem1.SLDASM"), StringComparison.CurrentCultureIgnoreCase)));
            }
        }

        [Test]
        public void DocumentDependenciesCachedExtFolderTest()
        {
            using (var doc = OpenDataDocument(@"Assembly3\Assemblies\Assem1.SLDASM"))
            {
                var assm = m_App.Documents.Active;

                var deps = assm.Dependencies.ToArray();

                var dir = GetFilePath(@"Assembly3");

                Assert.AreEqual(1, deps.Length);
                Assert.That(deps.All(d => !d.IsCommitted));
                Assert.That(deps.Any(d => string.Equals(d.Path, Path.Combine(dir, "Parts\\Part1.SLDPRT"), StringComparison.CurrentCultureIgnoreCase)));
            }
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

            File.Copy(GetFilePath("EmptyPart1.sldprt"), tempFile);

            using (var doc = OpenDataDocument(tempFile, false))
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

                part.Save();
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

            File.Copy(GetFilePath("EmptyPart1.sldprt"), tempFile);

            using (var doc = OpenDataDocument(tempFile, false))
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

                part.Save();
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
    }
}
