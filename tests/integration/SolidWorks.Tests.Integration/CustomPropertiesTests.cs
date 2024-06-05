using NUnit.Framework;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using Xarial.XCad.Base;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Data.Exceptions;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Documents.Exceptions;
using Xarial.XCad.SolidWorks.Enums;

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
        public void TestAddUnloadConfProperty()
        {
            object val1;
            string val2;

            using (var doc = OpenDataDocument("UnloadedConfPart.SLDPRT"))
            {
                var part = (ISwPart)m_App.Documents.Active;
                
                var prp1 = part.Configurations["Default"].Properties.PreCreate();
                prp1.Name = "Test1";
                prp1.Value = "Val1";
                try
                {
                    prp1.Commit();
                    m_App.Sw.IActiveDoc2.Extension.CustomPropertyManager["Default"].Get5("Test1", false, out string val1Str, out _, out _);
                    val1 = val1Str;
                }
                catch (CustomPropertyUnloadedConfigException ex)
                {
                    val1 = ex;
                }

                var prp2 = part.Configurations["Conf1"].Properties.PreCreate();
                prp2.Name = "Test2";
                prp2.Value = "Val2";
                prp2.Commit();
                m_App.Sw.IActiveDoc2.Extension.CustomPropertyManager["Conf1"].Get5("Test2", false, out val2, out _, out _);
            }

            if(m_App.Version.Major == SwVersion_e.Sw2021 && !m_App.IsVersionNewerOrEqual(SwVersion_e.Sw2021, 4, 1))
            {
                Assert.IsInstanceOf<CustomPropertyUnloadedConfigException>(val1);
            }
            else 
            {
                Assert.AreEqual("Val1", val1);
            }
            
            Assert.AreEqual("Val2", val2);
        }

        [Test]
        public void TestGetUnloadConfProperty()
        {
            object val1;
            object val2;
            
            using (var doc = OpenDataDocument("UnloadedConfPart.SLDPRT"))
            {
                var part = (ISwPart)m_App.Documents.Active;

                val1 = part.Configurations["Default"].Properties["Prp1"].Value;
                val2 = part.Configurations["Conf1"].Properties["Prp1"].Value;
            }

            Assert.AreEqual("DefaultVal1", val1);
            Assert.AreEqual("Conf1Val1", val2);
        }

        [Test]
        public void TestSetUnloadConfProperty()
        {
            string val1;
            string val2;

            using (var doc = OpenDataDocument("UnloadedConfPart.SLDPRT"))
            {
                var part = (ISwPart)m_App.Documents.Active;

                part.Configurations["Default"].Properties["Prp1"].Value = "_DefaultVal1_";
                part.Configurations["Conf1"].Properties["Prp1"].Value = "_Conf1Val1_";

                m_App.Sw.IActiveDoc2.Extension.CustomPropertyManager["Default"].Get5("Prp1", false, out val1, out _, out _);
                m_App.Sw.IActiveDoc2.Extension.CustomPropertyManager["Conf1"].Get5("Prp1", false, out val2, out _, out _);
            }

            Assert.AreEqual("_DefaultVal1_", val1);
            Assert.AreEqual("_Conf1Val1_", val2);
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
            Dictionary<string, object> confDefPrps;

            using (var doc = OpenDataDocument("CutListConfs1.SLDPRT")) 
            {
                var part = (ISwPart)m_App.Documents.Active;

                conf1Prps = ((IXPartConfiguration)part.Configurations.First(c => c.Name.StartsWith("Conf1"))).CutLists
                    ["Cut-List-Item1"].Properties
                    .ToDictionary(p => p.Name, p => p.Value);

                if (m_App.IsVersionNewerOrEqual(SwVersion_e.Sw2024))
                {
                    //need to refresh configurations to update cut-lists
                    var activeConf = part.Model.ConfigurationManager.ActiveConfiguration.Name;
                    part.Model.ShowConfiguration2("Default<As Machined>");
                    part.Model.ShowConfiguration2(activeConf);

                    confDefPrps = ((IXPartConfiguration)part.Configurations.First(c => c.Name.StartsWith("Default"))).CutLists
                        ["Cut-List-Item1"].Properties
                        .ToDictionary(p => p.Name, p => p.Value);
                }
                else
                {
                    Assert.Throws<ConfigurationSpecificCutListNotSupportedException>(
                        () => { var cl = ((IXPartConfiguration)part.Configurations.First(c => c.Name.StartsWith("Default"))).CutLists.ToArray(); });
                    
                    confDefPrps = null;
                }
            }

            Assert.AreEqual(4, conf1Prps.Count);
            Assert.That(conf1Prps.ContainsKey("Prp1"));
            Assert.AreEqual("Conf1Val", conf1Prps["Prp1"]);
            Assert.AreEqual("Gen1Val", conf1Prps["Prp2"]);

            if (m_App.IsVersionNewerOrEqual(SwVersion_e.Sw2024))
            {
                Assert.AreEqual(4, confDefPrps.Count);
                Assert.That(confDefPrps.ContainsKey("Prp1"));
                Assert.AreEqual("ConfDefVal", confDefPrps["Prp1"]);
                Assert.AreEqual("Gen1Val", confDefPrps["Prp2"]);
            }
        }

        [Test]
        public void GetComponentWeldmentCutListPropertiesTest()
        {
            Dictionary<string, object> conf1Prps;
            Dictionary<string, object> defPrps;

            using (var doc = OpenDataDocument("AssmCutLists1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                conf1Prps = ((ISwPartComponent)assm.Configurations.Active.Components["CutListConfs1-2"]).ReferencedConfiguration.CutLists
                    .First(c => c.Name == "Cut-List-Item1").Properties
                    .ToDictionary(p => p.Name, p => p.Value);

                defPrps = ((ISwPartComponent)assm.Configurations.Active.Components["CutListConfs1-1"]).ReferencedConfiguration.CutLists
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

                if (m_App.IsVersionNewerOrEqual(SwVersion_e.Sw2024))
                {
                    //need to refresh configurations to update cut-lists
                    var activeConf = part.Model.ConfigurationManager.ActiveConfiguration.Name;
                    part.Model.ShowConfiguration2("Default<As Machined>");
                    part.Model.ShowConfiguration2(activeConf);

                    var prp2 = part.Configurations["Default<As Machined>"].CutLists["Cut-List-Item1"].Properties.GetOrPreCreate("Prp3");
                    
                    Assert.Throws<ConfigurationSpecificCutListPropertiesWriteNotSupportedException>(() =>
                    {
                        prp2.Value = "NewValueDefault";
                        if (!prp2.IsCommitted)
                        {
                            prp2.Commit();
                        }
                    });
                }
                else
                {
                    Assert.Throws<ConfigurationSpecificCutListNotSupportedException>(
                        () => { var cl = part.Configurations["Default<As Machined>"].CutLists.ToArray(); });
                }

                part.Model.ShowConfiguration2("Conf1<As Machined>");
                part.Part.IFeatureByName("Cut-List-Item1").CustomPropertyManager.Get5("Prp3", false, out _, out conf1Val, out _);

                part.Model.ShowConfiguration2("Default<As Machined>");
                part.Part.IFeatureByName("Cut-List-Item1").CustomPropertyManager.Get5("Prp3", false, out _, out confDefVal, out _);
            }

            Assert.AreEqual("NewValueConf1", conf1Val);
            Assert.AreEqual("NewValueConf1", confDefVal);
        }
        
        [Test]
        public void SetComponentWeldmentCutListPropertiesTest()
        {
            var conf1Val = "";
            var confDefVal = "";

            using (var doc = OpenDataDocument("AssmCutLists1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                var prp1 = ((ISwPartComponent)assm.Configurations.Active.Components["CutListConfs1-2"]).ReferencedConfiguration.CutLists
                    .First(c => c.Name == "Cut-List-Item1").Properties.GetOrPreCreate("Prp3");
                prp1.Value = "NewValueConf1";
                prp1.Commit();

                var prp2 = ((ISwPartComponent)assm.Configurations.Active.Components["CutListConfs1-1"]).ReferencedConfiguration.CutLists
                    .First(c => c.Name == "Cut-List-Item1").Properties.GetOrPreCreate("Prp1");
                Assert.Throws<ConfigurationSpecificCutListPropertiesWriteNotSupportedException>(
                    () => { prp2.Value = "NewValue1Def1"; });

                var prp3 = ((ISwPartComponent)assm.Configurations.Active.Components["CutListConfs1-1"]).ReferencedConfiguration.CutLists
                    .First(c => c.Name == "Cut-List-Item1").Properties.GetOrPreCreate("Prp4");
                prp3.Value = "NewValue1Def1";
                Assert.Throws<ConfigurationSpecificCutListPropertiesWriteNotSupportedException>(
                    () => { prp3.Commit(); });

                var part = (ISwPart)assm.Configurations.Active.Components["CutListConfs1-1"].ReferencedDocument;

                part.Model.ShowConfiguration2("Conf1<As Machined>");
                part.Part.IFeatureByName("Cut-List-Item1").CustomPropertyManager.Get5("Prp3", false, out _, out conf1Val, out _);

                part.Model.ShowConfiguration2("Default<As Machined>");
                part.Part.IFeatureByName("Cut-List-Item1").CustomPropertyManager.Get5("Prp3", false, out _, out confDefVal, out _);
            }

            Assert.AreEqual("NewValueConf1", conf1Val);
            Assert.AreEqual("NewValueConf1", confDefVal);
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

        [Test]
        public void GetCustomPropertiesTypesTest()
        {
            object val1;
            object val2;
            object val3;
            object val4;
            object val5;
            object val6;

            using (var doc = OpenDataDocument("PrpTypes.SLDPRT"))
            {
                var part = (IXPart)m_App.Documents.Active;

                val1 = part.Properties["Text"].Value;
                val2 = part.Properties["Double"].Value;
                val3 = part.Properties["Integer"].Value;
                val4 = part.Properties["BoolTrue"].Value;
                val5 = part.Properties["BoolFalse"].Value;
                val6 = part.Properties["Date"].Value;
            }

            Assert.AreEqual("A", val1);
            Assert.IsInstanceOf<string>(val1);

            Assert.AreEqual(5.5, val2);
            Assert.IsInstanceOf<double>(val2);

            Assert.AreEqual(10, val3);
            Assert.IsInstanceOf<double>(val3);

            Assert.AreEqual(true, val4);
            Assert.IsInstanceOf<bool>(val4);

            Assert.AreEqual(false, val5);
            Assert.IsInstanceOf<bool>(val5);

            Assert.AreEqual(new DateTime(2023, 03, 28), val6);
            Assert.IsInstanceOf<DateTime>(val6);
        }

        //[Test]
        //public void SetCustomPropertiesTypesTest()
        //{
        //    using (var doc = NewDocument(swDocumentTypes_e.swDocPART))
        //    {
        //        var part = (IXPart)m_App.Documents.Active;

        //        var prp1 = part.Properties.GetOrPreCreate("Text");
        //        prp1.Value = "A";
        //        prp1.Commit();

        //        var prp2 = part.Properties.GetOrPreCreate("Double");
        //        prp2.Value = 5.5;
        //        prp2.Commit();

        //        var prp3 = part.Properties.GetOrPreCreate("Integer");
        //        prp3.Value = 10;
        //        prp3.Commit();

        //        var prp4 = part.Properties.GetOrPreCreate("BoolTrue");
        //        prp4.Value = true;
        //        prp4.Commit();

        //        var prp5 = part.Properties.GetOrPreCreate("BoolFalse");
        //        prp5.Value = false;
        //        prp5.Commit();

        //        var prp6 = part.Properties.GetOrPreCreate("Date");
        //        prp6.Value = new DateTime(2023, 03, 28);
        //        prp6.Commit();
        //    }
        //}

        [Test]
        public void SetExpressionCustomPropertiesTest()
        {
            string val;
            string resVal;

            using (var doc = OpenDataDocument("CustomPropsExpression1.SLDPRT"))
            {
                var part = (ISwPart)m_App.Documents.Active;

                var prp = part.Properties.PreCreate();

                prp.Name = "Test";
                prp.Expression = "\"D1@Sketch2\"";
                prp.Commit();

                part.Model.Extension.CustomPropertyManager[""].Get4("Test", false, out val, out resVal);
            }

            Assert.AreEqual("\"D1@Sketch2\"", val);
            Assert.AreEqual("25.00", resVal);
        }

        [Test]
        public void NotUpdatedConfPrpsCustomPropertiesTest()
        {
            object val1Def;
            object val2Def;

            object val1Conf1;
            object val2Conf1;

            object val1Def_1;
            object val2Def_1;

            object val1Conf1_1;
            object val2Conf1_1;

            using (var doc = OpenDataDocument("MultiConfNotUpdatePartPrps.SLDPRT"))
            {
                var part = (ISwPart)m_App.Documents.Active;

                var x = m_App.Sw.IActiveDoc2.Extension.CustomPropertyManager["Conf1"].Get6("Prp1", true, out string val, out string resVal, out _, out _);

                val1Def = part.Configurations["Default"].Properties["Prp1"].Value;
                val2Def = part.Configurations["Default"].Properties["Prp2"].Value;

                val1Conf1 = part.Configurations["Conf1"].Properties["Prp1"].Value;
                val2Conf1 = part.Configurations["Conf1"].Properties["Prp2"].Value;

                using (var assmDoc = NewDocument(swDocumentTypes_e.swDocASSEMBLY)) 
                {
                    var assm = (ISwAssembly)m_App.Documents.Active;
                    assm.Assembly.AddComponent5(part.Path, (int)swAddComponentConfigOptions_e.swAddComponentConfigOptions_CurrentSelectedConfig, "", true, "Conf1", 0, 0, 0);
                    assm.Assembly.AddComponent5(part.Path, (int)swAddComponentConfigOptions_e.swAddComponentConfigOptions_CurrentSelectedConfig, "", true, "Default", 0, 0, 0);
                    assm.Model.EditRebuild3();

                    val1Def_1 = part.Configurations["Default"].Properties["Prp1"].Value;
                    val2Def_1 = part.Configurations["Default"].Properties["Prp2"].Value;

                    val1Conf1_1 = part.Configurations["Conf1"].Properties["Prp1"].Value;
                    val2Conf1_1 = part.Configurations["Conf1"].Properties["Prp2"].Value;
                }
            }

            Assert.AreEqual("115.72", val1Def);
            Assert.AreEqual("200.00", val2Def);
            Assert.AreEqual("0.00", val1Conf1); //not resolved
            Assert.AreEqual("200.00", val2Conf1);

            Assert.AreEqual("115.72", val1Def_1);
            Assert.AreEqual("200.00", val2Def_1);
            Assert.AreEqual("57.86", val1Conf1_1); //not resolved
            Assert.AreEqual("100.00", val2Conf1_1);
        }
    }
}