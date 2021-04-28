using NUnit.Framework;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents.Enums;
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

                var doc1 = assm.Configurations.Active.Components["Part1-1"].Document;
                doc1FileName = Path.GetFileName(doc1.Path);
                doc1Contains = m_App.Documents.Contains(doc1);

                var doc2 = assm.Configurations.Active.Components["SubAssem1-1"].Document;
                doc2FileName = Path.GetFileName(doc2.Path);
                doc2Contains = m_App.Documents.Contains(doc2);

                var d = assm.Configurations.Active.Components["Part1-2"].Document;

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

                var doc1 = assm.Configurations.Active.Components["Part1-1"].Document;
                var doc2 = assm.Configurations.Active.Components["Assem2-1"].Document;
                var doc3 = assm.Configurations.Active.Components["Assem2-1"].Children["Part2-1"].Document;
                var doc4 = assm.Configurations.Active.Components["Assem2-1"].Children["Part3-1"].Document;
                var doc5 = assm.Configurations.Active.Components["Assem1-1"].Document;

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

                var comp = SwObjectFactory.FromDispatch<ISwComponent>(swComp, assm);

                var doc1 = comp.Document;
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

            using (var doc = OpenDataDocument(@"VirtAssem1.SLDASM"))
            {
                compNames = ((ISwAssembly)m_App.Documents.Active).Configurations.Active.Components.Select(c => c.Name).ToArray();
            }

            Assert.That(compNames.OrderBy(c => c).SequenceEqual(
                new string[] { "Part1^VirtAssem1-1", "Assem2^VirtAssem1-1" }.OrderBy(c => c)));
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
    }
}
