using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
