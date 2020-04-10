using NUnit.Framework;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Geometry;

namespace SolidWorks.Tests.Integration
{
    public class SelectionsTests : IntegrationTests
    {
        [Test]
        public void IterateSelectionsTest()
        {
            int selCount;
            var selTypes = new List<Type>();

            using (var doc = OpenDataDocument("Selections1.SLDPRT"))
            {
                var part = (IPartDoc)m_App.Sw.IActiveDoc2;
                (part as IModelDoc2).ClearSelection2(true);
                (part.GetEntityByName("Face1", (int)swSelectType_e.swSelFACES) as IEntity).Select4(true, null);
                (part.GetEntityByName("Face2", (int)swSelectType_e.swSelFACES) as IEntity).Select4(true, null);
                (part.GetEntityByName("Edge1", (int)swSelectType_e.swSelEDGES) as IEntity).Select4(true, null);
                (part.FeatureByName("Sketch1") as IFeature).Select2(true, -1);

                selCount = m_App.Documents.Active.Selections.Count;

                foreach (var sel in m_App.Documents.Active.Selections) 
                {
                    selTypes.Add(sel.GetType());
                }
            }

            Assert.AreEqual(4, selCount);
            Assert.That(selTypes.SequenceEqual(
                new Type[] { typeof(SwPlanarFace), typeof(SwCylindricalFace), typeof(SwEdge), typeof(SwSketch2D) }));
        }

        [Test]
        public void SelectionEventsTest() 
        {
            var selTypes = new List<Type>();

            using (var doc = OpenDataDocument("Selections1.SLDPRT"))
            {
                var part = (IPartDoc)m_App.Sw.IActiveDoc2;

                m_App.Documents.Active.Selections.NewSelection += o => selTypes.Add(o.GetType());

                (part.GetEntityByName("Face1", (int)swSelectType_e.swSelFACES) as IEntity).Select4(true, null);
                (part.GetEntityByName("Edge1", (int)swSelectType_e.swSelEDGES) as IEntity).Select4(true, null);
            }

            Assert.That(selTypes.SequenceEqual(
                new Type[] { typeof(SwPlanarFace), typeof(SwEdge) }));
        }
    }
}
