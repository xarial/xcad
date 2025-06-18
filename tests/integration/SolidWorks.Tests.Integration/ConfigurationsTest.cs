using NUnit.Framework;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XCad.Data;
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
                name = (doc.Document as ISwDocument3D).Configurations.Active.Name;
            }

            Assert.AreEqual("Conf3", name);
        }

        [Test]
        public void ActivateConfTest()
        {
            string name;

            using (var doc = OpenDataDocument("Configs1.SLDPRT"))
            {
                (doc.Document as ISwDocument3D).Configurations.Active 
                    = (ISwConfiguration)(doc.Document as ISwDocument3D).Configurations["Conf1"];

                name = doc.Document.Model.ConfigurationManager.ActiveConfiguration.Name;
            }

            Assert.AreEqual("Conf1", name);
        }

        [Test]
        public void ActivateConfEventTest()
        {
            string name = "";

            using (var doc = OpenDataDocument("Configs1.SLDPRT"))
            {
                (doc.Document as ISwDocument3D).Configurations.ConfigurationActivated
                    += (IXDocument3D d, IXConfiguration newConf) => 
                    {
                        name += newConf.Name;
                    };

                doc.Document.Model.ShowConfiguration2("Conf1");
            }

            Assert.AreEqual("Conf1", name);
        }

        [Test]
        public void CreateConfigurationTest() 
        {
            IConfiguration conf1;
            IConfiguration conf2;

            using (var doc = NewDataDocument(swDocumentTypes_e.swDocPART)) 
            {
                var part = doc.Document as ISwDocument3D;
                var newConf = part.Configurations.PreCreate();
                newConf.Name = "New Conf1";
                newConf.Commit();

                conf1 = doc.Document.Model.IGetConfigurationByName("New Conf1");
                conf2 = newConf.Configuration;
            }

            Assert.AreEqual(conf1, conf2);
        }

        [Test]
        public void CreateConfigurationCahedDimensionsTest()
        {
            double v1;
            double v2;
            double v3;

            double v1_1;
            double v2_1;
            double v3_1;

            double v1_2;
            double v2_2;
            double v3_2;

            using (var doc = OpenDataDocument("Part2.sldprt"))
            {
                var part = doc.Document as ISwDocument3D;
                var newConf = part.Configurations.PreCreate();

                newConf.Dimensions["D1@Sketch1"].Value = 0.123;

                foreach (var dim in newConf.Dimensions) 
                {
                    if (string.Equals(dim.Name, "D2@Sketch1")) 
                    {
                        dim.Value = 0.234;
                    }
                }

                newConf.Name = "New Conf1";
                newConf.Commit();

                part.Model.EditRebuild3();

                v1 = (((IDimension)part.Model.Parameter("D1@Sketch1")).GetSystemValue3((int)swInConfigurationOpts_e.swSpecifyConfiguration, "New Conf1") as double[])[0];
                v2 = (((IDimension)part.Model.Parameter("D2@Sketch1")).GetSystemValue3((int)swInConfigurationOpts_e.swSpecifyConfiguration, "New Conf1") as double[])[0];
                v3 = (((IDimension)part.Model.Parameter("D1@Sketch2")).GetSystemValue3((int)swInConfigurationOpts_e.swSpecifyConfiguration, "New Conf1") as double[])[0];

                v1_1 = (((IDimension)part.Model.Parameter("D1@Sketch1")).GetSystemValue3((int)swInConfigurationOpts_e.swSpecifyConfiguration, "Default") as double[])[0];
                v2_1 = (((IDimension)part.Model.Parameter("D2@Sketch1")).GetSystemValue3((int)swInConfigurationOpts_e.swSpecifyConfiguration, "Default") as double[])[0];
                v3_1 = (((IDimension)part.Model.Parameter("D1@Sketch2")).GetSystemValue3((int)swInConfigurationOpts_e.swSpecifyConfiguration, "Default") as double[])[0];

                v1_2 = (((IDimension)part.Model.Parameter("D1@Sketch1")).GetSystemValue3((int)swInConfigurationOpts_e.swSpecifyConfiguration, "Conf1") as double[])[0];
                v2_2 = (((IDimension)part.Model.Parameter("D2@Sketch1")).GetSystemValue3((int)swInConfigurationOpts_e.swSpecifyConfiguration, "Conf1") as double[])[0];
                v3_2 = (((IDimension)part.Model.Parameter("D1@Sketch2")).GetSystemValue3((int)swInConfigurationOpts_e.swSpecifyConfiguration, "Conf1") as double[])[0];
            }

            Assert.That(v1, Is.EqualTo(0.123).Within(0.00001).Percent);
            Assert.That(v2, Is.EqualTo(0.234).Within(0.00001).Percent);
            Assert.That(v3, Is.EqualTo(0.04).Within(0.00001).Percent);

            Assert.That(v1_1, Is.EqualTo(0.06).Within(0.00001).Percent);
            Assert.That(v2_1, Is.EqualTo(0.08).Within(0.00001).Percent);
            Assert.That(v3_1, Is.EqualTo(0.04).Within(0.00001).Percent);

            Assert.That(v1_2, Is.EqualTo(0.065).Within(0.00001).Percent);
            Assert.That(v2_2, Is.EqualTo(0.08).Within(0.00001).Percent);
            Assert.That(v3_2, Is.EqualTo(0.045).Within(0.00001).Percent);
        }

        [Test]
        public void CreateConfigurationCahedPropertiesTest()
        {
            var prps = new Dictionary<string, string>();

            using (var doc = OpenDataDocument("Part2.sldprt"))
            {
                var part = doc.Document as ISwDocument3D;
                var newConf = part.Configurations.PreCreate();

                var prp1 = newConf.Properties.GetOrPreCreate("Prp1");
                var prp2 = newConf.Properties.PreCreate();
                prp2.Name = "Prp2";
                var prp3 = newConf.Properties.GetOrPreCreate("Prp3");
                var prp4 = newConf.Properties.GetOrPreCreate("Prp4");

                prp1.Value = "A";
                prp2.Value = "B";
                prp3.Value = "C";
                prp4.Value = "D";

                newConf.Properties.AddRange(new IXProperty[] { prp1, prp2, prp3, prp4 });
                
                newConf.Name = "New Conf1";
                newConf.Commit();

                object prpNames = null;
                object prpTypes = null;
                object prpVals = null;
                object prpRes = null;
                object prpLink = null;
                part.Model.Extension.CustomPropertyManager["New Conf1"].GetAll3(ref prpNames, ref prpTypes, ref prpVals, ref prpRes, ref prpLink);

                for (int i = 0; i < ((string[])prpNames).Length; i++) 
                {
                    prps.Add(((string[])prpNames)[i], ((string[])prpVals)[i]);
                }
            }

            Assert.AreEqual(4, prps.Count);
            Assert.AreEqual("A", prps["Prp1"]);
            Assert.AreEqual("B", prps["Prp2"]);
            Assert.AreEqual("C", prps["Prp3"]);
            Assert.AreEqual("D", prps["Prp4"]);
        }

        [Test]
        public void DeleteConfsTest()
        {
            int count;
            string name;

            using (var doc = OpenDataDocument("Configs1.SLDPRT"))
            {
                var confsToDelete
                    = (doc.Document as ISwDocument3D).Configurations
                    .Where(c => c.Name != "Conf2" && c.Name != "SubSubConf1").ToArray();

                (doc.Document as ISwDocument3D).Configurations.RemoveRange(confsToDelete);

                count = doc.Document.Model.GetConfigurationCount();
                name = doc.Document.Model.ConfigurationManager.ActiveConfiguration.Name;
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
                confNames = (doc.Document as ISwDocument3D).Configurations.Select(x => x.Name).ToArray();
            }

            Assert.That(confNames.SequenceEqual(new string[] 
            {
                "Conf1", "Conf2", "SubConf1", "SubSubConf1", "SubConf2", "Conf3"
            }));
        }

        [Test]
        public void IterateConfsUnloadedTest()
        {
            string[] confNames;

            using (var dataFile = GetDataFile("Configs1.SLDPRT"))
            {
                var part = Application.Documents.PreCreate<ISwPart>();
                part.Path = dataFile.FilePath;
                confNames = part.Configurations.Select(x => x.Name).ToArray();
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
                var confs = (doc.Document as ISwDocument3D).Configurations;

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

            using (var doc = OpenDataDocument(@"LdrAssembly1\TopAssem.SLDASM", DocumentState_e.ViewOnly))
            {
                var confs = (doc.Document as ISwDocument3D).Configurations;
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
                var confs = (doc.Document as ISwDocument3D).Configurations;

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
                var confs = (doc.Document as IXDocument3D).Configurations;
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
                var assm = (ISwAssembly)doc.Document;

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

        [Test]
        public void ParentConfTest()
        {
            string c1, c2, c3, c4, c5, c6;

            using (var doc = OpenDataDocument(@"Assembly16\Part1.SLDPRT"))
            {
                var part = doc.Document as ISwDocument3D;

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
                var assm = doc.Document as ISwAssembly;
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

        [Test]
        public void GetMaterialTest()
        {
            string matName1;
            string dbName1;

            string matName2;
            string dbName2;

            IXMaterial mat3;

            using (var doc = OpenDataDocument(@"Material1.SLDPRT"))
            {
                var part = (IXPart)doc.Document;
                
                var mat1 = ((IXPartConfiguration)part.Configurations["Default"]).Material;
                matName1 = mat1.Name;
                dbName1 = mat1.Database.Name;

                var mat2 = ((IXPartConfiguration)part.Configurations["Conf1"]).Material;
                matName2 = mat2.Name;
                dbName2 = mat2.Database.Name;

                mat3 = ((IXPartConfiguration)part.Configurations["Conf2"]).Material;
            }

            Assert.AreEqual("1060 Alloy", matName1);
            Assert.AreEqual("", dbName1);

            Assert.AreEqual("Brass", matName2);
            Assert.AreEqual("", dbName2);

            Assert.IsNull(mat3);
        }

        [Test]
        public void GetMaterialBodyTest()
        {
            string matName1;
            string dbName1;

            string matName2;
            string dbName2;

            IXMaterial mat3;

            using (var doc = OpenDataDocument(@"Material2.SLDPRT"))
            {
                var part = (IXPart)doc.Document;

                part.Configurations.Active = (IXPartConfiguration)part.Configurations["Default"];
                var mat1 = part.Bodies["Boss-Extrude1"].Material;
                matName1 = mat1.Name;
                dbName1 = mat1.Database.Name;

                part.Configurations.Active = (IXPartConfiguration)part.Configurations["Conf1"];
                var mat2 = part.Bodies["Boss-Extrude1"].Material;
                matName2 = mat2.Name;
                dbName2 = mat2.Database.Name;

                mat3 = part.Bodies["Boss-Extrude2"].Material;
            }

            Assert.AreEqual("1060 Alloy", matName1);
            Assert.AreEqual("", dbName1);

            Assert.AreEqual("Brass", matName2);
            Assert.AreEqual("", dbName2);

            Assert.IsNull(mat3);
        }

        [Test]
        public void SetBodyMaterialTest()
        {
            string mat1;
            string db1;

            string mat2;
            string db2;

            string mat3;
            string db3;

            string mat4;
            string db4;

            using (var doc = OpenDataDocument(@"Material2.SLDPRT", DocumentState_e.Default))
            {
                var part = (ISwPart)doc.Document;

                var mat = Application.MaterialDatabases[""]["ABS PC"];

                part.Configurations.Active = part.Configurations["Default"];
                part.Bodies["Boss-Extrude1"].Material = mat;

                mat1 = ((ISwBody)part.Bodies["Boss-Extrude1"]).Body.GetMaterialPropertyName("Default", out db1);
                
                part.Configurations.Active = part.Configurations["Conf1"];
                mat2 = ((ISwBody)part.Bodies["Boss-Extrude1"]).Body.GetMaterialPropertyName("Conf1", out db2);
                part.Bodies["Boss-Extrude2"].Material = mat;

                mat3 = ((ISwBody)part.Bodies["Boss-Extrude2"]).Body.GetMaterialPropertyName("Conf1", out db3);

                part.Bodies["Boss-Extrude1"].Material = null;
                mat4 = ((ISwBody)part.Bodies["Boss-Extrude1"]).Body.GetMaterialPropertyName("Default", out db4);
            }

            Assert.AreEqual("ABS PC", mat1);
            Assert.AreEqual("SOLIDWORKS Materials", db1);
            Assert.AreEqual("Brass", mat2);
            Assert.AreEqual("SOLIDWORKS Materials", db2);
            Assert.AreEqual("ABS PC", mat3);
            Assert.AreEqual("SOLIDWORKS Materials", db3);
            Assert.AreEqual("", mat4);
            Assert.AreEqual("", db4);
        }

        [Test]
        public void SetMaterialTest()
        {
            string mat1;
            string db1;

            string mat2;
            string db2;

            string mat3;
            string db3;

            string mat4;
            string db4;

            string mat5;
            string db5;

            using (var doc = OpenDataDocument(@"Material1.SLDPRT"))
            {
                var part = (ISwPart)doc.Document;

                var mat = Application.MaterialDatabases[""]["ABS PC"];

                part.Configurations["Default"].Material = mat;

                mat1 = part.Part.GetMaterialPropertyName2("Default", out db1);
                mat2 = part.Part.GetMaterialPropertyName2("Conf1", out db2);

                part.Configurations["Conf1"].Material = mat;
                mat3 = part.Part.GetMaterialPropertyName2("Conf1", out db3);

                Assert.Throws<ConfigurationMaterialRemoveException>(() => part.Configurations["Default"].Material = null);

                part.Material = null;

                mat4 = part.Part.GetMaterialPropertyName2("Default", out db4);
                mat5 = part.Part.GetMaterialPropertyName2("Conf1", out db5);
            }

            Assert.AreEqual("ABS PC", mat1);
            Assert.AreEqual("SOLIDWORKS Materials", db1);
            Assert.AreEqual("Brass", mat2);
            Assert.AreEqual("SOLIDWORKS Materials", db2);
            Assert.AreEqual("ABS PC", mat3);
            Assert.AreEqual("SOLIDWORKS Materials", db3);
            Assert.AreEqual("", mat4);
            Assert.AreEqual("", db4);
            Assert.AreEqual("", mat5);
            Assert.AreEqual("", db5);
        }
    }
}
