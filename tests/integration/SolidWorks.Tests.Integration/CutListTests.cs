using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents;
using Xarial.XCad.Enums;
using Xarial.XCad.SolidWorks.Documents;

namespace SolidWorks.Tests.Integration
{
    public class CutListTests : IntegrationTests
    {
        [Test]
        public void SheetMetalCutListsTest()
        {
            Dictionary<string, Tuple<CutListType_e, string[]>> cutListData;

            using (var doc = OpenDataDocument("SheetMetal1.SLDPRT"))
            {
                var part = (ISwPart)m_App.Documents.Active;
                var cutLists = part.Configurations.Active.CutLists;
                cutListData = cutLists.ToDictionary(c => c.Name, c => new Tuple<CutListType_e, string[]>(c.Type, c.Bodies.Select(b => b.Name).ToArray()));
            }

            Assert.AreEqual(2, cutListData.Count);
            Assert.That(cutListData.ContainsKey("Sheet<1>"));
            Assert.AreEqual(1, cutListData["Sheet<1>"].Item2.Length);
            Assert.AreEqual(CutListType_e.SheetMetal, cutListData["Sheet<1>"].Item1);
            Assert.AreEqual("Edge-Flange1", cutListData["Sheet<1>"].Item2[0]);
            Assert.That(cutListData.ContainsKey("Sheet<2>"));
            Assert.AreEqual(CutListType_e.SheetMetal, cutListData["Sheet<2>"].Item1);
            Assert.AreEqual(1, cutListData["Sheet<2>"].Item2.Length);
            Assert.AreEqual("Hem1", cutListData["Sheet<2>"].Item2[0]);
        }

        [Test]
        public void CutListsTypes()
        {
            Dictionary<string, CutListType_e> cutListData;

            using (var doc = OpenDataDocument("CutListTypes_2021.SLDPRT"))
            {
                var part = (ISwPart)m_App.Documents.Active;
                var cutLists = part.Configurations.Active.CutLists;
                cutListData = cutLists.ToDictionary(c => c.Name, c => c.Type);
            }

            Assert.AreEqual(3, cutListData.Count);
            
            Assert.That(cutListData.ContainsKey("S 76.20 X 5.7<1>"));
            Assert.AreEqual(CutListType_e.Weldment, cutListData["S 76.20 X 5.7<1>"]);

            Assert.That(cutListData.ContainsKey("Sheet<1>"));
            Assert.AreEqual(CutListType_e.SheetMetal, cutListData["Sheet<1>"]);

            Assert.That(cutListData.ContainsKey("Cut-List-Item3"));
            Assert.AreEqual(CutListType_e.SolidBody, cutListData["Cut-List-Item3"]);
        }

        [Test]
        public void WeldmentCutListsTest()
        {
            Dictionary<string, string[]> cutListData;

            using (var doc = OpenDataDocument("Weldment1.SLDPRT"))
            {
                var part = (ISwPart)m_App.Documents.Active;
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
                var part = (ISwPart)m_App.Documents.Active;
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
            Dictionary<string, CutListStatus_e> cutListData;

            using (var doc = OpenDataDocument("CutListExcludeBom.SLDPRT"))
            {
                var part = (ISwPart)m_App.Documents.Active;
                var cutLists = part.Configurations.Active.CutLists;
                cutListData = cutLists.ToDictionary(c => c.Name, c => c.Status);
            }
            
            Assert.AreEqual((CutListStatus_e)0, cutListData["C CHANNEL 80.00 X 8<1>"]);
            Assert.AreEqual(CutListStatus_e.ExcludeFromBom, cutListData["PIPE, SCH 40, 25.40 DIA.<1>"]);
        }

        [Test]
        public void ComponentsCutListTest()
        {
            Dictionary<string, int> cutListData1;
            Dictionary<string, int> cutListData2;

            using (var doc = OpenDataDocument(@"CutListsAssembly1\Assem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;
                cutListData1 = ((ISwPartComponent)assm.Configurations.Active.Components["Part1-1"]).ReferencedConfiguration.CutLists.ToDictionary(c => c.Name, c => c.Bodies.Count());
                cutListData2 = ((ISwPartComponent)assm.Configurations.Active.Components["Part1-2"]).ReferencedConfiguration.CutLists.ToDictionary(c => c.Name, c => c.Bodies.Count());
            }

            Assert.AreEqual(1, cutListData1.Count);
            Assert.That(cutListData1.ContainsKey("L 25.40 X 25.40 X 3.175<1>"));
            Assert.AreEqual(1, cutListData1["L 25.40 X 25.40 X 3.175<1>"]);

            Assert.AreEqual(1, cutListData2.Count);
            Assert.That(cutListData2.ContainsKey("PIPE 21.30 X 2.3<1>"));
            Assert.AreEqual(1, cutListData2["PIPE 21.30 X 2.3<1>"]);
        }

        [Test]
        public void SubWeldmentsTest()
        {
            Dictionary<string, int> cutListData;

            using (var doc = OpenDataDocument(@"Weldment2.SLDPRT"))
            {
                var part = (ISwPart)m_App.Documents.Active;
                var cutLists = part.Configurations.Active.CutLists;
                cutListData = cutLists.ToDictionary(c => c.Name, c => c.Bodies.Count());
            }

            Assert.AreEqual(4, cutListData.Count);

            Assert.That(cutListData.ContainsKey("TUBE, RECTANGULAR 60.00 X 40.00 X 3.20<2>"));
            Assert.AreEqual(2, cutListData["TUBE, RECTANGULAR 60.00 X 40.00 X 3.20<2>"]);
            Assert.That(cutListData.ContainsKey("TUBE, RECTANGULAR 60.00 X 40.00 X 3.20<3>"));
            Assert.AreEqual(1, cutListData["TUBE, RECTANGULAR 60.00 X 40.00 X 3.20<3>"]);
            Assert.That(cutListData.ContainsKey("Cut-List-Item5"));
            Assert.AreEqual(1, cutListData["Cut-List-Item5"]);
            Assert.That(cutListData.ContainsKey("Cut-List-Item6"));
            Assert.AreEqual(1, cutListData["Cut-List-Item6"]);
        }
    }
}
