using NUnit.Framework;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Annotations;
using Xarial.XCad.Exceptions;
using Xarial.XCad.SolidWorks.Annotations;
using Xarial.XCad.SolidWorks.Annotations.Exceptions;
using Xarial.XCad.SolidWorks.Documents;

namespace SolidWorks.Tests.Integration
{
    public class DimensionsTest : IntegrationTests
    {
        [Test]
        public void IterateDocumentDimensionsTest() 
        {
            string[] dimNames;

            using (var doc = OpenDataDocument("Dimensions1.sldprt"))
            {
                dimNames = m_App.Documents.Active.Dimensions.Select(c => c.Name).ToArray();
            }

            CollectionAssert.AreEquivalent(dimNames,
                new string[] { "D1@Sketch1", "D2@Sketch1", "D1@Sketch2", "MyDim@Sketch1", "D1@Boss-Extrude1" });
        }

        [Test]
        public void IterateDrawingDimensionsTest()
        {
            string[] dimNames;

            using (var doc = OpenDataDocument(@"Drawing4.slddrw"))
            {
                dimNames = m_App.Documents.Active.Dimensions.Select(c => c.Name).ToArray();
            }

            CollectionAssert.AreEquivalent(dimNames, 
                new string[] { "D1@Sketch1", "D2@Sketch1", "RD1@Drawing View1" });
        }

        [Test]
        public void IterateFeatureDimensionsTest()
        {
            string[] dimNames;

            using (var doc = OpenDataDocument("Dimensions1.sldprt"))
            {
                dimNames = m_App.Documents.Active.Features["Sketch1"].Dimensions.Select(c => c.Name).ToArray();
            }

            CollectionAssert.AreEquivalent(dimNames,
                new string[] { "D1@Sketch1", "D2@Sketch1", "MyDim@Sketch1" });
        }

        [Test]
        public void ValueSetTest() 
        {
            double v1;
            double v2;
            double v2_1;
            double v3;
            double v3_1;

            IDimension swDim;

            using (var doc = OpenDataDocument("Dimensions2.sldprt"))
            {
                swDim = (IDimension)m_App.Documents.Active.Model.Parameter("D1@Sketch1");

                ((ISwDocument3D)m_App.Documents.Active).Configurations.Active.Dimensions["D1@Sketch1"].Value = 0.1d;
                v1 = (swDim.GetSystemValue3((int)swInConfigurationOpts_e.swThisConfiguration, null) as double[])[0];

                ((ISwDocument3D)m_App.Documents.Active).Configurations["Conf2"].Dimensions["D1@Sketch1"].Value = 0.2d;
                v2 = (swDim.GetSystemValue3((int)swInConfigurationOpts_e.swSpecifyConfiguration, new string[] { "Conf2" }) as double[])[0];
                v2_1 = (swDim.GetSystemValue3((int)swInConfigurationOpts_e.swSpecifyConfiguration, new string[] { "Default" }) as double[])[0];

                ((ISwDocument3D)m_App.Documents.Active).Configurations["Default"].Dimensions["D1@Sketch1"].Value = 0.5d;
                v3 = (swDim.GetSystemValue3((int)swInConfigurationOpts_e.swSpecifyConfiguration, new string[] { "Default" }) as double[])[0];
                v3_1 = (swDim.GetSystemValue3((int)swInConfigurationOpts_e.swSpecifyConfiguration, new string[] { "Conf2" }) as double[])[0];

                Assert.Throws<EntityNotFoundException>(() => ((ISwDocument3D)m_App.Documents.Active).Dimensions["D2@Sketch1"].Value = 0.3d);
                Assert.Throws<EntityNotFoundException>(() => ((ISwDocument3D)m_App.Documents.Active).Configurations["Conf2"].Dimensions["D2@Sketch1"].Value = 0.4d);
            }
            
            Assert.That(0.1, Is.EqualTo(v1).Within(0.001).Percent);
            Assert.That(0.2, Is.EqualTo(v2).Within(0.001).Percent);
            Assert.That(0.1, Is.EqualTo(v2_1).Within(0.001).Percent);
            Assert.That(0.5, Is.EqualTo(v3).Within(0.001).Percent);
            Assert.That(0.2, Is.EqualTo(v3_1).Within(0.001).Percent);
        }

