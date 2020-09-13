using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Xarial.XCad.Base;
using Xarial.XCad.Data;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Data.Exceptions;
using Xarial.XCad.SolidWorks.Documents;

namespace SolidWorks.Tests.Integration
{
    public class CustomPropertiesTests : IntegrationTests
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
                exists = prp.Exists;
                prp.Value = "AddTestPrp1Value";
                m_App.Documents.Active.Properties.Add(prp);
                m_App.Sw.IActiveDoc2.Extension.CustomPropertyManager[""].Get5("AddTestPrp1", false, out val, out _, out _);

                var prpConf = (m_App.Documents.Active as SwDocument3D).Configurations["Default"].Properties.GetOrPreCreate("AddTestPrp1Conf");
                existsConf = prpConf.Exists;
                prpConf.Value = "AddTestPrp1ValueConf";
                (m_App.Documents.Active as SwDocument3D).Configurations["Default"].Properties.Add(prpConf);
                m_App.Sw.IActiveDoc2.Extension.CustomPropertyManager["Default"].Get5("AddTestPrp1Conf", false, out valConf, out _, out _);
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
                prpsConf = (m_App.Documents.Active as SwDocument3D).Configurations["Default"].Properties.ToDictionary(p => p.Name, p => p.Value);
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
                valConf = (m_App.Documents.Active as SwDocument3D).Configurations["Default"].Properties["Prop1"].Value;
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

                Assert.Throws<CustomPropertyMissingException>(()=> { var p = m_App.Documents.Active.Properties["Prop1_"]; });
                Assert.Throws<CustomPropertyMissingException>(() => { var p = (m_App.Documents.Active as SwDocument3D).Configurations["Default"].Properties["Prop1_"]; });
            }
        }
    }
}