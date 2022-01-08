using NUnit.Framework;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Annotations;
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
        public void TrySetDimensionValueTest() 
        {
            bool r1;
            bool r2;
            bool r3;
            bool r4;
            bool r5;

            double v1;
            double v2;
            double v3;

            IDimension swDim;

            using (var doc = OpenDataDocument(@"Dimensions2.sldprt"))
            {
                swDim = (IDimension)(m_App.Documents.Active as ISwDocument).Model.Parameter("D1@Sketch1");

                r1 = m_App.Documents.Active.Dimensions.TrySetDimensionValue("D1@Sketch1", 0.1d);
                v1 = (swDim.GetSystemValue3((int)swInConfigurationOpts_e.swThisConfiguration, null) as double[])[0];

                r2 = m_App.Documents.Active.Dimensions.TrySetDimensionValue("D1@Sketch1", 0.2d, "Conf2");
                v2 = (swDim.GetSystemValue3((int)swInConfigurationOpts_e.swSpecifyConfiguration, new string[] { "Conf2" }) as double[])[0];

                r3 = m_App.Documents.Active.Dimensions.TrySetDimensionValue("D1@Sketch1", 0.5d, "Default");
                v3 = (swDim.GetSystemValue3((int)swInConfigurationOpts_e.swSpecifyConfiguration, new string[] { "Default" }) as double[])[0];

                r4 = m_App.Documents.Active.Dimensions.TrySetDimensionValue("D2@Sketch1", 0.3d, "Default");
                r5 = m_App.Documents.Active.Dimensions.TrySetDimensionValue("D2@Sketch1", 0.4d,  "Conf2");
            }

            Assert.IsTrue(r1);
            Assert.IsTrue(r2);
            Assert.IsTrue(r3);
            Assert.IsFalse(r4);
            Assert.IsFalse(r5);

            Assert.That(0.1, Is.EqualTo(v1).Within(0.001).Percent);
            Assert.That(0.2, Is.EqualTo(v2).Within(0.001).Percent);
            Assert.That(0.5, Is.EqualTo(v3).Within(0.001).Percent);
        }

        [Test]
        public void TryGetDimensionValueTest()
        {
            double r1;
            double r2;
            double r3;
            double r4;
            double r5;

            using (var doc = OpenDataDocument(@"Dimensions2.sldprt"))
            {
                r1 = m_App.Documents.Active.Dimensions.TryGetDimensionValue("D1@Sketch1");
                r2 = m_App.Documents.Active.Dimensions.TryGetDimensionValue("D1@Sketch1", "Conf2");
                r3 = m_App.Documents.Active.Dimensions.TryGetDimensionValue("D1@Sketch1", "Default");
                r4 = m_App.Documents.Active.Dimensions.TryGetDimensionValue("D2@Sketch1", "Default");
                r5 = m_App.Documents.Active.Dimensions.TryGetDimensionValue("D2@Sketch1", "Conf2");
            }

            Assert.That(0.125, Is.EqualTo(r1).Within(0.001).Percent);
            Assert.That(0.235, Is.EqualTo(r2).Within(0.001).Percent);
            Assert.That(0.125, Is.EqualTo(r3).Within(0.001).Percent);
            Assert.AreEqual(double.NaN, r4);
            Assert.AreEqual(double.NaN, r5);
        }
    }
}
