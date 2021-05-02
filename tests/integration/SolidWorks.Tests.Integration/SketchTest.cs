using NUnit.Framework;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Sketch;

namespace SolidWorks.Tests.Integration
{
    public class SketchTest : IntegrationTests
    {
        [Test]
        public void IterateSketchEntitiesTest() 
        {
            Type[] entTypes;

            using (var doc = OpenDataDocument("Sketch1.SLDPRT")) 
            {
                var part = (ISwPart)m_App.Documents.Active;

                var sketch = SwObjectFactory.FromDispatch<ISwSketch2D>(
                    part.Features["Sketch1"].Feature.GetSpecificFeature2() as ISketch, part);

                entTypes = sketch.Entities.Where(e => !(e is ISwSketchPoint)).Select(e => e.GetType()).ToArray();
            }

            Assert.AreEqual(6, entTypes.Length);
            Assert.IsNotNull(entTypes.FirstOrDefault(t => typeof(ISwSketchEllipse).IsAssignableFrom(t)));
            Assert.IsNotNull(entTypes.FirstOrDefault(t => typeof(ISwSketchLine).IsAssignableFrom(t)));
            Assert.IsNotNull(entTypes.FirstOrDefault(t => typeof(ISwSketchArc).IsAssignableFrom(t)));
            Assert.IsNotNull(entTypes.FirstOrDefault(t => typeof(ISwSketchText).IsAssignableFrom(t)));
            Assert.IsNotNull(entTypes.FirstOrDefault(t => typeof(ISwSketchSpline).IsAssignableFrom(t)));
            Assert.IsNotNull(entTypes.FirstOrDefault(t => typeof(ISwSketchParabola).IsAssignableFrom(t)));
        }

        [Test]
        public void CreateLineTest() 
        {
            var x1 = -1d;
            var y1 = -1d;
            var z1 = -1d;

            var x2 = -1d;
            var y2 = -1d;
            var z2 = -1d;

            using (var doc = NewDocument(Interop.swconst.swDocumentTypes_e.swDocPART))
            {
                var part = m_App.Documents.Active;

                var sketch3D = (ISwSketch3D)part.Features.PreCreate3DSketch();
                var line = (IXSketchLine)sketch3D.Entities.PreCreateLine();
                
                line.StartCoordinate = new Point(0.1, 0.1, 0.1);
                line.EndCoordinate = new Point(0.2, 0.2, 0.2);
                sketch3D.Entities.AddRange(new IXSketchEntity[] { line });

                part.Features.Add(sketch3D);

                var skLine = (sketch3D.Sketch.GetSketchSegments() as object[]).First() as ISketchLine;
                
                var startPt = skLine.GetStartPoint2() as ISketchPoint;
                
                x1 = startPt.X;
                y1 = startPt.Y;
                z1 = startPt.Z;

                var endPt = skLine.GetEndPoint2() as ISketchPoint;
                
                x2 = endPt.X;
                y2 = endPt.Y;
                z2 = endPt.Z;
            }

            Assert.That(0.1, Is.EqualTo(x1).Within(0.001).Percent);
            Assert.That(0.1, Is.EqualTo(y1).Within(0.001).Percent);
            Assert.That(0.1, Is.EqualTo(z1).Within(0.001).Percent);
            Assert.That(0.2, Is.EqualTo(x2).Within(0.001).Percent);
            Assert.That(0.2, Is.EqualTo(y2).Within(0.001).Percent);
            Assert.That(0.2, Is.EqualTo(z2).Within(0.001).Percent);
        }

        [Test]
        public void GetSketchEntitiesLengthTest()
        {
            double l1;
            double l2;
            double l3;
            double l4;
            double l5;

            using (var doc = OpenDataDocument("Sketch1.SLDPRT"))
            {
                var part = (ISwPart)m_App.Documents.Active;

                var sketch = SwObjectFactory.FromDispatch<ISwSketch2D>(
                    part.Features["Sketch1"].Feature.GetSpecificFeature2() as ISketch, part);

                var segs = sketch.Entities.OfType<IXSketchSegment>().ToArray();

                l1 = segs.OfType<ISwSketchEllipse>().First().Length;
                l2 = segs.OfType<ISwSketchLine>().First().Length;
                l3 = segs.OfType<ISwSketchArc>().First().Length;
                l4 = segs.OfType<ISwSketchSpline>().First().Length;
                l5 = segs.OfType<ISwSketchParabola>().First().Length;
            }

            Assert.That(0.12991965190301241, Is.EqualTo(l1).Within(0.001).Percent);
            Assert.That(0.08468717758758991, Is.EqualTo(l2).Within(0.001).Percent);
            Assert.That(0.10094045912639603, Is.EqualTo(l3).Within(0.001).Percent);
            Assert.That(0.16421451670784409, Is.EqualTo(l4).Within(0.001).Percent);
            Assert.That(0.1034014049596117, Is.EqualTo(l5).Within(0.001).Percent);
        }
    }
}
