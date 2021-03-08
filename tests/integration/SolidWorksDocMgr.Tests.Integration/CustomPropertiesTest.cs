using NUnit.Framework;
using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.Exceptions;
using Xarial.XCad.SwDocumentManager.Documents;

namespace SolidWorksDocMgr.Tests.Integration
{
    public class CustomPropertiesTest : IntegrationTests
    {
        [Test]
        public void TestAddProperty()
        {
            bool exists;
            string val;

            bool existsConf;
            string valConf;

            using (var doc = OpenDataDocument("CustomProps1.SLDPRT"))
            {
                var prp = m_App.Documents.Active.Properties.GetOrPreCreate("AddTestPrp1");
                exists = prp.Exists();
                prp.Value = "AddTestPrp1Value";
                m_App.Documents.Active.Properties.Add(prp);

                val = m_App.Documents.Active.Document.GetCustomProperty("AddTestPrp1", out _);

                var prpConf = (m_App.Documents.Active as ISwDmDocument3D).Configurations["Default"].Properties.GetOrPreCreate("AddTestPrp1Conf");
                existsConf = prpConf.Exists();
                prpConf.Value = "AddTestPrp1ValueConf";
                (m_App.Documents.Active as ISwDmDocument3D).Configurations["Default"].Properties.Add(prpConf);

                valConf = (m_App.Documents.Active as ISwDmDocument3D).Configurations["Default"].Configuration.GetCustomProperty("AddTestPrp1Conf", out _);
            }

            Assert.IsFalse(exists);
            Assert.AreEqual("AddTestPrp1Value", val);
            Assert.IsFalse(existsConf);
            Assert.AreEqual("AddTestPrp1ValueConf", valConf);
        }

        [Test]
        public void TestReadAllProperties()
        {
            Dictionary<string, object> prps;
            Dictionary<string, object> prpsConf;

            using (var doc = OpenDataDocument("CustomProps1.SLDPRT"))
            {
                prps = m_App.Documents.Active.Properties.ToDictionary(p => p.Name, p => p.Value);
                prpsConf = (m_App.Documents.Active as ISwDmDocument3D).Configurations["Default"].Properties.ToDictionary(p => p.Name, p => p.Value);
            }

            Assert.That(prps.ContainsKey("Prop1"));
            Assert.That(prps.ContainsKey("Prop2"));
            Assert.AreEqual("Prop1Val", prps["Prop1"]);
            Assert.AreEqual("Prop2Val", prps["Prop2"]);

            Assert.That(prpsConf.ContainsKey("Prop1"));
            Assert.That(prpsConf.ContainsKey("Prop3"));
            Assert.AreEqual("Prop1ValConf", prpsConf["Prop1"]);
            Assert.AreEqual("Prop3ValConf", prpsConf["Prop3"]);
        }

        [Test]
        public void TestGetProperty()
        {
            object val;
            object valConf;

            bool r1;
            IXProperty prp1;

            using (var doc = OpenDataDocument("CustomProps1.SLDPRT"))
            {
                r1 = m_App.Documents.Active.Properties.TryGet("Prop1", out prp1);

                val = m_App.Documents.Active.Properties["Prop1"].Value;
                valConf = (m_App.Documents.Active as ISwDmDocument3D).Configurations["Default"].Properties["Prop1"].Value;
            }

            Assert.IsTrue(r1);
            Assert.IsNotNull(prp1);
            Assert.AreEqual("Prop1Val", val);
            Assert.AreEqual("Prop1ValConf", valConf);
        }

        [Test]
        public void TestGetMissingProperty()
        {
            using (var doc = OpenDataDocument("CustomProps1.SLDPRT"))
            {
                var r1 = m_App.Documents.Active.Properties.TryGet("Prop1_", out IXProperty prp1);

                Assert.IsFalse(r1);
                Assert.IsNull(prp1);

                Assert.Throws<EntityNotFoundException>(() => { var p = m_App.Documents.Active.Properties["Prop1_"]; });
                Assert.Throws<EntityNotFoundException>(() => { var p = (m_App.Documents.Active as ISwDmDocument3D).Configurations["Default"].Properties["Prop1_"]; });
            }
        }

        [Test]
        public void TestPropertyEvents()
        {
            string newVal = null;
            string newConfVal = null;

            using (var doc = OpenDataDocument("CustomProps1.SLDPRT"))
            {
                var part = (ISwDmDocument3D)m_App.Documents.Active;
                var p1 = part.Properties.GetOrPreCreate("P1");
                p1.Value = "A";
                p1.Commit();
                p1.ValueChanged += (IXProperty prp, object newValue) => { newVal += (string)newValue; };

                var p2 = part.Configurations.Active.Properties.GetOrPreCreate("P2");
                p2.ValueChanged += (IXProperty prp, object newValue) => { newConfVal += (string)newValue; };
                p2.Value = "B";
                p2.Commit();

                p1.Value = "A1";
                p2.Value = "B1";
            }

            Assert.AreEqual("A1", newVal);
            Assert.AreEqual("B1", newConfVal);
        }

