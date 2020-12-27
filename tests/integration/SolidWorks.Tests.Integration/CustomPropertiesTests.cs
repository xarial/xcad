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
                exists = prp.Exists();
                prp.Value = "AddTestPrp1Value";
                m_App.Documents.Active.Properties.Add(prp);
                m_App.Sw.IActiveDoc2.Extension.CustomPropertyManager[""].Get5("AddTestPrp1", false, out val, out _, out _);

                var prpConf = (m_App.Documents.Active as ISwDocument3D).Configurations["Default"].Properties.GetOrPreCreate("AddTestPrp1Conf");
                existsConf = prpConf.Exists();
                prpConf.Value = "AddTestPrp1ValueConf";
                (m_App.Documents.Active as ISwDocument3D).Configurations["Default"].Properties.Add(prpConf);
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
                prpsConf = (m_App.Documents.Active as ISwDocument3D).Configurations["Default"].Properties.ToDictionary(p => p.Name, p => p.Value);
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
                valConf = (m_App.Documents.Active as ISwDocument3D).Configurations["Default"].Properties["Prop1"].Value;
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
                Assert.Throws<CustomPropertyMissingException>(() => { var p = (m_App.Documents.Active as ISwDocument3D).Configurations["Default"].Properties["Prop1_"]; });
            }
        }

        [Test]
        public void TestPropertyEvents() 
        {
            string newVal = null;
            string newConfVal = null;

            using (var doc = NewDocument(Interop.swconst.swDocumentTypes_e.swDocPART)) 
            {
                var part = (ISwDocument3D)m_App.Documents.Active;
                var p1 = part.Properties.GetOrPreCreate("P1");
                p1.Value = "A";
                p1.Commit();
                p1.ValueChanged += (IXProperty prp, object newValue) => { newVal += (string)newValue; };

                var p2 = part.Configurations.Active.Properties.GetOrPreCreate("P2");
                p2.ValueChanged += (IXProperty prp, object newValue) => { newConfVal += (string)newValue; };
                p2.Value = "B";
                p2.Commit();

                part.Model.Extension.CustomPropertyManager[""].Set("P1", "A1");
                part.Model.ConfigurationManager.ActiveConfiguration.CustomPropertyManager.Set("P2", "B1");
            }

            Assert.AreEqual("A1", newVal);
            Assert.AreEqual("B1", newConfVal);
        }

        [Test]
        public void GetWeldmentCutListPropertiesTest() 
        {
            Dictionary<string, object> conf1Prps;
            Dictionary<string, object> defPrps;

            using (var doc = OpenDataDocument("CutListConfs1.SLDPRT")) 
            {
                var part = (ISwDocument3D)m_App.Documents.Active;

                conf1Prps = part.Configurations["Conf1"].CutLists
                    .First(c => c.Name == "Cut-List-Item1").Properties
                    .ToDictionary(p => p.Name, p => p.Value);

                defPrps = part.Configurations["Default"].CutLists
                    .First(c => c.Name == "Cut-List-Item1").Properties
                    .ToDictionary(p => p.Name, p => p.Value);
            }

            Assert.AreEqual(4, conf1Prps.Count);
            Assert.That(conf1Prps.ContainsKey("Prp1"));
            Assert.AreEqual("Conf1Val", conf1Prps["Prp1"]);
            Assert.AreEqual("Gen1Val", conf1Prps["Prp2"]);

            Assert.AreEqual(4, defPrps.Count);
            Assert.That(defPrps.ContainsKey("Prp1"));
            Assert.AreEqual("ConfDefVal", defPrps["Prp1"]);
            Assert.AreEqual("Gen1Val", defPrps["Prp2"]);
        }

        [Test]
        public void GetComponentWeldmentCutListPropertiesTest()
        {
            Dictionary<string, object> conf1Prps;
            Dictionary<string, object> defPrps;

            using (var doc = OpenDataDocument("AssmCutLists1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                conf1Prps = assm.Components["CutListConfs1-2"].ReferencedConfiguration.CutLists
                    .First(c => c.Name == "Cut-List-Item1").Properties
                    .ToDictionary(p => p.Name, p => p.Value);

                defPrps = assm.Components["CutListConfs1-1"].ReferencedConfiguration.CutLists
                    .First(c => c.Name == "Cut-List-Item1").Properties
                    .ToDictionary(p => p.Name, p => p.Value);
            }

            Assert.AreEqual(4, conf1Prps.Count);
            Assert.That(conf1Prps.ContainsKey("Prp1"));
            Assert.AreEqual("Conf1Val", conf1Prps["Prp1"]);
            Assert.AreEqual("Gen1Val", conf1Prps["Prp2"]);

            Assert.AreEqual(4, defPrps.Count);
            Assert.That(defPrps.ContainsKey("Prp1"));
            Assert.AreEqual("ConfDefVal", defPrps["Prp1"]);
            Assert.AreEqual("Gen1Val", defPrps["Prp2"]);
        }

        [Test]
        public void SetWeldmentCutListPropertiesTest()
        {
            var conf1Val = "";
            var confDefVal = "";

            using (var doc = OpenDataDocument("CutListConfs1.SLDPRT"))
            {
                var part = (ISwPart)m_App.Documents.Active;

                var prp1 = part.Configurations["Conf1<As Machined>"].CutLists
                    .First(c => c.Name == "Cut-List-Item1").Properties.GetOrPreCreate("Prp3");
                prp1.Value = "NewValueConf1";
                prp1.Commit();

                var prp2 = part.Configurations["Default<As Machined>"].CutLists
                    .First(c => c.Name == "Cut-List-Item1").Properties.GetOrPreCreate("Prp3");
                prp2.Value = "NewValueDefaultConf";

                part.Model.ShowConfiguration2("Conf1<As Machined>");
                part.Part.IFeatureByName("Cut-List-Item1").CustomPropertyManager.Get5("Prp3", false, out _, out conf1Val, out _);

                part.Model.ShowConfiguration2("Default<As Machined>");
                part.Part.IFeatureByName("Cut-List-Item1").CustomPropertyManager.Get5("Prp3", false, out _, out confDefVal, out _);
            }

            Assert.AreEqual("NewValueConf1", conf1Val);
            Assert.AreEqual("NewValueDefaultConf", confDefVal);
        }

        [Test]
        public void SetWeldmentCutListPropertiesCachedTest()
        {
            var conf1Val1 = "";
            var confDefVal1 = "";
            var confDefP3Val1 = "";
            var conf1P3Val1 = "";

            var conf1Val2 = "";
            var confDefVal2 = "";
            var confDefP3Val2 = "";
            var conf1P3Val2 = "";

            using (var doc = OpenDataDocument("CutListConfs1.SLDPRT"))
            {
                var part = (ISwPart)m_App.Documents.Active;

                var conf1CutLists = part.Configurations["Conf1<As Machined>"].CutLists;
                var defaultConfCutLists = part.Configurations["Default<As Machined>"].CutLists;

                var prp1 = conf1CutLists
                    .First(c => c.Name == "Cut-List-Item1").Properties.GetOrPreCreate("Prp1");
                prp1.Value = "NewValueConf1";
                //prp1.Commit();

                var prp2 = defaultConfCutLists
                    .First(c => c.Name == "Cut-List-Item1").Properties.GetOrPreCreate("Prp1");
                prp2.Value = "NewValueDefaultConf";

                var prp3 = conf1CutLists.First(c => c.Name == "Cut-List-Item1").Properties.GetOrPreCreate("Prp3");
                prp3.Value = "P3Conf1";
                prp3.Commit();

                var prp4 = defaultConfCutLists.First(c => c.Name == "Cut-List-Item1").Properties.GetOrPreCreate("Prp3");
                prp4.Value = "P3Def";
                prp4.Commit();
                //if (!prp2.IsCommitted) 
                //{
                //    prp2.Commit();
                //}

                part.Model.ShowConfiguration2("Conf1<As Machined>");
                part.Part.IFeatureByName("Cut-List-Item1").CustomPropertyManager.Get5("Prp1", true, out _, out conf1Val1, out _);
                part.Part.IFeatureByName("Cut-List-Item1").CustomPropertyManager.Get5("Prp3", true, out _, out conf1P3Val1, out _);

                part.Model.ShowConfiguration2("Default<As Machined>");
                part.Part.IFeatureByName("Cut-List-Item1").CustomPropertyManager.Get5("Prp1", true, out _, out confDefVal1, out _);
                part.Part.IFeatureByName("Cut-List-Item1").CustomPropertyManager.Get5("Prp3", true, out _, out confDefP3Val1, out _);

                prp1.Value = "NewValueConf1-1";
                prp2.Value = "NewValueDefaultConf-1";
                prp3.Value = "NewP3Conf1";
                prp4.Value = "NewP3Def";

                part.Model.ShowConfiguration2("Conf1<As Machined>");
                part.Part.IFeatureByName("Cut-List-Item1").CustomPropertyManager.Get5("Prp1", true, out _, out conf1Val2, out _);
                part.Part.IFeatureByName("Cut-List-Item1").CustomPropertyManager.Get5("Prp3", true, out _, out conf1P3Val2, out _);

                part.Model.ShowConfiguration2("Default<As Machined>");
                part.Part.IFeatureByName("Cut-List-Item1").CustomPropertyManager.Get5("Prp1", true, out _, out confDefVal2, out _);
                part.Part.IFeatureByName("Cut-List-Item1").CustomPropertyManager.Get5("Prp3", true, out _, out confDefP3Val2, out _);
            }

            Assert.AreEqual("NewValueConf1", conf1Val1);
            Assert.AreEqual("NewValueDefaultConf", confDefVal1);
            Assert.AreEqual("NewValueConf1-1", conf1Val2);
            Assert.AreEqual("NewValueDefaultConf-1", confDefVal2);
            Assert.AreEqual("P3Conf1", conf1P3Val1);
            Assert.AreEqual("P3Def", confDefP3Val1);
            Assert.AreEqual("NewP3Conf1", conf1P3Val2);
            Assert.AreEqual("NewP3Def", confDefP3Val1);
        }

        [Test]
        public void SetComponentWeldmentCutListPropertiesTest()
        {
            var conf1Val = "";
            var confDefVal = "";

            using (var doc = OpenDataDocument("AssmCutLists1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                var prp1 = assm.Components["CutListConfs1-2"].ReferencedConfiguration.CutLists
                    .First(c => c.Name == "Cut-List-Item1").Properties.GetOrPreCreate("Prp3");
                prp1.Value = "NewValueConf1";
                prp1.Commit();

                var prp2 = assm.Components["CutListConfs1-1"].ReferencedConfiguration.CutLists
                    .First(c => c.Name == "Cut-List-Item1").Properties.GetOrPreCreate("Prp3");
                prp2.Value = "NewValueDefaultConf";

                if (!prp2.IsCommitted)
                {
                    prp2.Commit();
                }

                var part = (ISwPart)assm.Components["CutListConfs1-1"].Document;

                part.Model.ShowConfiguration2("Conf1<As Machined>");
                part.Part.IFeatureByName("Cut-List-Item1").CustomPropertyManager.Get5("Prp3", false, out _, out conf1Val, out _);

                part.Model.ShowConfiguration2("Default<As Machined>");
                part.Part.IFeatureByName("Cut-List-Item1").CustomPropertyManager.Get5("Prp3", false, out _, out confDefVal, out _);
            }

            Assert.AreEqual("NewValueConf1", conf1Val);
            Assert.AreEqual("NewValueDefaultConf", confDefVal);
        }
    }
}