        [Test]
        public void ValueGetTest()
        {
            double r1;
            double r2;
            double r3;

            using (var doc = OpenDataDocument("Dimensions2.sldprt"))
            {
                r1 = m_App.Documents.Active.Dimensions["D1@Sketch1"].Value;
                r2 = ((ISwDocument3D)m_App.Documents.Active).Configurations["Conf2"].Dimensions["D1@Sketch1"].Value;
                r3 = ((ISwDocument3D)m_App.Documents.Active).Configurations["Default"].Dimensions["D1@Sketch1"].Value;
                Assert.Throws<EntityNotFoundException>(() => { var r4 = ((ISwDocument3D)m_App.Documents.Active).Configurations["Default"].Dimensions["D2@Sketch1"].Value; });
                Assert.Throws<EntityNotFoundException>(() => { var r5 = ((ISwDocument3D)m_App.Documents.Active).Configurations["Conf2"].Dimensions["D2@Sketch1"].Value; });
            }

            Assert.That(0.125, Is.EqualTo(r1).Within(0.001).Percent);
            Assert.That(0.235, Is.EqualTo(r2).Within(0.001).Percent);
            Assert.That(0.125, Is.EqualTo(r3).Within(0.001).Percent);
        }

        [Test]
        public void ValueGetDrawingTest()
        {
            double r1;
            double r2;
            double r3;

            using (var doc = OpenDataDocument("Drawing4.slddrw"))
            {
                r1 = m_App.Documents.Active.Dimensions["D1@Sketch1"].Value;

                if (m_App.Documents.Active.Model.Extension.SelectByID2("D2@Sketch1@Drawing4.SLDDRW", "DIMENSION", 0, 0, 0, false, 0, null, 0))
                {
                    var dim1 = m_App.Documents.Active.Selections.OfType<IXDimension>().First();
                    r2 = dim1.Value;
                }
                else
                {
                    throw new Exception();
                }

                r3 = m_App.Documents.Active.Dimensions["RD1@Drawing View1"].Value;
            }

            Assert.That(0.05, Is.EqualTo(r1).Within(0.001).Percent);
            Assert.That(0.035, Is.EqualTo(r2).Within(0.001).Percent);
            Assert.That(0.032293575141742692, Is.EqualTo(r3).Within(0.001).Percent);
        }

        [Test]
        public void ValueSetDrawingTest()
        {
            double r1;
            double r2;

            using (var doc = OpenDataDocument("Drawing4.slddrw"))
            {
                m_App.Documents.Active.Dimensions["D1@Sketch1"].Value = 0.123;

                if (m_App.Documents.Active.Model.Extension.SelectByID2("D2@Sketch1@Drawing4.SLDDRW", "DIMENSION", 0, 0, 0, false, 0, null, 0))
                {
                    var dim1 = m_App.Documents.Active.Selections.OfType<IXDimension>().First();
                    dim1.Value = 0.234;
                }
                else
                {
                    throw new Exception();
                }

                Assert.Throws<NotEditableDrivenDimensionException>(() => m_App.Documents.Active.Dimensions["RD1@Drawing View1"].Value = 0.345);
                
                m_App.Documents.Active.Rebuild();

                r1 = m_App.Documents.Active.Model.IParameter("D1@Sketch1").SystemValue;
                r2 = m_App.Documents.Active.Model.IParameter("D2@Sketch1").SystemValue;
            }

            Assert.That(0.123, Is.EqualTo(r1).Within(0.001).Percent);
            Assert.That(0.234, Is.EqualTo(r2).Within(0.001).Percent);
        }

        [Test]
        public void ValueSetComponentTest()
        {
            double v1;
            double v2;
            double v2_1;
            double v3;
            double v3_1;

            IDimension swDim;

            using (var doc = OpenDataDocument("DimensionsAssem2.SLDASM"))
            {
                var assmConf = ((ISwAssembly)m_App.Documents.Active).Configurations.Active;

                swDim = (IDimension)((ISwDocument)assmConf.Components["Dimensions2-1"].ReferencedDocument).Model.Parameter("D1@Sketch1");

                assmConf.Components["Dimensions2-1"].Dimensions["D1@Sketch1"].Value = 0.1d;
                m_App.Documents.Active.Rebuild();
                v1 = (swDim.GetSystemValue3((int)swInConfigurationOpts_e.swThisConfiguration, null) as double[])[0];

                assmConf.Components["Dimensions2-2"].ReferencedConfiguration.Dimensions["D1@Sketch1"].Value = 0.2d;
                m_App.Documents.Active.Rebuild();
                v2 = (swDim.GetSystemValue3((int)swInConfigurationOpts_e.swSpecifyConfiguration, new string[] { "Conf2" }) as double[])[0];
                v2_1 = (swDim.GetSystemValue3((int)swInConfigurationOpts_e.swSpecifyConfiguration, new string[] { "Default" }) as double[])[0];

                assmConf.Components["Dimensions2-1"].ReferencedConfiguration.Dimensions["D1@Sketch1"].Value = 0.5d;
                m_App.Documents.Active.Rebuild();
                v3 = (swDim.GetSystemValue3((int)swInConfigurationOpts_e.swSpecifyConfiguration, new string[] { "Default" }) as double[])[0];
                v3_1 = (swDim.GetSystemValue3((int)swInConfigurationOpts_e.swSpecifyConfiguration, new string[] { "Conf2" }) as double[])[0];

                Assert.Throws<EntityNotFoundException>(() => assmConf.Components["Dimensions2-1"].Dimensions["D2@Sketch1"].Value = 0.3d);
                Assert.Throws<EntityNotFoundException>(() => assmConf.Components["Dimensions2-2"].Dimensions["D2@Sketch1"].Value = 0.4d);
            }

            Assert.That(0.1, Is.EqualTo(v1).Within(0.001).Percent);
            Assert.That(0.2, Is.EqualTo(v2).Within(0.001).Percent);
            Assert.That(0.1, Is.EqualTo(v2_1).Within(0.001).Percent);
            Assert.That(0.5, Is.EqualTo(v3).Within(0.001).Percent);
            Assert.That(0.2, Is.EqualTo(v3_1).Within(0.001).Percent);
        }

