using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

            System.Diagnostics.Debug.Print (string.Join(", ", featNames.Select(f => $"\"{f}\"").ToArray()));

            Assert.That(featNames.SequenceEqual(
                new string[] { "Favorites", "Selection Sets", "Sensors", "Design Binder", "Annotations", "Notes", "Notes1___EndTag___", "Surface Bodies", "Solid Bodies", "Lights, Cameras and Scene", "Ambient", "Directional1", "Directional2", "Directional3", "Markups", "Equations", "Material <not specified>", "Front Plane", "Top Plane", "Right Plane", "Origin", "Sketch1", "Boss-Extrude1", "Boss-Extrude2", "Sketch1<2>" }));
        }
    }
}
