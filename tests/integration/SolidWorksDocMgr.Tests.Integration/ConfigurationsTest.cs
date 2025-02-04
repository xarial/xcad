using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Exceptions;
using Xarial.XCad.SwDocumentManager.Documents;

namespace SolidWorksDocMgr.Tests.Integration
{
    public class ConfigurationsTest : IntegrationTests
    {
        [Test]
        public void ActiveConfTest()
        {
            string name;

            using (var doc = OpenDataDocument("Configs1.SLDPRT"))
            {
                name = (m_App.Documents.Active as ISwDmDocument3D).Configurations.Active.Name;
            }

            Assert.AreEqual("Conf3", name);
        }

        [Test]
        public void IterateConfsTest()
        {
            string[] confNames;

            using (var doc = OpenDataDocument("Configs1.SLDPRT"))
            {
                confNames = (m_App.Documents.Active as ISwDmDocument3D).Configurations.Select(x => x.Name).ToArray();
            }

            Assert.That(confNames.SequenceEqual(new string[]
            {
                "Conf1", "Conf2", "SubConf1", "SubSubConf1", "SubConf2", "Conf3"
            }));
        }

        [Test]
        public void GetConfigByNameTest()
        {
            IXConfiguration conf1;
            IXConfiguration conf2;
            IXConfiguration conf3;
            bool r1;
            bool r2;
            Exception e1 = null;

            using (var doc = OpenDataDocument("Configs1.SLDPRT"))
            {
                var confs = (m_App.Documents.Active as ISwDmDocument3D).Configurations;

                conf1 = confs["Conf1"];
                r1 = confs.TryGet("Conf2", out conf2);
                r2 = confs.TryGet("Conf4", out conf3);

                try
                {
                    var conf4 = confs["Conf5"];
                }
                catch (EntityNotFoundException ex)
                {
                    e1 = ex;
                }
            }

            Assert.IsNotNull(conf1);
            Assert.IsNotNull(conf2);
            Assert.IsNull(conf3);
            Assert.IsTrue(r1);
            Assert.IsFalse(r2);
            Assert.IsNotNull(e1);
        }

        [Test]
        public void DeleteConfsTest()
        {
            int count;
            string name;

            using (var doc = OpenDataDocument("Configs1.SLDPRT"))
            {
                var confsToDelete
                    = (m_App.Documents.Active as ISwDmDocument3D).Configurations
                    .Where(c => c.Name != "Conf3").ToArray();

                (m_App.Documents.Active as ISwDmDocument3D).Configurations.RemoveRange(confsToDelete);

                count = m_App.Documents.Active.Document.ConfigurationManager.GetConfigurationCount();
                name = m_App.Documents.Active.Document.ConfigurationManager.GetActiveConfigurationName();
            }

            Assert.AreEqual(1, count);
            Assert.AreEqual("Conf3", name);
        }

        [Test]
        public void PartNumberTest()
        {
            string v1;
            string v2;
            string v3;
            string v4;

            PartNumberSourceType_e s1;
            PartNumberSourceType_e s2;
            PartNumberSourceType_e s3;
            PartNumberSourceType_e s4;

            using (var doc = OpenDataDocument("PartNumber1.SLDPRT"))
            {
                var confs = (m_App.Documents.Active as ISwDmDocument3D).Configurations;

                var p1 = confs["Default"].PartNumber;
                var p2 = confs["Conf1"].PartNumber;
                var p3 = confs["Conf4"].PartNumber;
                var p4 = confs["Conf5"].PartNumber;

                v1 = p1.Value;
                v2 = p2.Value;
                v3 = p3.Value;
                v4 = p4.Value;

                s1 = p1.Type;
                s2 = p2.Type;
                s3 = p3.Type;
                s4 = p4.Type;
            }

            Assert.AreEqual("PartNumber1", v1);
            Assert.AreEqual("Conf1", v2);
            Assert.AreEqual("Conf3", v3);
            Assert.AreEqual("ABC", v4);

            Assert.AreEqual(PartNumberSourceType_e.DocumentName, s1);
            Assert.AreEqual(PartNumberSourceType_e.ConfigurationName, s2);
            Assert.AreEqual(PartNumberSourceType_e.ParentName, s3);
            Assert.AreEqual(PartNumberSourceType_e.Custom, s4);
        }