        [Test]
        public void ValueGetComponentTest()
        {
            double r1;
            double r2;
            double r3;

            using (var doc = OpenDataDocument("DimensionsAssem2.SLDASM"))
            {
                var assmConf = ((ISwAssembly)m_App.Documents.Active).Configurations.Active;

                r1 = assmConf.Components["Dimensions2-1"].ReferencedConfiguration.Dimensions["D1@Sketch1"].Value;
                r2 = assmConf.Components["Dimensions2-2"].Dimensions["D1@Sketch1"].Value;
                r3 = assmConf.Components["Dimensions2-1"].Dimensions["D1@Sketch1"].Value;
                Assert.Throws<EntityNotFoundException>(() => { var r4 = assmConf.Components["Dimensions2-1"].Dimensions["D2@Sketch1"].Value; });
                Assert.Throws<EntityNotFoundException>(() => { var r5 = assmConf.Components["Dimensions2-2"].Dimensions["D2@Sketch1"].Value; });
            }

            Assert.That(0.125, Is.EqualTo(r1).Within(0.001).Percent);
            Assert.That(0.235, Is.EqualTo(r2).Within(0.001).Percent);
            Assert.That(0.125, Is.EqualTo(r3).Within(0.001).Percent);
        }

        [Test]
        public void ValueGetComponentSelectedDimTest() 
        {
            double r1;
            double r2;

            using (var doc = OpenDataDocument("VirtAssem2.SLDASM"))
            {
                var assm = m_App.Documents.Active;

                if (assm.Model.Extension.SelectByID2("D1@Sketch1@Part2^VirtAssem2-1@VirtAssem2", "DIMENSION", 0, 0, 0, false, 0, null, 0))
                {
                    var dim1 = assm.Selections.OfType<IXDimension>().First();
                    r1 = dim1.Value;
                }
                else 
                {
                    throw new Exception();
                }

                if (assm.Model.Extension.SelectByID2("D1@Sketch1@Part2^VirtAssem2-2@VirtAssem2", "DIMENSION", 0, 0, 0, false, 0, null, 0))
                {
                    var dim2 = assm.Selections.OfType<IXDimension>().First();

                    r2 = dim2.Value;
                }
                else
                {
                    throw new Exception();
                }
            }

            Assert.That(0.01, Is.EqualTo(r1).Within(0.001).Percent);
            Assert.That(0.02, Is.EqualTo(r2).Within(0.001).Percent);
        }

        [Test]
        public void ValueSetComponentSelectedDimTest()
        {
            double r1;
            double r2;

            using (var doc = OpenDataDocument("VirtAssem2.SLDASM"))
            {
                var assm = m_App.Documents.Active;

                IXDimension dim1;
                IXDimension dim2;

                if (assm.Model.Extension.SelectByID2("D1@Sketch1@Part2^VirtAssem2-1@VirtAssem2", "DIMENSION", 0, 0, 0, false, 0, null, 0))
                {
                    dim1 = assm.Selections.OfType<IXDimension>().First();
                    dim1.Value = 0.123;
                }
                else
                {
                    throw new Exception();
                }

                if (assm.Model.Extension.SelectByID2("D1@Sketch1@Part2^VirtAssem2-2@VirtAssem2", "DIMENSION", 0, 0, 0, false, 0, null, 0))
                {
                    dim2 = assm.Selections.OfType<IXDimension>().First();
                    dim2.Value = 0.234;
                }
                else
                {
                    throw new Exception();
                }

                m_App.Documents.Active.Rebuild();
                r1 = (((ISwDimension)dim1).Dimension.GetSystemValue3((int)swInConfigurationOpts_e.swSpecifyConfiguration, new string[] { "Default" }) as double[])[0];
                r2 = (((ISwDimension)dim2).Dimension.GetSystemValue3((int)swInConfigurationOpts_e.swSpecifyConfiguration, new string[] { "Conf1" }) as double[])[0];
            }

            Assert.That(0.123, Is.EqualTo(r1).Within(0.001).Percent);
            Assert.That(0.234, Is.EqualTo(r2).Within(0.001).Percent);
        }
    }
}