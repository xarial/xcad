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
using Xarial.XCad.SolidWorks.Documents;

namespace SolidWorks.Tests.Integration
{
    public class DimensionsTest : IntegrationTests
    {
        [Test]
        public void IterateDocumentDimensionsTest() 
        {
            string[] dimNames;

            using (var doc = OpenDataDocument(@"Dimensions1.sldprt"))
            {
                dimNames = m_App.Documents.Active.Dimensions.Select(c => c.Name).ToArray();
            }

            Assert.That(dimNames.OrderBy(c => c).SequenceEqual(
                new string[] { "D1@Sketch1", "D2@Sketch1", "D1@Sketch2", "MyDim@Sketch1", "D1@Boss-Extrude1" }.OrderBy(c => c)));
        }

        [Test]
        public void IterateFeatureDimensionsTest()
        {
            string[] dimNames;

            using (var doc = OpenDataDocument(@"Dimensions1.sldprt"))
            {
                dimNames = m_App.Documents.Active.Features["Sketch1"].Dimensions.Select(c => c.Name).ToArray();
            }

            Assert.That(dimNames.OrderBy(c => c).SequenceEqual(
                new string[] { "D1@Sketch1", "D2@Sketch1", "MyDim@Sketch1" }.OrderBy(c => c)));
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

            using (var doc = OpenDataDocument(@"Dimensions2.sldprt"))
            {
                swDim = (IDimension)(m_App.Documents.Active as ISwDocument).Model.Parameter("D1@Sketch1");

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

            using (var doc = OpenDataDocument(@"Dimensions2.sldprt"))
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
    }
}