        [Test]
        public void GetWeldmentCutListPropertiesTest()
        {
            Dictionary<string, object> conf1Prps;
            Dictionary<string, object> confDefPrps;

            using (var doc = OpenDataDocument("CutListConfs1.SLDPRT"))
            {
                var part = (ISwDmDocument3D)m_App.Documents.Active;

                conf1Prps = part.Configurations["Conf1<As Machined>"].CutLists
                    .First(c => c.Name == "Cut-List-Item1").Properties
                    .ToDictionary(p => p.Name, p => p.Value);

                confDefPrps = part.Configurations["Default<As Machined>"].CutLists
                    .First(c => c.Name == "Cut-List-Item1").Properties
                    .ToDictionary(p => p.Name, p => p.Value);
            }

            Assert.AreEqual(4, conf1Prps.Count);
            Assert.That(conf1Prps.ContainsKey("Prp1"));
            Assert.AreEqual("Conf1Val", conf1Prps["Prp1"]);
            Assert.AreEqual("Gen1Val", conf1Prps["Prp2"]);

            Assert.AreEqual(4, confDefPrps.Count);
            Assert.That(confDefPrps.ContainsKey("Prp1"));
            Assert.AreEqual("ConfDefVal", confDefPrps["Prp1"]);
            Assert.AreEqual("Gen1Val", confDefPrps["Prp2"]);
        }
        
        [Test]
        public void SetWeldmentCutListPropertiesTest()
        {
            var conf1Val = "";
            var confDefVal = "";

            using (var doc = OpenDataDocument("CutListConfs1.SLDPRT"))
            {
                var part = (ISwDmPart)m_App.Documents.Active;

                var prp1 = part.Configurations["Conf1<As Machined>"].CutLists
                    .First(c => c.Name == "Cut-List-Item1").Properties.GetOrPreCreate("Prp3");
                prp1.Value = "NewValueConf1";
                prp1.Commit();

                var prp2 = part.Configurations["Default<As Machined>"].CutLists
                    .First(c => c.Name == "Cut-List-Item1").Properties.GetOrPreCreate("Prp3");
                prp2.Value = "NewValueDefault";
                prp2.Commit();

                conf1Val = part.Configurations["Conf1<As Machined>"].CutLists
                    .First(c => c.Name == "Cut-List-Item1").CutListItem.GetCustomPropertyValue("Prp3", out _, out _);

                confDefVal = part.Configurations["Default<As Machined>"].CutLists
                    .First(c => c.Name == "Cut-List-Item1").CutListItem.GetCustomPropertyValue("Prp3", out _, out _);
            }

            Assert.AreEqual("NewValueConf1", conf1Val);
            Assert.AreEqual("NewValueDefault", confDefVal);
        }

        [Test]
        public void GetExpressionCustomPropertiesTest()
        {
            string exp1;
            object val1;
            string exp2;
            object val2;
            string exp3;
            object val3;

            string exp4;
            string exp5;
            string exp6;

            using (var doc = OpenDataDocument("CustomPropsExpression1.SLDPRT"))
            {
                var part = (IXPart)m_App.Documents.Active;

                exp1 = part.Properties["Material"].Expression;
                val1 = part.Properties["Material"].Value;
                exp4 = part.Properties["Prp1"].Expression;

                exp2 = part.Configurations.Active.Properties["Volume"].Expression;
                val2 = part.Configurations.Active.Properties["Volume"].Value;
                exp5 = part.Configurations.Active.Properties["Prp2"].Expression;

                exp3 = part.Configurations.Active.CutLists.First().Properties["QUANTITY"].Expression;
                val3 = part.Configurations.Active.CutLists.First().Properties["QUANTITY"].Value;
                exp6 = part.Configurations.Active.CutLists.First().Properties["Prp3"].Expression;
            }

            Assert.AreEqual("\"SW-Material@CustomPropsExpression1.SLDPRT\"", exp1);
            Assert.AreEqual("Brass", val1);
            Assert.AreEqual("ABC", exp4);

            Assert.AreEqual("\"SW-Volume@@Default<As Machined>@CustomPropsExpression1.SLDPRT\"", exp2);
            Assert.AreEqual("160597.86", val2);
            Assert.AreEqual("XYZ", exp5);

            Assert.AreEqual("\"QUANTITY@@@ C CHANNEL, 76.20 X 5<1>@CustomPropsExpression1.SLDPRT\"", exp3);
            Assert.AreEqual("1", val3);
            Assert.AreEqual("IJK", exp6);
        }
    }
}
