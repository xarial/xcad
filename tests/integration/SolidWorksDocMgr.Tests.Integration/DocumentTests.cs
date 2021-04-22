using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Xarial.XCad.Data.Enums;
using Xarial.XCad.Documents;
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
        //[Test]
        //public void DocumentsTest() 
        //{
        //    var c1 = m_App.Documents.Count;
        //    var activeIsNull = m_App.Documents.Active == null;

        //    var doc1 = m_App.Documents.Open(GetFilePath("Part_2020.sldprt"), Xarial.XCad.Documents.Enums.DocumentState_e.ReadOnly);

        //    var c2 = m_App.Documents.Count;
        //    var activeIsDoc1 = m_App.Documents.Active == doc1;

        //    var doc2 = m_App.Documents.Open(GetFilePath("Part_2019.sldprt"), Xarial.XCad.Documents.Enums.DocumentState_e.ReadOnly);

        //    var c3 = m_App.Documents.Count;
        //    var activeIsDoc2 = m_App.Documents.Active == doc2;

        //    doc1.Close();
        //    var c4 = m_App.Documents.Count;
        //    var activeIsDoc21 = m_App.Documents.Active == doc2;

        //    doc2.Close();
        //    var c5 = m_App.Documents.Count;
        //    var activeIsNull1 = m_App.Documents.Active == null;

        //    Assert.AreEqual(0, c1);
        //    Assert.IsTrue(activeIsNull);
        //    Assert.AreEqual(1, c2);
        //    Assert.IsTrue(activeIsDoc1);
        //    Assert.AreEqual(2, c3);
        //    Assert.IsTrue(activeIsDoc2);
        //    Assert.AreEqual(1, c4);
        //    Assert.IsTrue(activeIsDoc21);
        //    Assert.AreEqual(0, c5);
        //    Assert.IsTrue(activeIsNull1);
        //}

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

                var deps = assm.Dependencies;

                var dir = Path.GetDirectoryName(assm.Path);

                Assert.AreEqual(4, deps.Length);
                Assert.That(deps.All(d => !d.IsCommitted));
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
                Assert.That(deps.All(d => !d.IsCommitted));
                Assert.That(deps.Any(d => string.Equals(d.Path, Path.Combine(dir, "Part2.SLDPRT"))));
                Assert.That(deps.Any(d => string.Equals(d.Path, Path.Combine(dir, "Part3.SLDPRT"))));
                Assert.That(deps.Any(d => string.Equals(d.Path, Path.Combine(dir, "Part4-1 (XYZ).SLDPRT"))));
                Assert.That(deps.Any(d => string.Equals(d.Path, Path.Combine(dir, "Assem1.SLDASM"))));
                Assert.That(deps.Any(d => string.Equals(d.Path, Path.Combine(dir, "Assem2.SLDASM"))));
                Assert.That(deps.Any(d => string.Equals(d.Path, Path.Combine(dir, "Part1.SLDPRT"))));
            }
        }

        [Test]
        public void DocumentDependenciesCachedTest()
        {
            using (var doc = OpenDataDocument(@"MovedNonOpenedAssembly1\TopAssembly.SLDASM"))
            {
                var assm = m_App.Documents.Active;

                var deps = assm.Dependencies;

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

                var deps = assm.Dependencies;

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
    }
}