        [Test]
        public void BomChildrenDisplayTest()
        {
            BomChildrenSolving_e s1;
            BomChildrenSolving_e s2;
            BomChildrenSolving_e s3;

            using (var doc = OpenDataDocument("BomChildrenDisplay.SLDASM"))
            {
                var confs = (m_App.Documents.Active as ISwDmDocument3D).Configurations;
                s1 = confs["Conf1"].BomChildrenSolving;
                s2 = confs["Conf2"].BomChildrenSolving;
                s3 = confs["Conf3"].BomChildrenSolving;
            }

            Assert.AreEqual(BomChildrenSolving_e.Show, s1);
            Assert.AreEqual(BomChildrenSolving_e.Hide, s2);
            Assert.AreEqual(BomChildrenSolving_e.Promote, s3);
        }

        [Test]
        public void ParentConfTest()
        {
            string c1, c2, c3, c4, c5, c6;

            using (var doc = OpenDataDocument(@"Assembly16\Part1.SLDPRT"))
            {
                var part = m_App.Documents.Active as ISwDmDocument3D;

                c1 = part.Configurations["SubConfA"].Parent?.Name;
                c2 = part.Configurations["SubConfA"].Parent.Parent?.Name;

                c3 = part.Configurations["ConfB"].Parent?.Name;

                c4 = part.Configurations["SubSubConf1"].Parent?.Name;
                c5 = part.Configurations["SubConf1"].Parent?.Name;
                c6 = part.Configurations["SubConf1"].Parent.Parent?.Name;
            }

            Assert.AreEqual("ConfA", c1);
            Assert.AreEqual(null, c2);
            Assert.AreEqual(null, c3);
            Assert.AreEqual("SubConf1", c4);
            Assert.AreEqual("Default", c5);
            Assert.AreEqual(null, c6);
        }

        [Test]
        public void ParentConfComponentTest()
        {
            IXConfiguration conf1, conf2, conf3, conf4, conf5;

            string c1, c2, c3, c4, c5, c6, c7;

            using (var doc = OpenDataDocument(@"Assembly16\Assem1.SLDASM"))
            {
                var assm = m_App.Documents.Active as ISwDmAssembly;
                var comp1 = assm.Configurations.Active.Components["Part1-1"];
                var comp2 = assm.Configurations.Active.Components["Part1-2"];
                var comp3 = assm.Configurations.Active.Components["SubAssem1-1"];
                var comp4 = assm.Configurations.Active.Components["SubAssem1-2"];

                conf1 = comp1.ReferencedConfiguration.Parent;
                c1 = conf1?.Name;

                conf2 = conf1.Parent;
                c2 = conf2?.Name;

                conf3 = conf2.Parent;
                c3 = conf3?.Name;

                c4 = comp2.ReferencedConfiguration.Parent?.Name;

                c5 = comp3.ReferencedConfiguration.Parent?.Name;

                conf4 = comp4.ReferencedConfiguration.Parent;
                c6 = conf4?.Name;

                conf5 = conf4.Parent;
                c7 = conf5?.Name;
            }

            Assert.AreEqual("SubConf1", c1);
            Assert.AreEqual("Default", c2);
            Assert.AreEqual(null, c3);
            Assert.IsInstanceOf<IXPartConfiguration>(conf1);
            Assert.IsInstanceOf<IXPartConfiguration>(conf2);
            Assert.IsNull(conf3);
            Assert.AreEqual(null, c4);
            Assert.AreEqual(null, c5);
            Assert.AreEqual("Default", c6);
            Assert.IsInstanceOf<IXAssemblyConfiguration>(conf4);
            Assert.IsNull(conf5);
            Assert.AreEqual(null, c7);
        }
    }
}
