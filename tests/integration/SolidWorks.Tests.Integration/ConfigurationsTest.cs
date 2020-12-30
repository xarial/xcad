using NUnit.Framework;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Exceptions;
using Xarial.XCad.SolidWorks.Documents;

namespace SolidWorks.Tests.Integration
{
    public class ConfigurationsTest : IntegrationTests
    {
        [Test]
        public void ActiveConfTest() 
        {
            string name;

            using (var doc = OpenDataDocument("Configs1.SLDPRT"))
            {
                name = (m_App.Documents.Active as ISwDocument3D).Configurations.Active.Name;
            }

            Assert.AreEqual("Conf3", name);
        }

        [Test]
        public void ActivateConfTest()
        {
            string name;

            using (var doc = OpenDataDocument("Configs1.SLDPRT"))
            {
                (m_App.Documents.Active as ISwDocument3D).Configurations.Active 
                    = (ISwConfiguration)(m_App.Documents.Active as ISwDocument3D).Configurations["Conf1"];

                name = m_App.Documents.Active.Model.ConfigurationManager.ActiveConfiguration.Name;
            }

            Assert.AreEqual("Conf1", name);
        }

        [Test]
        public void ActivateConfEventTest()
        {
            string name = "";

            using (var doc = OpenDataDocument("Configs1.SLDPRT"))
            {
                (m_App.Documents.Active as ISwDocument3D).Configurations.ConfigurationActivated
                    += (IXDocument3D d, IXConfiguration newConf) => 
                    {
                        name += newConf.Name;
                    };

                m_App.Documents.Active.Model.ShowConfiguration2("Conf1");
            }

            Assert.AreEqual("Conf1", name);
        }

        [Test]
        public void CreateConfigurationTest() 
        {
            IConfiguration conf1;
            IConfiguration conf2;

            using (var doc = NewDocument(Interop.swconst.swDocumentTypes_e.swDocPART)) 
            {
                var part = m_App.Documents.Active as ISwDocument3D;
                var newConf = part.Configurations.PreCreate();
                newConf.Name = "New Conf1";
                newConf.Commit();

                conf1 = m_App.Documents.Active.Model.IGetConfigurationByName("New Conf1");
                conf2 = newConf.Configuration;
            }

            Assert.AreEqual(conf1, conf2);
        }

        [Test]
        public void DeleteConfsTest()
        {
            int count;
            string name;

            using (var doc = OpenDataDocument("Configs1.SLDPRT"))
            {
                var confsToDelete
                    = (m_App.Documents.Active as ISwDocument3D).Configurations
                    .Where(c => c.Name != "Conf2" && c.Name != "SubSubConf1").ToArray();

                (m_App.Documents.Active as ISwDocument3D).Configurations.RemoveRange(confsToDelete);

                count = m_App.Documents.Active.Model.GetConfigurationCount();
                name = m_App.Documents.Active.Model.ConfigurationManager.ActiveConfiguration.Name;
            }

            Assert.AreEqual(1, count);
            Assert.AreEqual("Conf2", name);
        }

        [Test]
        public void IterateConfsTest()
        {
            string[] confNames;

            using (var doc = OpenDataDocument("Configs1.SLDPRT"))
            {
                confNames = (m_App.Documents.Active as ISwDocument3D).Configurations.Select(x => x.Name).ToArray();
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
                var confs = (m_App.Documents.Active as ISwDocument3D).Configurations;

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
    }
}
