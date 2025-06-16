using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Xarial.XCad.Base;
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
                var part1 = doc.Document;
                v1 = part1.Version;
            }
            
            using (var doc = OpenDataDocument("Part_2019.sldprt"))
            {
                var part2 = doc.Document;
                v2 = part2.Version;
            }

            Assert.AreEqual(SwDmVersion_e.Sw2020, v1.Major);
            Assert.AreEqual(SwDmVersion_e.Sw2019, v2.Major);
        }

        [Test]
        public void DocumentsTest()
        {
            bool activeIsNull;
            int c1;
            int c2;
            int c3;
            int c4;
            int c5;
            bool activeIsDoc1;
            bool activeIsDoc2;
            bool activeIsDoc21;
            bool activeIsNull1;

            c1 = Application.Documents.Count;
            activeIsNull = Application.Documents.Active == null;

            using (var dataFile1 = GetDataFile("Part_2020.sldprt"))
            {
                var doc1 = Application.Documents.Open(dataFile1.FilePath, DocumentState_e.ReadOnly);

                c2 = Application.Documents.Count;
                activeIsDoc1 = Application.Documents.Active.Equals(doc1);

                using (var dataFile2 = GetDataFile("Part_2019.sldprt"))
                {
                    var doc2 = Application.Documents.Open(dataFile2.FilePath, DocumentState_e.ReadOnly);

                    c3 = Application.Documents.Count;
                    activeIsDoc2 = Application.Documents.Active.Equals(doc2);

                    doc1.Close();
                    c4 = Application.Documents.Count;
                    activeIsDoc21 = Application.Documents.Active.Equals(doc2);

                    doc2.Close();
                    c5 = Application.Documents.Count;
                    activeIsNull1 = Application.Documents.Active == null;
                }
            }

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

            using (var dataFile = GetDataFile("Part_2020.sldprt"))
            {
                var doc1 = Application.Documents.Open(dataFile.FilePath, DocumentState_e.ReadOnly);
                r1 = doc1.IsAlive;
                doc1.Close();
                r2 = doc1.IsAlive;

                //doc1 = m_App.Documents.Open(GetFilePath("Part_2020.sldprt"), Xarial.XCad.Documents.Enums.DocumentState_e.ReadOnly);
                //((ISwDmDocument)doc1).Document.CloseDoc();
                //r3 = doc1.IsAlive;

            }

            Assert.IsTrue(r1);
            Assert.IsFalse(r2);
            //Assert.IsFalse(r3);
        }

        [Test]
        public void DocumentDependenciesTest()
        {
            using (var doc = OpenDataDocument(@"Assembly2\TopAssem.SLDASM"))
            {
                var assm = doc.Document;

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
                var assm = doc.Document;

                var deps = assm.Dependencies.TryIterateAll().ToArray();

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

        //NOTE: SW 2024 - 3D interconnect files are not listed in the dependencies (including Find References from the UI)
        [Test]
        public void DocumentAllDependenciesMissingAndVirtualTest()
        {
            using (var doc = OpenDataDocument(@"Assembly6\Assem1.SLDASM"))
            {
                var assm = doc.Document;

                var deps = assm.Dependencies.TryIterateAll().ToArray();

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
                var assm = doc.Document;

                titles = assm.Dependencies.TryIterateAll().Select(d => Path.GetFileNameWithoutExtension(d.Title)).ToArray();
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
                var deps = assm.Document.Dependencies.TryIterateAll().ToArray();
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

            using (var dataFile = GetDataFile(@"Assembly6\Assem1.SLDASM"))
            {
                foreach (var srcFile in Directory.GetFiles(Path.Combine(dataFile.WorkFolderPath, "Assembly6"), "*.*"))
                {
                    File.Copy(srcFile, Path.Combine(destPath, Path.GetFileName(srcFile)));
                }
            }

            var assm = Application.Documents.Open(Path.Combine(destPath, "Assem1.SLDASM"));

            var deps = assm.Dependencies.TryIterateAll().ToArray();

            Assert.AreEqual(8, deps.Length);
            Assert.That(deps.All(d => !d.State.HasFlag(DocumentState_e.ReadOnly)));

            foreach (var dep in deps)
            {
                if (dep.IsCommitted)
                {
                    dep.Close();
                }
            }

            assm.Close();

            Directory.Delete(destPath, true);

            using (var doc = OpenDataDocument(@"Assembly6\Assem1.SLDASM", true))
            {
                assm = doc.Document;

                deps = assm.Dependencies.TryIterateAll().ToArray();

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

            using (var dataFile = GetDataFile(@"Assembly6\Assem1.SLDASM"))
            {
                foreach (var srcFile in Directory.GetFiles(Path.Combine(dataFile.WorkFolderPath, "Assembly6"), "*.*"))
                {
                    File.Copy(srcFile, Path.Combine(destPath, Path.GetFileName(srcFile)));
                }
            }

            var assm = Application.Documents.Open(Path.Combine(destPath, "Assem1.SLDASM"));

            var deps = assm.Dependencies.TryIterateAll().ToArray();

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

            d1.Close();
            d4.Close();
            d5.Close();
            d6.Close();
            d7.Close();
            d8.Close();

            assm.Close();

            Directory.Delete(destPath, true);
        }

        [Test]
        public void DocumentDependenciesCachedTest()
        {
            using (var doc = OpenDataDocument(@"MovedNonOpenedAssembly1\TopAssembly.SLDASM"))
            {
                var assm = doc.Document;

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
                var assm = doc.Document;

                var deps = assm.Dependencies.ToArray();

                var dir = Path.Combine(doc.WorkFolderPath, "Assembly3");

                Assert.AreEqual(1, deps.Length);
                Assert.That(deps.All(d => !d.IsCommitted));
                Assert.That(deps.Any(d => string.Equals(d.Path, Path.Combine(dir, "Parts\\Part1.SLDPRT"), StringComparison.CurrentCultureIgnoreCase)));
            }
        }

        [Test]
        public void DocumentDependenciesCopiedFilesTest()
        {
            var tempPath = Path.Combine(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));

            Dictionary<string, bool> refs;

            var destPath = Path.Combine(tempPath, "_Assembly11");
            var tempSrcAssmPath = Path.Combine(tempPath, "Assembly11");

            try
            {
                using (var dataFile = GetDataFile(@"Assembly11\TopLevel\Assem1.sldasm"))
                {
                    var srcPath = Path.Combine(dataFile.WorkFolderPath, "Assembly11");

                    CopyDirectory(srcPath, tempSrcAssmPath);
                    UpdateSwReferences(tempSrcAssmPath, "TopLevel\\Assem1.sldasm", "SubAssemblies\\Assem3.SLDASM", "SubAssemblies\\A\\Assem2.SLDASM");

                    CopyDirectory(tempSrcAssmPath, destPath);

                    File.Delete(Path.Combine(destPath, "Parts\\Part4.sldprt"));
                    File.Delete(Path.Combine(destPath, "SubAssemblies\\Part2.sldprt"));
                    File.Delete(Path.Combine(tempSrcAssmPath, "Parts\\Part4.sldprt"));
                }

                var assm = (ISwDmAssembly)Application.Documents.Open(Path.Combine(destPath, "TopLevel\\Assem1.sldasm"));

                try
                {
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
                    assm.Close();
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

            Assert.AreEqual(refs[Path.Combine(destPath, @"SubAssemblies\A\Assem2.SLDASM")], true);
            Assert.AreEqual(refs[Path.Combine(destPath, @"Parts\Part1.SLDPRT")], true);
            Assert.AreEqual(refs[Path.Combine(destPath, @"SubAssemblies\Assem3.SLDASM")], true);
            Assert.That(!string.IsNullOrEmpty(virtComp.Key));
            Assert.AreEqual(virtComp.Value, true);
            Assert.AreEqual(refs[Path.Combine(destPath, @"SubAssemblies\A\Part3.SLDPRT")], true);
            Assert.AreEqual(refs[Path.Combine(tempSrcAssmPath, @"Parts\Part4.SLDPRT")], false);
            Assert.AreEqual(refs[Path.Combine(tempSrcAssmPath, @"SubAssemblies\Part2.SLDPRT")], true);
            Assert.AreEqual(refs[Path.Combine(destPath, @"SubAssemblies\Part5.SLDPRT")], true);
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

            TestData result = null;

            using (var dataFile = GetDataFile("EmptyPart1.sldprt"))
            {
                var part = Application.Documents.Open(dataFile.FilePath);

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

                part.Save();
                part.Close();

                part = Application.Documents.Open(dataFile.FilePath);

                using (var stream = part.OpenStream(STREAM_NAME, false))
                {
                    var xmlSer = new XmlSerializer(typeof(TestData));
                    result = xmlSer.Deserialize(stream) as TestData;
                }
            }

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

            var subStreamsCount = 0;
            var txt = "";
            var number = 0;

            using (var dataFile = GetDataFile("EmptyPart1.sldprt"))
            {
                var part = Application.Documents.Open(dataFile.FilePath);

                var path = SUB_STORAGE_PATH.Split('\\');

                part.StorageWriteAvailable += (d) =>
                {
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

                part.Save();
                part.Close();

                part = Application.Documents.Open(dataFile.FilePath);

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
                var part = (IXDocument3D)doc.Document;
                
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

            ISwDmDocument a1;
            ISwDmDocument b1;
            ISwDmDocument d1;
            ISwDmDocument l1;
            ISwDmDocument p1;
            ISwDmDocument at1;
            ISwDmDocument dt1;
            ISwDmDocument pt1;

            using (var dataFile = GetDataFile(@"Native\Assembly.SLDASM"))
            {
                a1 = Application.Documents.PreCreateFromPath(Path.Combine(dataFile.WorkFolderPath, @"Native\Assembly.SLDASM"));
                a1.State = DocumentState_e.ReadOnly;
                a1.Commit();
                r1 = a1.IsAlive;
                a1.Close();

                b1 = Application.Documents.PreCreateFromPath(Path.Combine(dataFile.WorkFolderPath, @"Native\Block.SLDBLK"));
                b1.State = DocumentState_e.ReadOnly;
                b1.Commit();
                r2 = b1.IsAlive;
                b1.Close();

                d1 = Application.Documents.PreCreateFromPath(Path.Combine(dataFile.WorkFolderPath, @"Native\Drawing.SLDDRW"));
                d1.State = DocumentState_e.ReadOnly;
                d1.Commit();
                r3 = d1.IsAlive;
                d1.Close();

                l1 = Application.Documents.PreCreateFromPath(Path.Combine(dataFile.WorkFolderPath, @"Native\LibFeatPart.SLDLFP"));
                l1.State = DocumentState_e.ReadOnly;
                l1.Commit();
                r4 = l1.IsAlive;
                l1.Close();

                p1 = Application.Documents.PreCreateFromPath(Path.Combine(dataFile.WorkFolderPath, @"Native\Part.SLDPRT"));
                p1.State = DocumentState_e.ReadOnly;
                p1.Commit();
                r5 = p1.IsAlive;
                p1.Close();

                at1 = Application.Documents.PreCreateFromPath(Path.Combine(dataFile.WorkFolderPath, @"Native\TemplateAssembly.ASMDOT"));
                at1.State = DocumentState_e.ReadOnly;
                at1.Commit();
                r6 = at1.IsAlive;
                at1.Close();

                dt1 = Application.Documents.PreCreateFromPath(Path.Combine(dataFile.WorkFolderPath, @"Native\TemplateDrawing.DRWDOT"));
                dt1.State = DocumentState_e.ReadOnly;
                dt1.Commit();
                r7 = dt1.IsAlive;
                dt1.Close();

                pt1 = Application.Documents.PreCreateFromPath(Path.Combine(dataFile.WorkFolderPath, @"Native\TemplatePart.PRTDOT"));
                pt1.State = DocumentState_e.ReadOnly;
                pt1.Commit();
                r8 = pt1.IsAlive;
                pt1.Close();
            }

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

            IXDocument a1_1;
            IXDocument b1_1;
            IXDocument d1_1;
            IXDocument l1_1;
            IXDocument p1_1;
            IXDocument at1_1;
            IXDocument dt1_1;
            IXDocument pt1_1;

            using (var dataFile = GetDataFile(@"Native\Assembly.SLDASM"))
            {
                var a1 = Application.Documents.PreCreate<IXDocument>();
                a1.Path = Path.Combine(dataFile.WorkFolderPath, @"Native\Assembly.SLDASM");
                a1.State = DocumentState_e.ReadOnly;
                a1.Commit();
                a1_1 = ((IXUnknownDocument)a1).GetSpecific();
                r1 = a1_1.IsAlive;
                a1.Close();

                var b1 = Application.Documents.PreCreate<IXDocument>();
                b1.Path = Path.Combine(dataFile.WorkFolderPath, @"Native\Block.SLDBLK");
                b1.State = DocumentState_e.ReadOnly;
                b1.Commit();
                b1_1 = ((IXUnknownDocument)b1).GetSpecific();
                r2 = b1_1.IsAlive;
                b1.Close();

                var d1 = Application.Documents.PreCreate<IXDocument>();
                d1.Path = Path.Combine(dataFile.WorkFolderPath, @"Native\Drawing.SLDDRW");
                d1.State = DocumentState_e.ReadOnly;
                d1.Commit();
                d1_1 = ((IXUnknownDocument)d1).GetSpecific();
                r3 = d1_1.IsAlive;
                d1.Close();

                var l1 = Application.Documents.PreCreate<IXDocument>();
                l1.Path = Path.Combine(dataFile.WorkFolderPath, @"Native\LibFeatPart.SLDLFP");
                l1.State = DocumentState_e.ReadOnly;
                l1.Commit();
                l1_1 = ((IXUnknownDocument)l1).GetSpecific();
                r4 = l1_1.IsAlive;
                l1.Close();

                var p1 = Application.Documents.PreCreate<IXDocument>();
                p1.Path = Path.Combine(dataFile.WorkFolderPath, @"Native\Part.SLDPRT");
                p1.State = DocumentState_e.ReadOnly;
                p1.Commit();
                p1_1 = ((IXUnknownDocument)p1).GetSpecific();
                r5 = p1_1.IsAlive;
                p1.Close();

                var at1 = Application.Documents.PreCreate<IXDocument>();
                at1.Path = Path.Combine(dataFile.WorkFolderPath, @"Native\TemplateAssembly.ASMDOT");
                at1.State = DocumentState_e.ReadOnly;
                at1.Commit();
                at1_1 = ((IXUnknownDocument)at1).GetSpecific();
                r6 = at1_1.IsAlive;
                at1.Close();

                var dt1 = Application.Documents.PreCreate<IXDocument>();
                dt1.Path = Path.Combine(dataFile.WorkFolderPath, @"Native\TemplateDrawing.DRWDOT");
                dt1.State = DocumentState_e.ReadOnly;
                dt1.Commit();
                dt1_1 = ((IXUnknownDocument)dt1).GetSpecific();
                r7 = dt1_1.IsAlive;
                dt1.Close();

                var pt1 = Application.Documents.PreCreate<IXDocument>();
                pt1.Path = Path.Combine(dataFile.WorkFolderPath, @"Native\TemplatePart.PRTDOT");
                pt1.State = DocumentState_e.ReadOnly;
                pt1.Commit();
                pt1_1 = ((IXUnknownDocument)pt1).GetSpecific();
                r8 = pt1_1.IsAlive;
                pt1.Close();
            }

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
                var part = (IXDocument3D)doc.Document;

                id1 = part.Id.Thumbprint;
            }

            using (var doc = OpenDataDocument(@"Id\IdPart1Copy.SLDPRT"))
            {
                var part = (IXDocument3D)doc.Document;

                id2 = part.Id.Thumbprint;
            }

            using (var doc = OpenDataDocument(@"Id\IdPart1CopyModified.SLDPRT"))
            {
                var part = (IXDocument3D)doc.Document;

                id3 = part.Id.Thumbprint;
            }

            using (var doc = OpenDataDocument(@"Id\IdPart1SaveAs.SLDPRT"))
            {
                var part = (IXDocument3D)doc.Document;

                id4 = part.Id.Thumbprint;
            }

            using (var doc = OpenDataDocument(@"Id\IdPart2.SLDPRT"))
            {
                var part = (IXDocument3D)doc.Document;

                id5 = part.Id.Thumbprint;
            }

            using (var doc = OpenDataDocument(@"Id\IdPart2Copy.SLDPRT"))
            {
                var part = (IXDocument3D)doc.Document;

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
