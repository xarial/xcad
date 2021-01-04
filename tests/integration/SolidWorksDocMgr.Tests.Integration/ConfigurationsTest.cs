using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents;
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
            string p1;
            string p2;
            string p3;
            string p4;

            using (var doc = OpenDataDocument("PartNumber1.SLDPRT"))
            {
                var confs = (m_App.Documents.Active as ISwDmDocument3D).Configurations;
                p1 = confs["Default"].PartNumber;
                p2 = confs["Conf1"].PartNumber;
                p3 = confs["Conf4"].PartNumber;
                p4 = confs["Conf5"].PartNumber;
            }

            Assert.AreEqual("PartNumber1", System.IO.Path.GetFileNameWithoutExtension(p1));
            Assert.AreEqual("Conf1", p2);
            Assert.AreEqual("Conf3", p3);
            Assert.AreEqual("ABC", p4);
        }
    }
}
