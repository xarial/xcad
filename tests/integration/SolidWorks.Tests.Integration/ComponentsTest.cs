using NUnit.Framework;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
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

            Assert.That(compNames.OrderBy(c => c).SequenceEqual(
                new string[] { "Part1-1", "Part1-2", "SubAssem1-1", "SubAssem1-2", "SubAssem2-1", "Part1-3" }.OrderBy(c => c)));
        }

        [Test]
        public void IterateSubComponentsTest()
        {
            string[] compNames;

            using (var doc = OpenDataDocument(@"Assembly1\TopAssem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;
                var comp = assm.Configurations.Active.Components["SubAssem1-1"];
                compNames = comp.Children.Select(c => c.Name).ToArray();
            }

            Assert.That(compNames.OrderBy(c => c).SequenceEqual(
                new string[] { "SubAssem1-1/Part2-1", "SubAssem1-1/SubSubAssem1-1" }.OrderBy(c => c)));
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
            
            Assert.That(featNames.SequenceEqual(
                new string[] { "Favorites", "Selection Sets", "Sensors", "Design Binder", "Annotations", "Notes", "Notes1___EndTag___", "Surface Bodies", "Solid Bodies", "Lights, Cameras and Scene", "Ambient", "Directional1", "Directional2", "Directional3", "Markups", "Equations", "Material <not specified>", "Front Plane", "Top Plane", "Right Plane", "Origin", "Sketch1", "Boss-Extrude1" }));
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

            string c1_conf1;
            bool s1_conf1;
            string c2_conf1;
            bool s2_conf1;
            string c3_conf1;
            bool s3_conf1;
            string c4_conf1;
            bool s4_conf1;

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

                c1_conf1 = assm.Configurations["Conf1"].Components["SubAssem1-1"].ReferencedConfiguration.Name;
                s1_conf1 = assm.Configurations["Conf1"].Components["SubAssem1-1"].State.HasFlag(ComponentState_e.Suppressed);
                c2_conf1 = assm.Configurations["Conf1"].Components["Part1-1"].ReferencedConfiguration.Name;
                s2_conf1 = assm.Configurations["Conf1"].Components["Part1-1"].State.HasFlag(ComponentState_e.Suppressed);
                c3_conf1 = assm.Configurations["Conf1"].Components["SubAssem1-1"].Children["Part1-1"].ReferencedConfiguration.Name;
                s3_conf1 = assm.Configurations["Conf1"].Components["SubAssem1-1"].Children["Part1-1"].State.HasFlag(ComponentState_e.Suppressed);
                c4_conf1 = assm.Configurations["Conf1"].Components["SubAssem1-1"].Children["Part1-2"].ReferencedConfiguration.Name;
                s4_conf1 = assm.Configurations["Conf1"].Components["SubAssem1-1"].Children["Part1-2"].State.HasFlag(ComponentState_e.Suppressed);
            }

            Assert.AreEqual("Default", c1_def);
            Assert.IsFalse(s1_def);
            Assert.AreEqual("Default", c2_def);
            Assert.IsFalse(s2_def);
            Assert.AreEqual("Default", c3_def);
            Assert.IsFalse(s3_def);
            Assert.AreEqual("Default", c4_def);
            Assert.IsFalse(s4_def);

            Assert.AreEqual("Conf1", c1_conf1);
            Assert.IsFalse(s1_conf1);
            Assert.AreEqual("Conf1", c2_conf1);
            Assert.IsFalse(s2_conf1);
            Assert.AreEqual("Conf1", c3_conf1);
            Assert.IsFalse(s3_conf1);
            Assert.AreEqual("Conf1", c4_conf1);
            Assert.IsTrue(s4_conf1);
        }

        [Test]
        public void ComponentStateTest()
        {
            ComponentState_e s1;
            ComponentState_e s2;
            ComponentState_e s3;
            ComponentState_e s4;
            ComponentState_e s5;

            using (var doc = OpenDataDocument(@"Assembly5\Assem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                s1 = assm.Configurations.Active.Components["Part1-1"].State;
                s2 = assm.Configurations.Active.Components["Part1-2"].State;
                s3 = assm.Configurations.Active.Components["Part1-3"].State;
                s4 = assm.Configurations.Active.Components["Part1-4"].State;
                s5 = assm.Configurations.Active.Components["Part1-5"].State;
            }

            Assert.AreEqual(ComponentState_e.Default, s1);
            Assert.AreEqual(ComponentState_e.Suppressed, s2);
            Assert.AreEqual(ComponentState_e.Envelope, s3);
            Assert.AreEqual(ComponentState_e.ExcludedFromBom, s4);
            Assert.AreEqual(ComponentState_e.Hidden, s5);
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
    }
}
