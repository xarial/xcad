using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents;
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
    }
}
