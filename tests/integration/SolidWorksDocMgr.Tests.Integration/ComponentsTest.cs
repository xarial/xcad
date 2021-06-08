using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.SwDocumentManager.Documents;

namespace SolidWorksDocMgr.Tests.Integration
{
    public class ComponentsTest : IntegrationTests
    {
        [Test]
        public void IterateRootComponentsTest()
        {
            string[] compNames;

            using (var doc = OpenDataDocument(@"Assembly1\TopAssem1.SLDASM"))
            {
                compNames = ((ISwDmAssembly)m_App.Documents.Active).Configurations.Active.Components.Select(c => c.Name).ToArray();
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
                var assm = (ISwDmAssembly)m_App.Documents.Active;
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
            DocumentState_e state1;

            using (var doc = OpenDataDocument(@"Assembly1\TopAssem1.SLDASM"))
            {
                var assm = (ISwDmAssembly)m_App.Documents.Active;

                var doc1 = assm.Configurations.Active.Components["Part1-1"].Document;
                state1 = doc1.State;

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
            Assert.AreEqual(DocumentState_e.ReadOnly, state1);
            Assert.IsTrue(doc1Contains);
            Assert.IsTrue(doc2Contains);
        }
        
        [Test]
        public void GetComponentRefConfigTest()
        {
            using (var doc = OpenDataDocument(@"Assembly2\TopAssem.SLDASM"))
            {
                var assm = (ISwDmAssembly)m_App.Documents.Active;

                var conf1 = assm.Configurations.Active.Components["Part1-1"].ReferencedConfiguration;
                var conf2 = assm.Configurations.Active.Components["Assem2-1"].ReferencedConfiguration;
                var conf3 = assm.Configurations.Active.Components["Assem2-1"].Children["Part2-1"].ReferencedConfiguration;
                var conf4 = assm.Configurations.Active.Components["Part4-1 (XYZ)-2"].ReferencedConfiguration;
                var conf5 = assm.Configurations.Active.Components["Assem1-1"].ReferencedConfiguration;

                Assert.AreEqual("Default", conf1.Name);
                Assert.AreEqual("Default", conf2.Name);
                Assert.AreEqual("Default", conf3.Name);
                Assert.AreEqual("1-1", conf4.Name);
                Assert.AreEqual("Default", conf5.Name);
            }
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
                var comps = ((ISwDmAssembly)m_App.Documents.Active).Configurations.Active.Components;
                compNames = comps.Select(c => c.Name).ToArray();
                var docs = comps.Select(c => c.Document).ToArray();
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
        public void MovedCachedRefsAssemblyTest()
        {
            string[] paths;
            bool[] isCommitted;

            using (var doc = OpenDataDocument(@"MovedNonOpenedAssembly1\TopAssembly.SLDASM"))
            {
                var comps = ((ISwDmAssembly)m_App.Documents.Active).Configurations.Active.Components.Flatten().ToArray();
                paths = comps.Select(c => c.Path).ToArray();
                isCommitted = comps.Select(c => c.Document.IsCommitted).ToArray();
            }

            var dir = Path.GetDirectoryName(GetFilePath(@"MovedNonOpenedAssembly1\TopAssembly.SLDASM"));

            Assert.AreEqual(2, paths.Length);
            Assert.That(isCommitted.All(d => d));
            Assert.That(paths.Any(d => string.Equals(d, Path.Combine(dir, "Assemblies\\Assem1.SLDASM"), StringComparison.CurrentCultureIgnoreCase)));
            Assert.That(paths.Any(d => string.Equals(d, Path.Combine(dir, "Parts\\Part1.SLDPRT"), StringComparison.CurrentCultureIgnoreCase)));
        }

        [Test]
        public void MovedCachedRefsAssemblyExtFolderTest()
        {
            string[] paths;
            bool[] isCommitted;

            using (var doc = OpenDataDocument(@"Assembly3\Assemblies\Assem1.SLDASM"))
            {
                var comps = ((ISwDmAssembly)m_App.Documents.Active).Configurations.Active.Components.Flatten().ToArray();
                paths = comps.Select(c => c.Path).ToArray();
                isCommitted = comps.Select(c => c.Document.IsCommitted).ToArray();
            }

            var dir = GetFilePath(@"Assembly3");

            Assert.AreEqual(1, paths.Length);
            Assert.That(isCommitted.All(d => d));
            Assert.That(paths.Any(d => string.Equals(d, Path.Combine(dir, "Parts\\Part1.SLDPRT"), StringComparison.CurrentCultureIgnoreCase)));
        }

        [Test]
        public void ComponentCountTest()
        {
            int count;
            int totalCount;

            using (var doc = OpenDataDocument(@"Assembly1\TopAssem1.SLDASM"))
            {
                var assm = (ISwDmAssembly)m_App.Documents.Active;

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
                var assm = (ISwDmAssembly)m_App.Documents.Active;
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
                var assm = (ISwDmAssembly)m_App.Documents.Active;

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
    }
}
