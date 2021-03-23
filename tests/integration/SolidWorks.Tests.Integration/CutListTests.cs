using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Enums;
using Xarial.XCad.SolidWorks.Documents;

namespace SolidWorks.Tests.Integration
{
    public class CutListTests : IntegrationTests
    {
        [Test]
        public void SheetMetalCutListsTest()
        {
            Dictionary<string, string[]> cutListData;

            using (var doc = OpenDataDocument("SheetMetal1.SLDPRT"))
            {
                var part = (ISwDocument3D)m_App.Documents.Active;
                var cutLists = part.Configurations.Active.CutLists;
                cutListData = cutLists.ToDictionary(c => c.Name, c => c.Bodies.Select(b => b.Name).ToArray());
            }

            Assert.AreEqual(2, cutListData.Count);
            Assert.That(cutListData.ContainsKey("Sheet<1>"));
            Assert.AreEqual(1, cutListData["Sheet<1>"].Length);
            Assert.AreEqual("Edge-Flange1", cutListData["Sheet<1>"][0]);
            Assert.That(cutListData.ContainsKey("Sheet<2>"));
            Assert.AreEqual(1, cutListData["Sheet<2>"].Length);
            Assert.AreEqual("Hem1", cutListData["Sheet<2>"][0]);
        }

        [Test]
        public void WeldmentCutListsTest()
        {
            Dictionary<string, string[]> cutListData;

            using (var doc = OpenDataDocument("Weldment1.SLDPRT"))
            {
                var part = (ISwDocument3D)m_App.Documents.Active;
                var cutLists = part.Configurations.Active.CutLists;
                cutListData = cutLists.ToDictionary(c => c.Name, c => c.Bodies.Select(b => b.Name).ToArray());
            }

            Assert.AreEqual(1, cutListData.Count);
            Assert.That(cutListData.ContainsKey(" C CHANNEL, 76.20 X 5<1>"));
            Assert.AreEqual(3, cutListData[" C CHANNEL, 76.20 X 5<1>"].Length);
            Assert.That(cutListData[" C CHANNEL, 76.20 X 5<1>"].Contains("C channel - configured 3 X 5(1)"));
            Assert.That(cutListData[" C CHANNEL, 76.20 X 5<1>"].Contains("CirPattern1[1]"));
            Assert.That(cutListData[" C CHANNEL, 76.20 X 5<1>"].Contains("CirPattern1[2]"));
        }

        [Test]
        public void OutdatedCutListsTest()
        {
            Dictionary<string, string[]> cutListData;

            using (var doc = OpenDataDocument("CutListsOutdated.SLDPRT"))
            {
                var part = (ISwDocument3D)m_App.Documents.Active;
                var cutLists = part.Configurations.Active.CutLists;
                cutListData = cutLists.ToDictionary(c => c.Name, c => c.Bodies.Select(b => b.Name).ToArray());
            }

            Assert.AreEqual(3, cutListData.Count);
            Assert.That(cutListData.ContainsKey(" C CHANNEL, 76.20 X 5<1>"));
            Assert.That(cutListData.ContainsKey(" C CHANNEL, 76.20 X 5<2>"));
            Assert.That(cutListData.ContainsKey(" C CHANNEL, 76.20 X 5<3>"));
            Assert.AreEqual(1, cutListData[" C CHANNEL, 76.20 X 5<1>"].Length);
            Assert.That(cutListData[" C CHANNEL, 76.20 X 5<1>"].Contains("C channel - configured 3 X 5(1)[1]"));
            Assert.AreEqual(1, cutListData[" C CHANNEL, 76.20 X 5<2>"].Length);
            Assert.That(cutListData[" C CHANNEL, 76.20 X 5<2>"].Contains("C channel - configured 3 X 5(1)[2]"));
            Assert.AreEqual(1, cutListData[" C CHANNEL, 76.20 X 5<3>"].Length);
            Assert.That(cutListData[" C CHANNEL, 76.20 X 5<3>"].Contains("C channel - configured 3 X 5(1)[3]"));
        }

        [Test]
        public void ExcludeFromBomTest()
        {
            Dictionary<string, CutListState_e> cutListData;

            using (var doc = OpenDataDocument("CutListExcludeBom.SLDPRT"))
            {
                var part = (ISwDocument3D)m_App.Documents.Active;
                var cutLists = part.Configurations.Active.CutLists;
                cutListData = cutLists.ToDictionary(c => c.Name, c => c.State);
            }
            
            Assert.AreEqual((CutListState_e)0, cutListData["C CHANNEL 80.00 X 8<1>"]);
            Assert.AreEqual(CutListState_e.ExcludeFromBom, cutListData["PIPE, SCH 40, 25.40 DIA.<1>"]);
        }
    }
}
