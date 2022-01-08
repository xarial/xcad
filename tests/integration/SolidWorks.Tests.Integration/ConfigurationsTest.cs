using NUnit.Framework;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Geometry;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Documents.Exceptions;
using Xarial.XCad.SolidWorks.Geometry;

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
        public void IterateConfsUnloadedTest()
        {
            var part = m_App.Documents.PreCreate<IXPart>();
            part.Path = GetFilePath("Configs1.SLDPRT");
            var confNames = part.Configurations.Select(x => x.Name).ToArray();

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

        [Test]
        public void LdrConfsTest()
        {
            string[] confNames;
            bool r1;
            bool r2;
            bool r3;
            bool[] r;

            using (var doc = OpenDataDocument(@"LdrAssembly1\TopAssem.SLDASM", true, s => { s.ViewOnly = true; }))
            {
                var confs = (m_App.Documents.Active as ISwDocument3D).Configurations;
                confNames = confs.Select(x => x.Name).ToArray();
                r = confs.Select(x => x.IsCommitted).ToArray();
                r1 = confs["Default"].IsCommitted;
                r2 = confs["Conf1"].IsCommitted;
                r3 = confs["Conf2"].IsCommitted;
                Assert.Throws<InactiveLdrConfigurationNotSupportedException>(() => { var p1 = confs["Conf1"].Properties; });
                var p2 = confs["Default"].Properties;
                var p3 = confs.First().Properties;
            }

            Assert.That(confNames.SequenceEqual(new string[]
            {
                "Default", "Conf1", "Conf2"
            }));

            Assert.That(r.SequenceEqual(new bool[]
            {
                true, false, false
            }));

            Assert.IsTrue(r1);
            Assert.IsFalse(r2);
            Assert.IsFalse(r3);
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
                var confs = (m_App.Documents.Active as ISwDocument3D).Configurations;
                p1 = confs["Default"].PartNumber;
                p2 = confs["Conf1"].PartNumber;
                p3 = confs["Conf4"].PartNumber;
                p4 = confs["Conf5"].PartNumber;
            }

            Assert.AreEqual("PartNumber1", p1);
            Assert.AreEqual("Conf1", p2);
            Assert.AreEqual("Conf3", p3);
            Assert.AreEqual("ABC", p4);
        }

        [Test]
        public void BomChildrenDisplayTest()
        {
            BomChildrenSolving_e s1;
            BomChildrenSolving_e s2;
            BomChildrenSolving_e s3;

            using (var doc = OpenDataDocument("BomChildrenDisplay.SLDASM"))
            {
                var confs = (m_App.Documents.Active as IXDocument3D).Configurations;
                s1 = confs["Conf1"].BomChildrenSolving;
                s2 = confs["Conf2"].BomChildrenSolving;
                s3 = confs["Conf3"].BomChildrenSolving;
            }

            Assert.AreEqual(BomChildrenSolving_e.Show, s1);
            Assert.AreEqual(BomChildrenSolving_e.Hide, s2);
            Assert.AreEqual(BomChildrenSolving_e.Promote, s3);
        }

        [Test]
        public void FaceColorTest()
        {
            System.Drawing.Color? c1;
            System.Drawing.Color? c2;

            double[] mat1;
            double[] mat2;

            using (var doc = OpenDataDocument(@"ColorAssembly\Assem1.SLDASM"))
            {
                var assm = (ISwAssembly)m_App.Documents.Active;

                var comp1Face = assm.Configurations.Active.Components["Part1-1"].Bodies.First().Faces.OfType<ISwCylindricalFace>().First();
                var comp2Face = assm.Configurations.Active.Components["Part1-2"].Bodies.First().Faces.OfType<ISwCylindricalFace>().First();

                c1 = comp1Face.Color;
                c2 = comp2Face.Color;

                comp1Face.Color = System.Drawing.Color.FromArgb(100, 50, 150, 250);
                comp2Face.Color = null;

                mat1 = (double[])(comp1Face.Face.GetMaterialPropertyValues2((int)swInConfigurationOpts_e.swThisConfiguration, null));
                mat2 = (double[])(comp2Face.Face.GetMaterialPropertyValues2((int)swInConfigurationOpts_e.swThisConfiguration, null));
            }

            Assert.IsNotNull(c1);
            Assert.AreEqual(255, c1.Value.A);
            Assert.AreEqual(255, c1.Value.R);
            Assert.AreEqual(0, c1.Value.G);
            Assert.AreEqual(0, c1.Value.B);

            Assert.IsNotNull(c2);
            Assert.AreEqual(255, c2.Value.A);
            Assert.AreEqual(0, c2.Value.R);
            Assert.AreEqual(255, c2.Value.G);
            Assert.AreEqual(0, c2.Value.B);

            Assert.AreEqual(50, (int)(mat1[0] * 255));
            Assert.AreEqual(150, (int)(mat1[1] * 255));
            Assert.AreEqual(250, (int)(mat1[2] * 255));
            Assert.AreEqual(100, (int)((1f - mat1[7]) * 255));

            Assert.IsTrue(mat2.All(m => m == -1));
        }
    }
}
