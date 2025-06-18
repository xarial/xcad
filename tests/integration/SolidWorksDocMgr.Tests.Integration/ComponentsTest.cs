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
using Xarial.XCad.Documents.Extensions;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SwDocumentManager.Documents;
using Xarial.XCad.SwDocumentManager.Exceptions;

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
                compNames = ((ISwDmAssembly)doc.Document).Configurations.Active.Components.Select(c => c.Name).ToArray();
            }

            CollectionAssert.AreEquivalent(new string[] { "Part1-1", "Part1-2", "SubAssem1-1", "SubAssem1-2", "SubAssem2-1", "Part1-3" }, compNames);
        }

        [Test]
        public void IterateSubComponentsTest()
        {
            string[] compNames;

            using (var doc = OpenDataDocument(@"Assembly1\TopAssem1.SLDASM"))
            {
                var assm = (ISwDmAssembly)doc.Document;
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
                var assm = (ISwDmAssembly)doc.Document;
                rootCompNames = ((ISwDmAssembly)doc.Document).Configurations.Active.Components.Select(c => c.Name).ToArray();
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
                var assm = (ISwDmAssembly)doc.Document;
                rootCompNames = ((ISwDmAssembly)doc.Document).Configurations.Active.Components.Select(c => c.Name).ToArray();
                subCompNames = assm.Configurations.Active.Components["SubAssem1-1"].Children.Select(c => c.FullName).ToArray();
            }

            CollectionAssert.AreEqual(new string[] { "Part1-1", "Part1-3", "Part1-4", "Part1-5", "Part1-6", "Part1-7", "Part1-8", "Part1-9", "Part1-10", "Part1-11", "Part1-12", "Part1-13", "SubAssem1-1" }, rootCompNames);
            CollectionAssert.AreEqual(new string[] { "SubAssem1-1/Part2-1", "SubAssem1-1/Part2-2", "SubAssem1-1/Part2-3", "SubAssem1-1/Part2-4" }, subCompNames);
        }

        [Test]
        public void IterateComponentsSpeedPakTest()
        {
            string[] flattenCompNames;

            using (var doc = OpenDataDocument(@"Assembly17\Assem17.SLDASM"))
            {
                var assm = (IXAssembly)doc.Document;
                flattenCompNames = assm.Configurations.Active.Components.TryFlatten().Select(c => c.FullName).ToArray();

                Assert.Throws<SpeedPakConfigurationComponentsException>(() => assm.Configurations.Active.Components["SubAssem1-1"].Children.ToArray());
            }

            CollectionAssert.AreEquivalent(new string[] { "SubAssem1-2/Part1-1", "SubAssem1-1", "SubAssem1-2" }, flattenCompNames);
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
                var assm = (ISwDmAssembly)doc.Document;

                var doc1 = assm.Configurations.Active.Components["Part1-1"].ReferencedDocument;
                state1 = doc1.State;

                doc1FileName = Path.GetFileName(doc1.Path);
                doc1Contains = Application.Documents.Contains(doc1);

                var doc2 = assm.Configurations.Active.Components["SubAssem1-1"].ReferencedDocument;
                doc2FileName = Path.GetFileName(doc2.Path);
                doc2Contains = Application.Documents.Contains(doc2);

                var d = assm.Configurations.Active.Components["Part1-2"].ReferencedDocument;

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
                var assm = (ISwDmAssembly)doc.Document;

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
                var comps = ((ISwDmAssembly)doc.Document).Configurations.Active.Components;
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

            CollectionAssert.AreEquivalent(compNames, new string[] { "Part1^VirtAssem1-1", "Assem2^VirtAssem1-1" });
            Assert.That(isCommitted.All(x => x == true));
            Assert.That(isAlive.All(x => x == true));
            Assert.That(isVirtual.All(x => x == true));
        }

        [Test]
        public void SavingVirtualComponentsTest()
        {
            string p1;
            string p2;
            string p3;
            string p4;

            using (var dataFile = GetDataFile(@"VirtAssm2\Assem1.sldasm"))
            {
                var doc = Application.Documents.Open(dataFile.FilePath);
                var deps = doc.Dependencies.All.ToArray();

                var d1 = deps.FirstOrDefault(d => string.Equals(Path.GetFileNameWithoutExtension(d.Title),
                    "Part1^Assem1", StringComparison.CurrentCultureIgnoreCase));
                var d2 = deps.FirstOrDefault(d => string.Equals(Path.GetFileNameWithoutExtension(d.Title),
                    "Part2^SubAssem1", StringComparison.CurrentCultureIgnoreCase));
                var d3 = deps.FirstOrDefault(d => string.Equals(Path.GetFileNameWithoutExtension(d.Title),
                    "Part3^Assem3_Assem1", StringComparison.CurrentCultureIgnoreCase));
                var d4 = deps.FirstOrDefault(d => string.Equals(Path.GetFileNameWithoutExtension(d.Title),
                    "Assem3^Assem1", StringComparison.CurrentCultureIgnoreCase));

                ((ISwDmDocument)d1).Document.AddCustomProperty("UnitTest", SolidWorks.Interop.swdocumentmgr.SwDmCustomInfoType.swDmCustomInfoText, "xCAD");
                ((ISwDmDocument)d2).Document.AddCustomProperty("UnitTest", SolidWorks.Interop.swdocumentmgr.SwDmCustomInfoType.swDmCustomInfoText, "xCAD");
                ((ISwDmDocument)d3).Document.AddCustomProperty("UnitTest", SolidWorks.Interop.swdocumentmgr.SwDmCustomInfoType.swDmCustomInfoText, "xCAD");
                ((ISwDmDocument)d4).Document.AddCustomProperty("UnitTest", SolidWorks.Interop.swdocumentmgr.SwDmCustomInfoType.swDmCustomInfoText, "xCAD");

                d1.Save();
                d2.Save();
                d3.Save();
                d4.Save();

                doc.Close();

                doc = Application.Documents.Open(dataFile.FilePath);

                deps = doc.Dependencies.All.ToArray();

                d1 = deps.FirstOrDefault(d => string.Equals(Path.GetFileNameWithoutExtension(d.Title),
                    "Part1^Assem1", StringComparison.CurrentCultureIgnoreCase));
                d2 = deps.FirstOrDefault(d => string.Equals(Path.GetFileNameWithoutExtension(d.Title),
                    "Part2^SubAssem1", StringComparison.CurrentCultureIgnoreCase));
                d3 = deps.FirstOrDefault(d => string.Equals(Path.GetFileNameWithoutExtension(d.Title),
                    "Part3^Assem3_Assem1", StringComparison.CurrentCultureIgnoreCase));
                d4 = deps.FirstOrDefault(d => string.Equals(Path.GetFileNameWithoutExtension(d.Title),
                    "Assem3^Assem1", StringComparison.CurrentCultureIgnoreCase));

                p1 = ((ISwDmDocument)d1).Document.GetCustomProperty("UnitTest", out _);
                p2 = ((ISwDmDocument)d1).Document.GetCustomProperty("UnitTest", out _);
                p3 = ((ISwDmDocument)d1).Document.GetCustomProperty("UnitTest", out _);
                p4 = ((ISwDmDocument)d1).Document.GetCustomProperty("UnitTest", out _);
            }

            Assert.AreEqual("xCAD", p1);
            Assert.AreEqual("xCAD", p2);
            Assert.AreEqual("xCAD", p3);
            Assert.AreEqual("xCAD", p4);
        }

        [Test]
        public void MovedCachedRefsAssemblyTest()
        {
            string[] paths;
            bool[] isCommitted;

            string workFolder;

            using (var doc = OpenDataDocument(@"MovedNonOpenedAssembly1\TopAssembly.SLDASM"))
            {
                workFolder = doc.WorkFolderPath;

                var comps = ((ISwDmAssembly)doc.Document).Configurations.Active.Components.TryFlatten().ToArray();
                paths = comps.Select(c => c.ReferencedDocument.Path).ToArray();
                isCommitted = comps.Select(c => c.ReferencedDocument.IsCommitted).ToArray();
            }

            var dir = Path.GetDirectoryName(Path.Combine(workFolder, @"MovedNonOpenedAssembly1\TopAssembly.SLDASM"));

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

            string workFolder;

            using (var doc = OpenDataDocument(@"Assembly3\Assemblies\Assem1.SLDASM"))
            {
                workFolder = doc.WorkFolderPath;

                var comps = ((ISwDmAssembly)doc.Document).Configurations.Active.Components.TryFlatten().ToArray();
                paths = comps.Select(c => c.ReferencedDocument.Path).ToArray();
                isCommitted = comps.Select(c => c.ReferencedDocument.IsCommitted).ToArray();
            }

            var dir = Path.Combine(workFolder, "Assembly3");

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
                var assm = (ISwDmAssembly)doc.Document;

                count = assm.Configurations.Active.Components.Count;
                totalCount = assm.Configurations.Active.Components.TotalCount;
            }

            Assert.AreEqual(6, count);
            Assert.AreEqual(17, totalCount);
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
                var assm = (ISwDmAssembly)doc.Document;

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
                var assm = (ISwDmAssembly)doc.Document;
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
            
            using (var doc = OpenDataDocument(@"Assembly5\Assem1.SLDASM"))
            {
                var assm = (ISwDmAssembly)doc.Document;

                s1 = assm.Configurations.Active.Components["Part1-1"].State;
                s2 = assm.Configurations.Active.Components["Part1-2"].State;
                s3 = assm.Configurations.Active.Components["Part1-3"].State;
                s4 = assm.Configurations.Active.Components["Part1-4"].State;
                s5 = assm.Configurations.Active.Components["Part1-5"].State;
                s6 = assm.Configurations.Active.Components["Part2^Assem1-1"].State;
            }

            Assert.AreEqual(ComponentState_e.Default, s1);
            Assert.AreEqual(ComponentState_e.Suppressed, s2);
            Assert.AreEqual(ComponentState_e.Envelope, s3);
            Assert.AreEqual(ComponentState_e.ExcludedFromBom, s4);
            Assert.AreEqual(ComponentState_e.Hidden, s5);
            Assert.AreEqual(ComponentState_e.Embedded, s6);
        }

        [Test]
        public void TransformTest()
        {
            TransformMatrix m1;
            TransformMatrix m2;
            TransformMatrix m3;

            using (var doc = OpenDataDocument(@"AssemTransform1\Assem1.SLDASM"))
            {
                var assm = (IXAssembly)doc.Document;
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
        public void PathsTest()
        {
            var tempPath = Path.Combine(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            
            Dictionary<string, Tuple<string, bool>> refs;

            var destPath = Path.Combine(tempPath, "_Assembly11");

            string workFolder;

            try
            {
                using (var dataFile = GetDataFile(@"Assembly11\TopLevel\Assem1.sldasm"))
                {
                    workFolder = dataFile.WorkFolderPath;

                    UpdateSwReferences(dataFile.FilePath, workFolder);

                    CopyDirectory(Path.Combine(workFolder, "Assembly11"), destPath);

                    File.Delete(Path.Combine(destPath, @"Parts\Part4.sldprt"));
                    File.Delete(Path.Combine(destPath, @"SubAssemblies\Part2.sldprt"));
                    File.Delete(Path.Combine(workFolder, @"Assembly11\Parts\Part4.sldprt"));

                    var assm = (ISwDmAssembly)Application.Documents.Open(Path.Combine(destPath, @"TopLevel\Assem1.sldasm"));

                    refs = assm.Configurations.Active.Components.TryFlatten()
                        .ToDictionary(x => x.FullName, x => new Tuple<string, bool>(x.ReferencedDocument.Path.ToLower(), x.ReferencedDocument.IsCommitted), StringComparer.CurrentCultureIgnoreCase);

                    foreach (var comp in assm.Configurations.Active.Components.TryFlatten().ToArray())
                    {
                        var refDoc = comp.ReferencedDocument;

                        if (refDoc.IsCommitted && refDoc.IsAlive)
                        {
                            refDoc.Close();
                        }
                    }

                    assm.Close();
                }
            }
            finally
            {
                try
                {
                    Directory.Delete(tempPath, true);
                }
                catch //folder can be locked by SW while files can be deleted
                {
                    foreach (var file in Directory.GetFiles(tempPath, "*.*", SearchOption.AllDirectories))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch 
                        {
                        }
                    }
                }
            }

            Assert.AreEqual(13, refs.Count);

            Assert.AreEqual(new Tuple<string, bool>(Path.Combine(destPath, @"SubAssemblies\A\Assem2.SLDASM").ToLower(), true), refs["Assem2-1"]);
            Assert.AreEqual(new Tuple<string, bool>(Path.Combine(destPath, @"Parts\Part1.SLDPRT").ToLower(), true), refs["Part1-1"]);
            Assert.AreEqual(new Tuple<string, bool>(Path.Combine(destPath, @"SubAssemblies\Assem3.SLDASM").ToLower(), true), refs["Assem3-1"]);
            Assert.That(refs["Part6^Assem1-1"].Item1.EndsWith("Part6^Assem1.sldprt", StringComparison.CurrentCultureIgnoreCase));
            Assert.AreEqual(true, refs["Part6^Assem1-1"].Item2);

            Assert.AreEqual(new Tuple<string, bool>(Path.Combine(destPath, @"SubAssemblies\A\Part3.SLDPRT").ToLower(), true), refs["Assem2-1/Part3-1"]);
            Assert.AreEqual(new Tuple<string, bool>(Path.Combine(destPath, @"Parts\Part1.SLDPRT").ToLower(), true), refs["Assem2-1/Part1-1"]);
            Assert.AreEqual(new Tuple<string, bool>(Path.Combine(workFolder, @"Assembly11\Parts\Part4.SLDPRT").ToLower(), false), refs["Assem2-1/Part4-1"]);

            Assert.AreEqual(new Tuple<string, bool>(Path.Combine(destPath, @"SubAssemblies\A\Assem2.SLDASM").ToLower(), true), refs["Assem3-1/Assem2-1"]);
            Assert.AreEqual(new Tuple<string, bool>(Path.Combine(workFolder, @"Assembly11\SubAssemblies\Part2.SLDPRT").ToLower(), true), refs["Assem3-1/Part2-1"]);
            Assert.AreEqual(new Tuple<string, bool>(Path.Combine(destPath, @"SubAssemblies\Part5.SLDPRT").ToLower(), true), refs["Assem3-1/Part5-1"]);
            Assert.AreEqual(new Tuple<string, bool>(Path.Combine(destPath, @"SubAssemblies\A\Part3.SLDPRT").ToLower(), true), refs["Assem3-1/Assem2-1/Part3-1"]);
            Assert.AreEqual(new Tuple<string, bool>(Path.Combine(destPath, @"Parts\Part1.SLDPRT").ToLower(), true), refs["Assem3-1/Assem2-1/Part1-1"]);
            Assert.AreEqual(new Tuple<string, bool>(Path.Combine(workFolder, @"Assembly11\Parts\Part4.SLDPRT").ToLower(), false), refs["Assem3-1/Assem2-1/Part4-1"]);
        }

        [Test]
        public void ChangedReferencesTest()
        {
            int count;
            string refPath;
            bool isCommitted;
            string workDir;

            using (var doc = OpenDataDocument(@"Assembly12\Assem1.SLDASM"))
            {
                workDir = doc.WorkFolderPath;

                var assm = (ISwDmAssembly)doc.Document;

                var refDocs = assm.Configurations.Active.Components.Select(c => c.ReferencedDocument).ToArray();
                isCommitted = refDocs[0].IsCommitted;
                count = refDocs.Length;
                refPath = refDocs[0].Path;
            }

            Assert.AreEqual(1, count);
            Assert.IsTrue(isCommitted);
            Assert.That(string.Equals(refPath, Path.Combine(workDir, @"Assembly12\_Part1.sldprt"), StringComparison.CurrentCultureIgnoreCase));
        }

        [Test]
        public void SuppressedPatternsTest()
        {
            string[] compNames;
            bool[] suppStates;

            using (var doc = OpenDataDocument(@"SuppressedCompsPattern1.SLDASM"))
            {
                var assm = (ISwDmAssembly)doc.Document;

                var comps = assm.Configurations.Active.Components.TryFlatten().ToArray();

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
                var assm = (ISwDmAssembly)doc.Document;

                var compsDef = assm.Configurations["Default"].Components.TryFlatten().ToArray();
                var compsConf1 = assm.Configurations["Conf1"].Components.TryFlatten().ToArray();

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

            using (var doc = OpenDataDocument(@"Assembly15\Assem15.SLDASM"))
            {
                var assm = (ISwDmAssembly)doc.Document;

                r1 = assm.Configurations.Active.Components["Part1-1"].Reference;
                r2 = assm.Configurations.Active.Components["Part1-2"].Reference;
                r3 = assm.Configurations.Active.Components["Assem2^Assem15-1"].Reference;
                r4 = assm.Configurations.Active.Components["Assem2^Assem15-1"].Children["Part2^Assem2_Assem15-1"].Reference;
            }

            Assert.AreEqual("A", r1);
            Assert.AreEqual("B", r2);
            Assert.AreEqual("C", r3);
            Assert.AreEqual("D", r4);
        }
    }
}
