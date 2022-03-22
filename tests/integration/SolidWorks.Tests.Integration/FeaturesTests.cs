using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Features;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks.Features;

namespace SolidWorks.Tests.Integration
{
    public class FeaturesTests : IntegrationTests
    {
        [Test]
        public void IterateFeaturesTest()
        {
            var featNames = new List<string>();

            using (var doc = OpenDataDocument("Features1.SLDPRT"))
            {
                foreach (var feat in m_App.Documents.Active.Features)
                {
                    featNames.Add(feat.Name);
                }
            }

            var expected = new string[] { "Comments", "Favorites", "Selection Sets", "Sensors", "Design Binder", "Annotations", "Notes",
                "Notes1___EndTag___", "Surface Bodies", "Solid Bodies", "Lights, Cameras and Scene", "Ambient", "Directional1", "Directional2", "Directional3",
                "Markups", "Equations", "Material <not specified>", "Front Plane", "Top Plane", "Right Plane", "Origin", "Sketch1", "Boss-Extrude1", "Boss-Extrude2",
                "Sketch1<2>" };

            CollectionAssert.AreEqual(expected, featNames);
        }

        [Test]
        public void GetFeatureByNameTest()
        {
            IXFeature feat1;
            IXFeature feat2;
            IXFeature feat3;
            bool r1;
            bool r2;
            Exception e1 = null;

            using (var doc = OpenDataDocument("Features1.SLDPRT"))
            {
                feat1 = m_App.Documents.Active.Features["Sketch1"];
                r1 = m_App.Documents.Active.Features.TryGet("Sketch1", out feat2);
                r2 = m_App.Documents.Active.Features.TryGet("Sketch2", out feat3);
                
                try
                {
                    var feat4 = m_App.Documents.Active.Features["Sketch2"];
                }
                catch (Exception ex)
                {
                    e1 = ex;
                }
            }

            Assert.IsNotNull(feat1);
            Assert.IsNotNull(feat2);
            Assert.IsNull(feat3);
            Assert.IsTrue(r1);
            Assert.IsFalse(r2);
            Assert.IsNotNull(e1);
        }

        [Test]
        public void SpecificFeatureEqualsTest() 
        {
            var e1 = false;

            using (var doc = OpenDataDocument("Sketch1.SLDPRT"))
            {
                var sketch1 = m_App.Documents.Active.Features["Sketch1"];
                var sketch2 = ((ISwSketchBase)sketch1).Entities.OfType<IXSketchSegment>().First().OwnerSketch;
                e1 = sketch1.Equals(sketch2);
            }

            Assert.IsTrue(e1);
        }
    }
}
