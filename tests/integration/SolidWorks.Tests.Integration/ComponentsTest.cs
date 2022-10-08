using NUnit.Framework;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;

namespace SolidWorks.Tests.Integration
{
    public class ComponentsTest : IntegrationTests
    {
        [Test]
        public void IterateRootComponentsTest() 
        {
            string[] compNames;

            using (var doc = OpenDataDocument(@"Assembly1\TopAssem1.SLDASM"))
            {
                compNames = ((ISwAssembly)m_App.Documents.Active).Configurations.Active.Components.Select(c => c.Name).ToArray();
            }

            CollectionAssert.AreEquivalent(new string[] { "Part1-1", "Part1-2", "SubAssem1-1", "SubAssem1-2", "SubAssem2-1", "Part1-3" }, compNames);
        }

        [Test]
        public void IterateSubComponentsTest()
        {
            string[] compNames;

            using (var doc = OpenDataDocument(@"Assembly1\TopAssem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;
                var comp = assm.Configurations.Active.Components["SubAssem1-1"];
                compNames = comp.Children.Select(c => c.FullName).ToArray();
            }

            CollectionAssert.AreEquivalent(new string[] { "SubAssem1-1/Part2-1", "SubAssem1-1/SubSubAssem1-1" }, compNames);
        }

        [Test]
        public void IterateComponentsOrderTest()
        {
            string[] rootCompNames;
            string[] subCompNames;

            using (var doc = OpenDataDocument(@"Assembly13\Assem13.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;
                rootCompNames = ((ISwAssembly)m_App.Documents.Active).Configurations.Active.Components.Select(c => c.Name).ToArray();
                subCompNames = assm.Configurations.Active.Components["SubAssem1-1"].Children.Select(c => c.FullName).ToArray();
            }

            CollectionAssert.AreEqual(new string[] { "Part1-2", "Part1-3", "Part5-1", "Part2-2", "SubAssem1-1", "Part4-1", "Part2-3", "Part3-1" }, rootCompNames);
            CollectionAssert.AreEqual(new string[] { "SubAssem1-1/Part5-1", "SubAssem1-1/Part6-1", "SubAssem1-1/Part8-1", "SubAssem1-1/Part6-2" }, subCompNames);
        }

        [Test]
        public void IterateComponentsPatternsTest()
        {
            string[] rootCompNames;
            string[] subCompNames;

            using (var doc = OpenDataDocument(@"Assembly14\Assem14.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;
                rootCompNames = ((ISwAssembly)m_App.Documents.Active).Configurations.Active.Components.Select(c => c.Name).ToArray();
                subCompNames = assm.Configurations.Active.Components["SubAssem1-1"].Children.Select(c => c.FullName).ToArray();
            }

            CollectionAssert.AreEqual(new string[] { "Part1-1", "Part1-3", "Part1-4", "Part1-5", "Part1-6", "Part1-7", "Part1-8", "Part1-9", "Part1-10", "Part1-11", "Part1-12", "Part1-13", "SubAssem1-1" }, rootCompNames);
            CollectionAssert.AreEqual(new string[] { "SubAssem1-1/Part2-1", "SubAssem1-1/Part2-2", "SubAssem1-1/Part2-3", "SubAssem1-1/Part2-4" }, subCompNames);
        }

        [Test]
        public void GetDocumentTest() 
        {
            bool doc1Contains;
            bool doc2Contains;
            string doc1FileName;
            string doc2FileName;

            using (var doc = OpenDataDocument(@"Assembly1\TopAssem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                var doc1 = assm.Configurations.Active.Components["Part1-1"].ReferencedDocument;
                doc1FileName = Path.GetFileName(doc1.Path);
                doc1Contains = m_App.Documents.Contains(doc1);

                var doc2 = assm.Configurations.Active.Components["SubAssem1-1"].ReferencedDocument;
                doc2FileName = Path.GetFileName(doc2.Path);
                doc2Contains = m_App.Documents.Contains(doc2);

                var d = assm.Configurations.Active.Components["Part1-2"].ReferencedDocument;

                Assert.IsTrue(doc1.IsCommitted);
                Assert.IsTrue(doc2.IsCommitted);
                Assert.IsTrue(d.IsCommitted);
                Assert.That(string.Equals(Path.Combine(Path.GetDirectoryName(assm.Path), "Part1.sldprt"), 
                    d.Path, StringComparison.CurrentCultureIgnoreCase));
            }

            Assert.That(doc1FileName.Equals("Part1.sldprt", StringComparison.CurrentCultureIgnoreCase));
            Assert.That(doc2FileName.Equals("SubAssem1.sldasm", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(doc1Contains);
            Assert.IsTrue(doc2Contains);
        }

        [Test]
        public void GetDocumentUncommittedTest() 
        {
            using (var doc = OpenDataDocument(@"Assembly2\TopAssem.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                var doc1 = assm.Configurations.Active.Components["Part1-1"].ReferencedDocument;
                var doc2 = assm.Configurations.Active.Components["Assem2-1"].ReferencedDocument;
                var doc3 = assm.Configurations.Active.Components["Assem2-1"].Children["Part2-1"].ReferencedDocument;
                var doc4 = assm.Configurations.Active.Components["Assem2-1"].Children["Part3-1"].ReferencedDocument;
                var doc5 = assm.Configurations.Active.Components["Assem1-1"].ReferencedDocument;

                Assert.IsTrue(doc1.IsCommitted);
                Assert.IsTrue(doc2.IsCommitted);
                Assert.IsFalse(doc3.IsCommitted);
                Assert.IsTrue(doc4.IsCommitted);
                Assert.IsFalse(doc5.IsCommitted);

                Assert.That(doc3 is ISwPart);
                Assert.That(string.Equals(Path.Combine(Path.GetDirectoryName(assm.Path), "Part2.sldprt"),
                    doc3.Path, StringComparison.CurrentCultureIgnoreCase));

                Assert.That(doc5 is ISwAssembly);
                Assert.That(string.Equals(Path.Combine(Path.GetDirectoryName(assm.Path), "Assem1.sldasm"),
                    doc5.Path, StringComparison.CurrentCultureIgnoreCase));
            }
        }

        [Test]
        public void GetComponentRefConfigTest()
        {
            using (var doc = OpenDataDocument(@"Assembly2\TopAssem.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                var conf1 = assm.Configurations.Active.Components["Part1-1"].ReferencedConfiguration;
                var conf2 = assm.Configurations.Active.Components["Assem2-1"].ReferencedConfiguration;
                var conf3 = assm.Configurations.Active.Components["Assem2-1"].Children["Part2-1"].ReferencedConfiguration;
                var conf4 = assm.Configurations.Active.Components["Part4-1 (XYZ)-2"].ReferencedConfiguration;
                var conf5 = assm.Configurations.Active.Components["Assem1-1"].ReferencedConfiguration;

                Assert.IsTrue(conf1.IsCommitted);
                Assert.IsTrue(conf2.IsCommitted);
                Assert.IsFalse(conf3.IsCommitted);
                Assert.IsTrue(conf4.IsCommitted);
                Assert.IsFalse(conf5.IsCommitted);

                Assert.AreEqual("Default", conf1.Name);
                Assert.AreEqual("Default", conf2.Name);
                Assert.AreEqual("Default", conf3.Name);
                Assert.AreEqual("1-1", conf4.Name);
                Assert.AreEqual("Default", conf5.Name);
            }
        }

        [Test]
        public void GetDocumentLdrTest()
        {
            string doc1FileName;
            bool doc1IsCommitted;

            using (var doc = OpenDataDocument(@"Assembly1\TopAssem1.SLDASM", false, s => s.ViewOnly = true))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                assm.Model.Extension.SelectByID2("Part1-1@TopAssem1", "COMPONENT", 0, 0, 0, false, 0, null, 0);

                var swComp = assm.Model.ISelectionManager.GetSelectedObject6(1, -1) as IComponent2;

                var comp = assm.CreateObjectFromDispatch<ISwComponent>(swComp);

                var doc1 = comp.ReferencedDocument;
                doc1FileName = Path.GetFileName(doc1.Path);
                doc1IsCommitted = doc1.IsCommitted;
            }

            Assert.That(doc1FileName.Equals("Part1.sldprt", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsFalse(doc1IsCommitted);
        }

        [Test]
        public void IterateFeaturesTest()
        {
            var featNames = new List<string>();

            using (var doc = OpenDataDocument(@"Assembly1\TopAssem1.SLDASM"))
            {
                var comp = ((ISwAssembly)m_App.Documents.Active).Configurations.Active.Components["Part1-1"];

                foreach (var feat in comp.Features)
                {
                    featNames.Add(feat.Name);
                }
            }

            var expected = new string[] { "Comments", "Favorites", "History", "Selection Sets", "Sensors", "Design Binder", "Annotations",
                "Notes", "Notes1___EndTag___", "Surface Bodies", "Solid Bodies", "Lights, Cameras and Scene", "Ambient",
                "Directional1", "Directional2", "Directional3", "Markups", "Equations", "Material <not specified>", "Front Plane",
                "Top Plane", "Right Plane", "Origin", "Sketch1", "Boss-Extrude1" };

            CollectionAssert.AreEqual(expected, featNames);
        }

        [Test]
        public void VirtualComponentsTest() 
        {
            string[] compNames;
            bool[] isCommitted;
            bool[] isAlive;
            bool[] isVirtual;

            using (var doc = OpenDataDocument(@"VirtAssem1.SLDASM"))
            {
                var comps = ((ISwAssembly)m_App.Documents.Active).Configurations.Active.Components;
                compNames = comps.Select(c => c.Name).ToArray();
                var docs = comps.Select(c => c.ReferencedDocument).ToArray();
                foreach (var compDoc in docs) 
                {
                    if (!compDoc.IsCommitted) 
                    {
                        compDoc.Commit();
                    }
                }

                isCommitted = docs.Select(d => d.IsCommitted).ToArray();
                isAlive = docs.Select(d => d.IsAlive).ToArray();
                isVirtual = comps.Select(c => c.State.HasFlag(ComponentState_e.Embedded)).ToArray();

                //SOLIDWORKS 2022 is crashing if assembly of the virtual component is closed while virtual component is opened in its own window
                foreach (var compDoc in docs)
                {
                    if (compDoc.IsCommitted)
                    {
                        compDoc.Close();
                    }
                }
            }

            Assert.That(compNames.OrderBy(c => c).SequenceEqual(
                new string[] { "Part1^VirtAssem1-1", "Assem2^VirtAssem1-1" }.OrderBy(c => c)));
            Assert.That(isCommitted.All(x => x == true));
            Assert.That(isAlive.All(x => x == true));
            Assert.That(isVirtual.All(x => x == true));
        }

        [Test]
        public void ComponentCountTest()
        {
            int count;
            int totalCount;

            using (var doc = OpenDataDocument(@"Assembly1\TopAssem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;
                count = assm.Configurations.Active.Components.Count;
                totalCount = assm.Configurations.Active.Components.TotalCount;
            }

            Assert.AreEqual(6, count);
            Assert.AreEqual(17, totalCount);
        }

        [Test]
        public void ComponentsMultiConfigsTest()
        {
            string c1_def;
            bool s1_def;
            string c2_def;
            bool s2_def;
            string c3_def;
            bool s3_def;
            string c4_def;
            bool s4_def;
            string c5_def;
            bool s5_def;
            string c6_def;
            ComponentState_e s6_def;
            string c7_def;
            ComponentState_e s7_def;
            string c8_def;
            ComponentState_e s8_def;
            string c9_def;
            ComponentState_e s9_def;
            string c10_def;
            ComponentState_e s10_def;

            string c1_conf1;
            bool s1_conf1;
            string c2_conf1;
            bool s2_conf1;
            string c3_conf1;
            bool s3_conf1;
            string c4_conf1;
            bool s4_conf1;
            string c5_conf1;
            bool s5_conf1;
            string c6_conf1;
            ComponentState_e s6_conf1;
            string c7_conf1;
            ComponentState_e s7_conf1;
            string c8_conf1;
            ComponentState_e s8_conf1;
            string c9_conf1;
            ComponentState_e s9_conf1;
            string c10_conf1;
            ComponentState_e s10_conf1;

            using (var doc = OpenDataDocument(@"Assembly4\Assembly1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;
                c1_def = assm.Configurations["Default"].Components["SubAssem1-1"].ReferencedConfiguration.Name;
                s1_def = assm.Configurations["Default"].Components["SubAssem1-1"].State.HasFlag(ComponentState_e.Suppressed);
                c2_def = assm.Configurations["Default"].Components["Part1-1"].ReferencedConfiguration.Name;
                s2_def = assm.Configurations["Default"].Components["Part1-1"].State.HasFlag(ComponentState_e.Suppressed);
                c3_def = assm.Configurations["Default"].Components["SubAssem1-1"].Children["Part1-1"].ReferencedConfiguration.Name;
                s3_def = assm.Configurations["Default"].Components["SubAssem1-1"].Children["Part1-1"].State.HasFlag(ComponentState_e.Suppressed);
                c4_def = assm.Configurations["Default"].Components["SubAssem1-1"].Children["Part1-2"].ReferencedConfiguration.Name;
                s4_def = assm.Configurations["Default"].Components["SubAssem1-1"].Children["Part1-2"].State.HasFlag(ComponentState_e.Suppressed);
                c5_def = assm.Configurations["Default"].Components["SubAssem1-1"].Children["SubSubAssem1-1"].ReferencedConfiguration.Name;
                s5_def = assm.Configurations["Default"].Components["SubAssem1-1"].Children["SubSubAssem1-1"].State.HasFlag(ComponentState_e.Suppressed);
                c6_def = assm.Configurations["Default"].Components["SubAssem1-1"].Children["SubSubAssem1-1"].Children["Part2-1"].ReferencedConfiguration.Name;
                s6_def = assm.Configurations["Default"].Components["SubAssem1-1"].Children["SubSubAssem1-1"].Children["Part2-1"].State;
                c7_def = assm.Configurations["Default"].Components["SubAssem1-1"].Children["SubSubAssem1-1"].Children["Part2-2"].ReferencedConfiguration.Name;
                s7_def = assm.Configurations["Default"].Components["SubAssem1-1"].Children["SubSubAssem1-1"].Children["Part2-2"].State;
                c8_def = assm.Configurations["Default"].Components["SubAssem1-1"].Children["SubSubAssem1-1"].Children["Part2-3"].ReferencedConfiguration.Name;
                s8_def = assm.Configurations["Default"].Components["SubAssem1-1"].Children["SubSubAssem1-1"].Children["Part2-3"].State;
                c9_def = assm.Configurations["Default"].Components["SubAssem1-1"].Children["SubSubAssem1-1"].Children["Part2-4"].ReferencedConfiguration.Name;
                s9_def = assm.Configurations["Default"].Components["SubAssem1-1"].Children["SubSubAssem1-1"].Children["Part2-4"].State;
                c10_def = assm.Configurations["Default"].Components["SubAssem1-1"].Children["SubSubAssem1-1"].Children["Part2-5"].ReferencedConfiguration.Name;
                s10_def = assm.Configurations["Default"].Components["SubAssem1-1"].Children["SubSubAssem1-1"].Children["Part2-5"].State;

                c1_conf1 = assm.Configurations["Conf1"].Components["SubAssem1-1"].ReferencedConfiguration.Name;
                s1_conf1 = assm.Configurations["Conf1"].Components["SubAssem1-1"].State.HasFlag(ComponentState_e.Suppressed);
                c2_conf1 = assm.Configurations["Conf1"].Components["Part1-1"].ReferencedConfiguration.Name;
                s2_conf1 = assm.Configurations["Conf1"].Components["Part1-1"].State.HasFlag(ComponentState_e.Suppressed);
                c3_conf1 = assm.Configurations["Conf1"].Components["SubAssem1-1"].Children["Part1-1"].ReferencedConfiguration.Name;
                s3_conf1 = assm.Configurations["Conf1"].Components["SubAssem1-1"].Children["Part1-1"].State.HasFlag(ComponentState_e.Suppressed);
                c4_conf1 = assm.Configurations["Conf1"].Components["SubAssem1-1"].Children["Part1-2"].ReferencedConfiguration.Name;
                s4_conf1 = assm.Configurations["Conf1"].Components["SubAssem1-1"].Children["Part1-2"].State.HasFlag(ComponentState_e.Suppressed);
                c5_conf1 = assm.Configurations["Conf1"].Components["SubAssem1-1"].Children["SubSubAssem1-1"].ReferencedConfiguration.Name;
                s5_conf1 = assm.Configurations["Conf1"].Components["SubAssem1-1"].Children["SubSubAssem1-1"].State.HasFlag(ComponentState_e.Suppressed);
                c6_conf1 = assm.Configurations["Conf1"].Components["SubAssem1-1"].Children["SubSubAssem1-1"].Children["Part2-1"].ReferencedConfiguration.Name;
                s6_conf1 = assm.Configurations["Conf1"].Components["SubAssem1-1"].Children["SubSubAssem1-1"].Children["Part2-1"].State;
                c7_conf1 = assm.Configurations["Conf1"].Components["SubAssem1-1"].Children["SubSubAssem1-1"].Children["Part2-2"].ReferencedConfiguration.Name;
                s7_conf1 = assm.Configurations["Conf1"].Components["SubAssem1-1"].Children["SubSubAssem1-1"].Children["Part2-2"].State;
                c8_conf1 = assm.Configurations["Conf1"].Components["SubAssem1-1"].Children["SubSubAssem1-1"].Children["Part2-3"].ReferencedConfiguration.Name;
                s8_conf1 = assm.Configurations["Conf1"].Components["SubAssem1-1"].Children["SubSubAssem1-1"].Children["Part2-3"].State;
                c9_conf1 = assm.Configurations["Conf1"].Components["SubAssem1-1"].Children["SubSubAssem1-1"].Children["Part2-4"].ReferencedConfiguration.Name;
                s9_conf1 = assm.Configurations["Conf1"].Components["SubAssem1-1"].Children["SubSubAssem1-1"].Children["Part2-4"].State;
                c10_conf1 = assm.Configurations["Conf1"].Components["SubAssem1-1"].Children["SubSubAssem1-1"].Children["Part2-5"].ReferencedConfiguration.Name;
                s10_conf1 = assm.Configurations["Conf1"].Components["SubAssem1-1"].Children["SubSubAssem1-1"].Children["Part2-5"].State;
            }

            Assert.AreEqual("Default", c1_def);
            Assert.IsFalse(s1_def);
            Assert.AreEqual("Default", c2_def);
            Assert.IsFalse(s2_def);
            Assert.AreEqual("Default", c3_def);
            Assert.IsFalse(s3_def);
            Assert.AreEqual("Default", c4_def);
            Assert.IsFalse(s4_def);
            Assert.AreEqual("Default", c5_def);
            Assert.IsFalse(s5_def);
            Assert.AreEqual("Default", c6_def);
            Assert.AreEqual(ComponentState_e.Default, s6_def);
            Assert.AreEqual("Default", c7_def);
            Assert.AreEqual(ComponentState_e.Default, s7_def);
            Assert.AreEqual("Default", c8_def);
            Assert.AreEqual(ComponentState_e.Default, s8_def);
            Assert.AreEqual("Default", c9_def);
            Assert.AreEqual(ComponentState_e.Default, s9_def);
            Assert.AreEqual("Conf1", c10_def);
            Assert.AreEqual(ComponentState_e.Default, s10_def);

            Assert.AreEqual("Conf1", c1_conf1);
            Assert.IsFalse(s1_conf1);
            Assert.AreEqual("Conf1", c2_conf1);
            Assert.IsFalse(s2_conf1);
            Assert.AreEqual("Conf1", c3_conf1);
            Assert.IsFalse(s3_conf1);
            Assert.AreEqual("Conf1", c4_conf1);
            Assert.IsTrue(s4_conf1);
            Assert.AreEqual("Conf1", c5_conf1);
            Assert.IsFalse(s5_conf1);
            Assert.AreEqual("Conf1", c6_conf1);
            Assert.AreEqual(ComponentState_e.Envelope, s6_conf1);
            Assert.AreEqual("Conf1", c7_conf1);
            Assert.AreEqual(ComponentState_e.Suppressed, s7_conf1);
            Assert.AreEqual("Default", c8_conf1);
            Assert.AreEqual(ComponentState_e.ExcludedFromBom, s8_conf1);
            Assert.AreEqual("Default", c9_conf1);
            //Assert.AreEqual(ComponentState_e.Fixed, s9_conf1);
            Assert.AreEqual("Conf1", c10_conf1);
            Assert.AreEqual(ComponentState_e.Hidden, s10_conf1);
        }

        [Test]
        public void ComponentStateTest()
        {
            ComponentState_e s1;
            ComponentState_e s2;
            ComponentState_e s3;
            ComponentState_e s4;
            ComponentState_e s5;
            ComponentState_e s6;
            ComponentState_e s7;

            using (var doc = OpenDataDocument(@"Assembly5\Assem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                s1 = assm.Configurations.Active.Components["Part1-1"].State;
                s2 = assm.Configurations.Active.Components["Part1-2"].State;
                s3 = assm.Configurations.Active.Components["Part1-3"].State;
                s4 = assm.Configurations.Active.Components["Part1-4"].State;
                s5 = assm.Configurations.Active.Components["Part1-5"].State;
                s6 = assm.Configurations.Active.Components["Part2^Assem1-1"].State;
                s7 = assm.Configurations.Active.Components["Part1-6"].State;
            }

            Assert.AreEqual(ComponentState_e.Default, s1);
            Assert.AreEqual(ComponentState_e.Suppressed, s2);
            Assert.AreEqual(ComponentState_e.Envelope, s3);
            Assert.AreEqual(ComponentState_e.ExcludedFromBom, s4);
            Assert.AreEqual(ComponentState_e.Hidden, s5);
            Assert.AreEqual(ComponentState_e.Embedded | ComponentState_e.Fixed, s6);
            Assert.AreEqual(ComponentState_e.Fixed, s7);
        }

        [Test]
        public void ComponentNameTest()
        {
            string fn1;
            string fn2;
            string fn3;

            string n1;
            string n2;
            string n3;

            using (var doc = OpenDataDocument(@"Assembly10\Assem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                var c1 = assm.Configurations.Active.Components.First();
                var c2 = c1.Children.First();
                var c3 = c2.Children.First();

                n1 = c1.Name;
                n2 = c2.Name;
                n3 = c3.Name;

                fn1 = c1.FullName;
                fn2 = c2.FullName;
                fn3 = c3.FullName;
            }

            Assert.AreEqual("SubAssem1-1", fn1);
            Assert.AreEqual("SubAssem1-1/SubSubAssem1-1", fn2);
            Assert.AreEqual("SubAssem1-1/SubSubAssem1-1/Part1-1", fn3);
            Assert.AreEqual("SubAssem1-1", n1);
            Assert.AreEqual("SubSubAssem1-1", n2);
            Assert.AreEqual("Part1-1", n3);
        }

        [Test]
        public void ComponentSetStateTest()
        {
            using (var doc = OpenDataDocument(@"Assembly5\Assem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                var c1 = (ISwComponent)assm.Configurations.Active.Components["Part1-1"];
                var c2 = (ISwComponent)assm.Configurations.Active.Components["Part1-2"];
                var c3 = (ISwComponent)assm.Configurations.Active.Components["Part1-4"];
                var c4 = (ISwComponent)assm.Configurations.Active.Components["Part1-5"];
                var c5 = (ISwComponent)assm.Configurations.Active.Components["Part3-1"];
                var c6 = (ISwComponent)assm.Configurations.Active.Components["Part2^Assem1-1"];
                var c7 = (ISwComponent)assm.Configurations.Active.Components["Part1-6"];

                c1.State = ComponentState_e.Suppressed;
                c2.State = ComponentState_e.Hidden | ComponentState_e.Fixed;
                c3.State = ComponentState_e.Default;
                c4.State = ComponentState_e.ExcludedFromBom | ComponentState_e.Hidden;
                c5.State = ComponentState_e.Embedded;
                c6.State = ComponentState_e.Embedded | ComponentState_e.Lightweight;
                c7.State = ComponentState_e.Default;

                Assert.AreEqual((int)swComponentSuppressionState_e.swComponentSuppressed, c1.Component.GetSuppression2());
                Assert.IsTrue(c2.Component.IsHidden(false));
                Assert.IsTrue(c2.Component.IsFixed());
                Assert.IsFalse(c3.Component.ExcludeFromBOM);
                Assert.IsTrue(c4.Component.IsHidden(false));
                Assert.IsTrue(c4.Component.ExcludeFromBOM);
                Assert.That(c4.Component.GetSuppression2() == (int)swComponentSuppressionState_e.swComponentResolved || c4.Component.GetSuppression2() == (int)swComponentSuppressionState_e.swComponentFullyResolved);
                Assert.IsTrue(c5.Component.IsVirtual);
                Assert.That(c6.Component.GetSuppression2() == (int)swComponentSuppressionState_e.swComponentLightweight || c4.Component.GetSuppression2() == (int)swComponentSuppressionState_e.swComponentFullyLightweight);
                Assert.IsFalse(c7.Component.IsFixed());
            }
        }

        [Test]
        public void TransformTest()
        {
            TransformMatrix m1;
            TransformMatrix m2;
            TransformMatrix m3;

            using (var doc = OpenDataDocument(@"AssemTransform1\Assem1.SLDASM"))
            {
                var assm = (IXAssembly)m_App.Documents.Active;
                m1 = assm.Configurations.Active.Components["Part1-1"].Transformation;
                m2 = assm.Configurations.Active.Components["Assem2-1"].Transformation;
                m3 = assm.Configurations.Active.Components["Assem2-1"].Children["Part1-1"].Transformation;
            }

            Assert.That(m1.M11, Is.EqualTo(1).Within(0.00000000001).Percent);
            Assert.That(m1.M12, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(m1.M13, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(m1.M14, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(m1.M21, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(m1.M22, Is.EqualTo(1).Within(0.00000000001).Percent);
            Assert.That(m1.M23, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(m1.M24, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(m1.M31, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(m1.M32, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(m1.M33, Is.EqualTo(1).Within(0.00000000001).Percent);
            Assert.That(m1.M34, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(m1.M41, Is.EqualTo(0.1).Within(0.00000000001).Percent);
            Assert.That(m1.M42, Is.EqualTo(0.2).Within(0.00000000001).Percent);
            Assert.That(m1.M43, Is.EqualTo(0.3).Within(0.00000000001).Percent);
            Assert.That(m1.M44, Is.EqualTo(1).Within(0.00000000001).Percent);

            Assert.That(m2.M11, Is.EqualTo(0.778911219112665).Within(0.00000000001).Percent);
            Assert.That(m2.M12, Is.EqualTo(-0.315828483619943).Within(0.00000000001).Percent);
            Assert.That(m2.M13, Is.EqualTo(-0.541802253294271).Within(0.00000000001).Percent);
            Assert.That(m2.M14, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(m2.M21, Is.EqualTo(0.440686764398983).Within(0.00000000001).Percent);
            Assert.That(m2.M22, Is.EqualTo(0.890321262112265).Within(0.00000000001).Percent);
            Assert.That(m2.M23, Is.EqualTo(0.114556649367804).Within(0.00000000001).Percent);
            Assert.That(m2.M24, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(m2.M31, Is.EqualTo(0.446197813109809).Within(0.00000000001).Percent);
            Assert.That(m2.M32, Is.EqualTo(-0.327994541364868).Within(0.00000000001).Percent);
            Assert.That(m2.M33, Is.EqualTo(0.832662652225301).Within(0.00000000001).Percent);
            Assert.That(m2.M34, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(m2.M41, Is.EqualTo(1.19494753551567E-02).Within(0.00000000001).Percent);
            Assert.That(m2.M42, Is.EqualTo(0.238358452555315).Within(0.00000000001).Percent);
            Assert.That(m2.M43, Is.EqualTo(0.283118040140713).Within(0.00000000001).Percent);
            Assert.That(m2.M44, Is.EqualTo(1).Within(0.00000000001).Percent);

            Assert.That(m3.M11, Is.EqualTo(0.469471562785889).Within(0.00000000001).Percent);
            Assert.That(m3.M12, Is.EqualTo(0.323601375916072).Within(0.00000000001).Percent);
            Assert.That(m3.M13, Is.EqualTo(-0.821509952003383).Within(0.00000000001).Percent);
            Assert.That(m3.M14, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(m3.M21, Is.EqualTo(0.387058748836215).Within(0.00000000001).Percent);
            Assert.That(m3.M22, Is.EqualTo(0.760826796642461).Within(0.00000000001).Percent);
            Assert.That(m3.M23, Is.EqualTo(0.520891649443639).Within(0.00000000001).Percent);
            Assert.That(m3.M24, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(m3.M31, Is.EqualTo(0.79358803965579).Within(0.00000000001).Percent);
            Assert.That(m3.M32, Is.EqualTo(-0.562516430885353).Within(0.00000000001).Percent);
            Assert.That(m3.M33, Is.EqualTo(0.231933801545365).Within(0.00000000001).Percent);
            Assert.That(m3.M34, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(m3.M41, Is.EqualTo(0.23870483272917).Within(0.00000000001).Percent);
            Assert.That(m3.M42, Is.EqualTo(6.52374740041025E-02).Within(0.00000000001).Percent);
            Assert.That(m3.M43, Is.EqualTo(0.673600359515548).Within(0.00000000001).Percent);
            Assert.That(m3.M44, Is.EqualTo(1).Within(0.00000000001).Percent);
        }

        [Test]
        public void ComponentColorTest()
        {
            System.Drawing.Color? c1;
            System.Drawing.Color? c2;
            System.Drawing.Color? c3;

            double[] mat1;
            double[] mat2;

            using (var doc = OpenDataDocument(@"ColorAssembly\Assem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                c1 = assm.Configurations.Active.Components["Part1-1"].Color;
                c2 = assm.Configurations.Active.Components["Part1-2"].Color;
                c3 = assm.Configurations.Active.Components["Part1-3"].Color;

                assm.Configurations.Active.Components["Part1-3"].Color = System.Drawing.Color.FromArgb(100, 50, 150, 250);
                assm.Configurations.Active.Components["Part1-1"].Color = null;

                mat1 = (double[])((ISwComponent)assm.Configurations.Active.Components["Part1-3"]).Component.GetMaterialPropertyValues2((int)swInConfigurationOpts_e.swThisConfiguration, null);
                mat2 = (double[])((ISwComponent)assm.Configurations.Active.Components["Part1-1"]).Component.GetMaterialPropertyValues2((int)swInConfigurationOpts_e.swThisConfiguration, null);
            }

            Assert.IsNotNull(c1);
            Assert.AreEqual(255, c1.Value.A);
            Assert.AreEqual(103, c1.Value.R);
            Assert.AreEqual(229, c1.Value.G);
            Assert.AreEqual(255, c1.Value.B);

            Assert.IsNotNull(c2);
            Assert.AreEqual(127, c2.Value.A);
            Assert.AreEqual(255, c2.Value.R);
            Assert.AreEqual(63, c2.Value.G);
            Assert.AreEqual(103, c2.Value.B);

            Assert.IsNull(c3);

            Assert.AreEqual(50, (int)(mat1[0] * 255));
            Assert.AreEqual(150, (int)(mat1[1] * 255));
            Assert.AreEqual(250, (int)(mat1[2] * 255));
            Assert.AreEqual(100, (int)((1f - mat1[7]) * 255));

            Assert.IsTrue(mat2.All(m => m == -1));
        }

        [Test]
        public void AddComponentsTest()
        {
            using (var partDoc = OpenDataDocument("Box1.sldprt"))
            {
                var doc4 = (IXPart)m_App.Documents.Active;

                using (var doc = NewDocument(swDocumentTypes_e.swDocASSEMBLY))
                {
                    var assm = (ISwAssembly)m_App.Documents.Active;

                    var doc1 = (IXPart)m_App.Documents.PreCreateFromPath(GetFilePath("Cylinder1.sldprt"));
                    var doc2 = m_App.Documents.PreCreatePart();
                    var doc3 = m_App.Documents.PreCreateAssembly();

                    var comp1 = assm.Configurations.Active.Components.PreCreate<IXPartComponent>();
                    comp1.ReferencedDocument = doc1;
                    comp1.State = ComponentState_e.Fixed;

                    var comp2 = assm.Configurations.Active.Components.PreCreate<IXPartComponent>();
                    comp2.ReferencedDocument = doc1;
                    comp2.ReferencedConfiguration = (IXPartConfiguration)doc1.Configurations["Conf1"];
                    comp2.Transformation = TransformMatrix.CreateFromTranslation(new Vector(0.1, 0.2, 0.3));
                    comp2.State = ComponentState_e.Fixed;

                    var comp3 = assm.Configurations.Active.Components.PreCreate<IXPartComponent>();
                    comp3.ReferencedDocument = doc2;
                    comp3.State = ComponentState_e.Embedded;

                    var comp4 = assm.Configurations.Active.Components.PreCreate<IXPartComponent>();
                    comp4.ReferencedDocument = doc2;
                    comp4.State = ComponentState_e.Embedded;
                    comp4.Transformation = TransformMatrix.CreateFromRotationAroundAxis(new Vector(1, 0, 0), Math.PI / 4, new Point(0, 0, 0));

                    var comp5 = assm.Configurations.Active.Components.PreCreate<IXAssemblyComponent>();
                    comp5.ReferencedDocument = doc3;
                    comp5.State = ComponentState_e.Embedded;

                    var comp6 = assm.Configurations.Active.Components.PreCreate<IXPartComponent>();
                    comp6.ReferencedDocument = doc4;
                    comp6.State = ComponentState_e.Suppressed;

                    var comp7 = assm.Configurations.Active.Components.PreCreate<IXPartComponent>();
                    comp7.ReferencedDocument = doc4;
                    comp7.State = ComponentState_e.ExcludedFromBom;

                    var comp8 = assm.Configurations.Active.Components.PreCreate<IXPartComponent>();
                    comp8.ReferencedDocument = doc4;
                    comp8.State = ComponentState_e.Hidden;

                    assm.Configurations.Active.Components.AddRange(new IXComponent[] { comp1, comp2, comp3, comp4, comp5, comp6, comp7, comp8 });

                    var comp3ModelDoc = (IModelDoc2)((ISwComponent)comp3).Component.GetModelDoc2();

                    Assert.That(string.Equals(Path.GetFileName(((IModelDoc2)((ISwComponent)comp1).Component.GetModelDoc2()).GetPathName()), "Cylinder1.sldprt", StringComparison.CurrentCultureIgnoreCase));
                    Assert.That(((ISwComponent)comp1).Component.IsFixed());
                    Assert.That(comp1.ReferencedDocument.IsCommitted);

                    Assert.That(string.Equals(Path.GetFileName(((IModelDoc2)((ISwComponent)comp2).Component.GetModelDoc2()).GetPathName()), "Cylinder1.sldprt", StringComparison.CurrentCultureIgnoreCase));
                    Assert.That(((ISwComponent)comp2).Component.IsFixed());
                    Assert.AreEqual("Conf1", ((ISwComponent)comp2).Component.ReferencedConfiguration);
                    AssertCompareDoubleArray((double[])((ISwComponent)comp2).Component.Transform2.ArrayData, new double[] { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0.1, 0.2, 0.3, 1, 0, 0, 0 });
                    Assert.That(comp2.ReferencedDocument.IsCommitted);

                    Assert.That(!((ISwComponent)comp3).Component.IsFixed());
                    Assert.AreEqual((int)swDocumentTypes_e.swDocPART, comp3ModelDoc.GetType());
                    Assert.That(((ISwComponent)comp3).Component.IsVirtual);
                    Assert.That(comp4.ReferencedDocument.IsCommitted);

                    Assert.That(!((ISwComponent)comp4).Component.IsFixed());
                    Assert.AreEqual(comp3ModelDoc, (IModelDoc2)((ISwComponent)comp4).Component.GetModelDoc2());
                    Assert.That(((ISwComponent)comp4).Component.IsVirtual);
                    AssertCompareDoubleArray((double[])((ISwComponent)comp4).Component.Transform2.ArrayData, new double[] { 1, 0, 0, 0, 0.70710678118654757, 0.70710678118654746, 0, -0.70710678118654746, 0.70710678118654757, 0, 0, 0, 1, 0, 0, 0 });
                    Assert.That(comp5.ReferencedDocument.IsCommitted);

                    Assert.That(!((ISwComponent)comp5).Component.IsFixed());
                    Assert.AreEqual((int)swDocumentTypes_e.swDocASSEMBLY, ((IModelDoc2)((ISwComponent)comp5).Component.GetModelDoc2()).GetType());
                    Assert.That(((ISwComponent)comp5).Component.IsVirtual);
                    Assert.That(comp6.ReferencedDocument.IsCommitted);

                    Assert.That(string.Equals(Path.GetFileName(comp6.ReferencedDocument.Path), "Box1.sldprt", StringComparison.CurrentCultureIgnoreCase));
                    Assert.That(((ISwComponent)comp6).Component.IsSuppressed());
                    Assert.That(comp7.ReferencedDocument.IsCommitted);

                    Assert.That(string.Equals(Path.GetFileName(((IModelDoc2)((ISwComponent)comp7).Component.GetModelDoc2()).GetPathName()), "Box1.sldprt", StringComparison.CurrentCultureIgnoreCase));
                    Assert.That(((ISwComponent)comp7).Component.ExcludeFromBOM);
                    Assert.That(comp7.ReferencedDocument.IsCommitted);

                    Assert.That(string.Equals(Path.GetFileName(((IModelDoc2)((ISwComponent)comp8).Component.GetModelDoc2()).GetPathName()), "Box1.sldprt", StringComparison.CurrentCultureIgnoreCase));
                    Assert.That(((ISwComponent)comp8).Component.IsHidden(false));
                    Assert.That(comp8.ReferencedDocument.IsCommitted);

                    Assert.AreEqual(5, m_App.Documents.Count);
                    Assert.That(m_App.Documents.Contains(assm));
                    Assert.That(m_App.Documents.Contains(doc1));
                    Assert.That(m_App.Documents.Contains(doc2));
                    Assert.That(m_App.Documents.Contains(doc3));
                    Assert.That(m_App.Documents.Contains(doc4));
                }
            }
        }

        [Test]
        public void EditComponentInContext() 
        {
            IXComponent editComp1;
            string editComp2;
            int bodyCount;
            double[] vols;

            using (var doc = OpenDataDocument(@"Assembly5\Assem1.SLDASM", false))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                var comp = assm.Configurations.Active.Components["Part1-1"];

                editComp1 = assm.EditingComponent;

                using (var editor = comp.Edit()) 
                {
                    editComp2 = assm.EditingComponent?.Name;

                    var dumbBodyFeat = comp.Features.PreCreateDumbBody();
                    dumbBodyFeat.Body = m_App.MemoryGeometryBuilder.CreateSolidBox(new Point(0, 0, 0), new Vector(1, 0, 0), new Vector(0, 1, 0), 0.1, 0.2, 0.3).Bodies.First();
                    dumbBodyFeat.Commit();
                }

                bodyCount = comp.Bodies.Count;
                vols = comp.Bodies.Where(b => !string.Equals(b.Name, "Boss-Extrude1", StringComparison.CurrentCultureIgnoreCase)).Select(b => ((IXSolidBody)b).Volume).ToArray();
            }

            Assert.IsNull(editComp1);
            Assert.AreEqual("Part1-1", editComp2);
            Assert.AreEqual(2, bodyCount);
            Assert.AreEqual(1, vols.Length);
            Assert.That(vols[0], Is.EqualTo(0.006).Within(0.00000000001).Percent);
        }

        [Test]
        public void InsertComponentEventTest()
        {
            var res = new List<Tuple<string, string>>();
            string compName;

            using (var doc = OpenDataDocument(@"Assembly1\TopAssem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                assm.ComponentInserted += (a, c) => 
                {
                    res.Add(new Tuple<string, string>(a.Path, c.Name));
                };

                using (var partDoc = OpenDataDocument("BBox1.SLDPRT"))
                {
                    var part = (ISwPart)m_App.Documents.Active;
                    compName = assm.Assembly.AddComponent5(part.Path, (int)swAddComponentConfigOptions_e.swAddComponentConfigOptions_CurrentSelectedConfig, "", false, "", 0, 0, 0).Name2;
                }
            }

            Assert.AreEqual(1, res.Count);
            Assert.AreEqual(GetFilePath(@"Assembly1\TopAssem1.SLDASM").ToLower(), res[0].Item1.ToLower());
            Assert.AreEqual(compName, res[0].Item2);
        }

        [Test]
        public void InsertMultipleComponentEventTest()
        {
            var res = new List<Tuple<string, string>>();
            string[] compNames;

            using (var doc = OpenDataDocument(@"Assembly1\TopAssem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                assm.ComponentInserted += (a, c) =>
                {
                    res.Add(new Tuple<string, string>(a.Path, c.Name));
                };

                compNames = ((object[])assm.Assembly.AddComponents3(
                    Enumerable.Repeat(GetFilePath("BBox1.SLDPRT"), 3).ToArray(),
                    Enumerable.Repeat(new double[] { 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0 }, 3).SelectMany(x => x).ToArray(),
                    Enumerable.Repeat("", 3).ToArray())).Cast<IComponent2>().Select(c => c.Name2).ToArray();
            }

            var assmPath = GetFilePath(@"Assembly1\TopAssem1.SLDASM");

            Assert.AreEqual(3, res.Count);
            Assert.AreEqual(assmPath.ToLower(), res[0].Item1.ToLower());
            Assert.AreEqual(assmPath.ToLower(), res[1].Item1.ToLower());
            Assert.AreEqual(assmPath.ToLower(), res[2].Item1.ToLower());

            CollectionAssert.AreEquivalent(compNames, res.Select(r => r.Item2));
        }

        [Test]
        public void DeleteComponentEventTest()
        {
            IXComponent[] comps;
            var resDeleting = new List<Tuple<string, string>>();
            var resDeleted = new List<Tuple<string, IXComponent>>();

            using (var doc = OpenDataDocument(@"Assembly1\TopAssem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                comps = new IXComponent[]
                {
                    assm.Configurations.Active.Components["Part1-1"],
                    assm.Configurations.Active.Components["Part1-2"],
                    assm.Configurations.Active.Components["SubAssem1-2"]
                };

                assm.ComponentDeleting += (a, c, _) =>
                {
                    resDeleting.Add(new Tuple<string, string>(a.Path, c.Name));
                };

                assm.ComponentDeleted += (a, c) =>
                {
                    resDeleted.Add(new Tuple<string, IXComponent>(a.Path, c));
                };

                assm.Model.Extension.MultiSelect2(comps.Select(c => new DispatchWrapper(((ISwComponent)c).Component)).ToArray(), false, null);
                assm.Model.Extension.DeleteSelection2((int)swDeleteSelectionOptions_e.swDelete_Absorbed);
            }

            var assmPath = GetFilePath(@"Assembly1\TopAssem1.SLDASM");

            Assert.AreEqual(3, resDeleting.Count);
            Assert.AreEqual(3, resDeleted.Count);

            Assert.AreEqual(assmPath.ToLower(), resDeleting[0].Item1.ToLower());
            Assert.AreEqual(assmPath.ToLower(), resDeleting[1].Item1.ToLower());
            Assert.AreEqual(assmPath.ToLower(), resDeleting[2].Item1.ToLower());

            Assert.AreEqual(assmPath.ToLower(), resDeleted[0].Item1.ToLower());
            Assert.AreEqual(assmPath.ToLower(), resDeleted[1].Item1.ToLower());
            Assert.AreEqual(assmPath.ToLower(), resDeleted[2].Item1.ToLower());

            CollectionAssert.AreEquivalent(new string[] { "Part1-1", "Part1-2", "SubAssem1-2" }, resDeleting.Select(r => r.Item2));
            Assert.IsTrue(comps.Any(c => c.Equals(resDeleted[0].Item2)));
            Assert.IsTrue(comps.Any(c => c.Equals(resDeleted[1].Item2)));
            Assert.IsTrue(comps.Any(c => c.Equals(resDeleted[2].Item2)));
        }

        [Test]
        public void SuppressedPatternsTest()
        {
            string[] compNames;
            bool[] suppStates;

            using (var doc = OpenDataDocument(@"SuppressedCompsPattern1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                var comps = assm.Configurations.Active.Components.Flatten().ToArray();

                compNames = comps.Select(c => c.Name).ToArray();
                suppStates = comps.Select(c => c.State.HasFlag(ComponentState_e.Suppressed)).ToArray();
            }

            CollectionAssert.AreEqual(new string[] 
            {
                "Part1^SuppressedCompsPattern1-1",
                "Part1^SuppressedCompsPattern1-2",
                "Part1^SuppressedCompsPattern1-3",
                "Part1^SuppressedCompsPattern1-4",
                "Part1^SuppressedCompsPattern1-5",
                "Part1^SuppressedCompsPattern1-6"
            }, compNames);

            CollectionAssert.AreEqual(new bool[]
            {
                false,
                true,
                true,
                true,
                true,
                true
            }, suppStates);
        }

        [Test]
        public void ConfigsDifferentCountTest()
        {
            string[] compNamesDef;
            string[] compNamesConf1;

            using (var doc = OpenDataDocument(@"AssemPatternDiffConf1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                var compsDef = assm.Configurations["Default"].Components.Flatten().ToArray();
                var compsConf1 = assm.Configurations["Conf1"].Components.Flatten().ToArray();

                compNamesDef = compsDef.Select(c => c.Name).ToArray();
                compNamesConf1 = compsConf1.Select(c => c.Name).ToArray();
            }

            CollectionAssert.AreEquivalent(new string[]
            {
                "Part1^AssemPatternDiffConf1-1",
                "Part1^AssemPatternDiffConf1-2",
                "Part1^AssemPatternDiffConf1-3",
                "Part1^AssemPatternDiffConf1-4",
                "Part1^AssemPatternDiffConf1-7",
                "Part1^AssemPatternDiffConf1-8",
                "Assem1^AssemPatternDiffConf1-1",
                "Part1^Assem1_AssemPatternDiffConf1-1",
                "Part1^Assem1_AssemPatternDiffConf1-2",
                "Part1^Assem1_AssemPatternDiffConf1-3",
            }, compNamesDef);

            CollectionAssert.AreEquivalent(new string[]
            {
                "Part1^AssemPatternDiffConf1-1",
                "Part1^AssemPatternDiffConf1-2",
                "Part1^AssemPatternDiffConf1-3",
                "Part1^AssemPatternDiffConf1-4",
                "Part1^AssemPatternDiffConf1-7",
                "Part1^AssemPatternDiffConf1-8",
                "Assem1^AssemPatternDiffConf1-1",
                "Part1^Assem1_AssemPatternDiffConf1-1",
                "Part1^Assem1_AssemPatternDiffConf1-2",
                "Part1^Assem1_AssemPatternDiffConf1-3",
                "Part1^Assem1_AssemPatternDiffConf1-4",
                "Part1^Assem1_AssemPatternDiffConf1-5",
                "Part1^AssemPatternDiffConf1-5",
                "Part1^AssemPatternDiffConf1-6"
            }, compNamesConf1);
        }

        [Test]
        public void ComponentReferenceTest()
        {
            string r1;
            string r2;
            string r3;
            string r4;
            string r5;

            using (var doc = OpenDataDocument(@"Assembly15\Assem15.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                r1 = assm.Configurations.Active.Components["Part1-1"].Reference;
                r2 = assm.Configurations.Active.Components["Part1-2"].Reference;
                r3 = assm.Configurations.Active.Components["Assem2^Assem15-1"].Reference;
                r4 = assm.Configurations.Active.Components["Assem2^Assem15-1"].Children["Part2^Assem2_Assem15-1"].Reference;
                assm.Configurations.Active.Components["Part1-2"].Reference = "E";
                r5 = ((ISwComponent)assm.Configurations.Active.Components["Part1-2"]).Component.ComponentReference;
            }

            Assert.AreEqual("A", r1);
            Assert.AreEqual("B", r2);
            Assert.AreEqual("C", r3);
            Assert.AreEqual("D", r4);
            Assert.AreEqual("E", r5);
        }
    }
}
