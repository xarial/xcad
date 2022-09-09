using NUnit.Framework;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
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

                var sketch = part.CreateObjectFromDispatch<ISwSketch2D>(
                    part.Features["Sketch1"].Feature.GetSpecificFeature2() as ISketch);

                entTypes = sketch.Entities.Where(e => !(e is ISwSketchPoint) || ((ISwSketchPoint)e).Point.Type == (int)swSketchPointType_e.swSketchPointType_User)
                    .Select(e => e.GetType()).ToArray();
            }

            Assert.AreEqual(9, entTypes.Length);
            Assert.IsNotNull(entTypes.FirstOrDefault(t => typeof(ISwSketchEllipse).IsAssignableFrom(t)));
            Assert.IsNotNull(entTypes.FirstOrDefault(t => typeof(ISwSketchLine).IsAssignableFrom(t)));
            Assert.IsNotNull(entTypes.FirstOrDefault(t => typeof(ISwSketchCircle).IsAssignableFrom(t)));
            Assert.IsNotNull(entTypes.FirstOrDefault(t => typeof(ISwSketchText).IsAssignableFrom(t)));
            Assert.IsNotNull(entTypes.FirstOrDefault(t => typeof(ISwSketchSpline).IsAssignableFrom(t)));
            Assert.IsNotNull(entTypes.FirstOrDefault(t => typeof(ISwSketchParabola).IsAssignableFrom(t)));
            Assert.IsNotNull(entTypes.FirstOrDefault(t => typeof(ISwSketchArc).IsAssignableFrom(t)));
            Assert.IsNotNull(entTypes.FirstOrDefault(t => typeof(ISwSketchPicture).IsAssignableFrom(t)));
            Assert.IsNotNull(entTypes.FirstOrDefault(t => typeof(ISwSketchPoint).IsAssignableFrom(t)));
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
                line.Geometry = new Line(new Point(0.1, 0.1, 0.1), new Point(0.2, 0.2, 0.2));
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
            double l6;

            using (var doc = OpenDataDocument("Sketch1.SLDPRT"))
            {
                var part = (ISwPart)m_App.Documents.Active;

                var sketch = part.CreateObjectFromDispatch<ISwSketch2D>(
                    part.Features["Sketch1"].Feature.GetSpecificFeature2() as ISketch);

                var segs = sketch.Entities.OfType<IXSketchSegment>().ToArray();

                l1 = segs.OfType<ISwSketchEllipse>().First().Length;
                l2 = segs.OfType<ISwSketchLine>().First().Length;
                l3 = segs.OfType<ISwSketchCircle>().First().Length;
                l4 = segs.OfType<ISwSketchSpline>().First().Length;
                l5 = segs.OfType<ISwSketchParabola>().First().Length;
                l6 = segs.OfType<ISwSketchArc>().First().Length;
            }

            Assert.That(0.08017834, Is.EqualTo(l1).Within(0.001).Percent);
            Assert.That(0.05558668, Is.EqualTo(l2).Within(0.001).Percent);
            Assert.That(0.14665069, Is.EqualTo(l3).Within(0.001).Percent);
            Assert.That(0.11576035, Is.EqualTo(l4).Within(0.001).Percent);
            Assert.That(0.09222105, Is.EqualTo(l5).Within(0.001).Percent);
            Assert.That(0.03540695, Is.EqualTo(l6).Within(0.001).Percent);
        }

        [Test]
        public void IterateSketchBlocksTest() 
        {
            string[] names;

            using (var doc = OpenDataDocument("Blocks1.SLDPRT"))
            {
                var part = (ISwPart)m_App.Documents.Active;

                var sketch = part.CreateObjectFromDispatch<ISwSketch2D>(
                    part.Features["Sketch1"].Feature.GetSpecificFeature2() as ISketch);

                names = sketch.Entities.OfType<IXSketchBlockInstance>().Select(x => x.Name).ToArray();
            }

            CollectionAssert.AreEquivalent(new string[] { "Block1-1", "Block1-2", "Block2-1", "Block2-2" }, names);
        }

        [Test]
        public void SketchBlockParentsTest() 
        {
            string seg1OwnerBlock;
            string seg2OwnerBlock;
            string seg3OwnerBlock;
            string block1OwnerBlock;
            string block2OwnerBlock;

            using (var doc = OpenDataDocument("Blocks1.SLDPRT"))
            {
                var part = (ISwPart)m_App.Documents.Active;

                part.Model.Extension.SelectByID2("Line8@Sketch1", "EXTSKETCHSEGMENT", 0, 0, 0, false, 0, null, 0);
                var seg1 = (IXSketchSegment)part.Selections.First();

                part.Model.Extension.SelectByID2("Block1-1@Sketch1", "SUBSKETCHINST", 0, 0, 0, false, 0, null, 0);
                var block1 = (IXSketchBlockInstance)part.Selections.First();

                part.Model.Extension.SelectByID2("Line3/Block2-1@Sketch1", "EXTSKETCHSEGMENT", 0, 0, 0, false, 0, null, 0);
                var seg2 = (IXSketchSegment)part.Selections.First();

                part.Model.Extension.SelectByID2("Arc1/Block1-3/Block2-1@Sketch1", "EXTSKETCHSEGMENT", 0, 0, 0, false, 0, null, 0);
                var seg3 = (IXSketchSegment)part.Selections.First();

                part.Model.Extension.SelectByID2("Block1-3/Block2-1@Sketch1", "SUBSKETCHINST", 0, 0, 0, false, 0, null, 0);
                var block2 = (IXSketchBlockInstance)part.Selections.First();

                seg1OwnerBlock = seg1.OwnerBlock?.Name;
                seg2OwnerBlock = seg2.OwnerBlock?.Name;
                seg3OwnerBlock = seg3.OwnerBlock?.Name;
                block1OwnerBlock = block1.OwnerBlock?.Name;
                block2OwnerBlock = block2.OwnerBlock?.Name;
            }

            Assert.IsNull(seg1OwnerBlock);
            Assert.IsNull(block1OwnerBlock);
            Assert.AreEqual("Block2-1", seg2OwnerBlock);
            Assert.AreEqual("Block1-3", seg3OwnerBlock);
            Assert.AreEqual("Block2-1", block2OwnerBlock);
        }

        [Test]
        public void SketchBlocksEntitiesTransformTest()
        {
            bool areDefsEqual;
            Type[] entTypes;
            Point pt1;
            Point pt2;

            using (var doc = OpenDataDocument("Blocks1.SLDPRT"))
            {
                var part = (ISwPart)m_App.Documents.Active;

                var sketch = (ISwSketch2D)part.Features["Sketch1"];

                var blocks = sketch.Entities.OfType<IXSketchBlockInstance>().ToDictionary(b => b.Name, b => b);
                var block1 = blocks["Block1-1"];
                var block2 = blocks["Block1-2"];

                var def = block1.Definition;
                areDefsEqual = def.Equals(block2.Definition);

                var ents = def.Entities.OfType<IXSketchSegment>().ToArray();
                entTypes = ents.Select(e => e.GetType()).ToArray();

                var circ = ents.OfType<IXSketchCircle>().First();
                var centerPt = circ.Geometry.CenterAxis.Point;
                pt1 = centerPt.Transform(block1.Transform);
                pt2 = centerPt.Transform(block2.Transform);
            }

            Assert.IsTrue(areDefsEqual);
            Assert.AreEqual(2, entTypes.Length);
            Assert.That(entTypes.Any(e => typeof(IXSketchCircle).IsAssignableFrom(e)));
            Assert.That(entTypes.Any(e => typeof(IXSketchLine).IsAssignableFrom(e)));
            Assert.That(pt1.X, Is.EqualTo(-0.051242849137327).Within(0.00000000001).Percent);
            Assert.That(pt1.Y, Is.EqualTo(0.027597052487211).Within(0.00000000001).Percent);
            Assert.That(pt1.Z, Is.EqualTo(0).Within(0.00000000001).Percent);
            Assert.That(pt2.X, Is.EqualTo(0.032844814145853).Within(0.00000000001).Percent);
            Assert.That(pt2.Y, Is.EqualTo(0.0769249139264982).Within(0.00000000001).Percent);
            Assert.That(pt2.Z, Is.EqualTo(0).Within(0.00000000001).Percent);
        }

        [Test]
        public void OwnerSketchTestTest()
        {
            string n1;
            string n2;
            string n3;
            string n4;
            string n5;
            string n6;
            string n7;
            string n8;
            string n9;

            string n1_1;
            string n2_1;
            string n3_1;
            string n4_1;
            string n5_1;
            string n6_1;
            string n7_1;
            string n8_1;

            using (var doc = OpenDataDocument("Sketch1.SLDPRT"))
            {
                var part = (ISwPart)m_App.Documents.Active;

                var sketch = part.CreateObjectFromDispatch<ISwSketch2D>(
                    part.Features["Sketch1"].Feature.GetSpecificFeature2() as ISketch);

                n1 = ((ISwSketchPicture)sketch.Entities["Sketch Picture1"]).OwnerSketch.Name;
                n2 = ((ISwSketchLine)sketch.Entities["Line1"]).OwnerSketch.Name;
                n3 = ((ISwSketchCircle)sketch.Entities["Arc1"]).OwnerSketch.Name;
                n4 = ((ISwSketchArc)sketch.Entities["Arc2"]).OwnerSketch.Name;
                n5 = ((ISwSketchParabola)sketch.Entities["Parabola2"]).OwnerSketch.Name;
                n6 = ((ISwSketchText)sketch.Entities["SketchText1"]).OwnerSketch.Name;
                n7 = ((ISwSketchPoint)sketch.Entities["Point1@Sketch1"]).OwnerSketch.Name;
                n8 = ((ISwSketchSpline)sketch.Entities["Spline1"]).OwnerSketch.Name;
                n9 = ((ISwSketchEllipse)sketch.Entities["Ellipse1"]).OwnerSketch.Name;
            }

            using (var doc = OpenDataDocument("SheetSketch1.SLDDRW"))
            {
                var drw = (ISwDrawing)m_App.Documents.Active;

                var sketch = drw.Sheets["Sheet2"].Sketch;

                n1_1 = ((ISwSketchPicture)sketch.Entities["Sketch Picture1"]).OwnerSketch.Name;
                n2_1 = ((ISwSketchLine)sketch.Entities["Line1"]).OwnerSketch.Name;
                n3_1 = ((ISwSketchCircle)sketch.Entities["Arc1"]).OwnerSketch.Name;
                n4_1 = ((ISwSketchArc)sketch.Entities["Arc2"]).OwnerSketch.Name;
                n5_1 = ((ISwSketchParabola)sketch.Entities["Parabola1"]).OwnerSketch.Name;
                n6_1 = ((ISwSketchPoint)sketch.Entities["Point1"]).OwnerSketch.Name;
                n7_1 = ((ISwSketchSpline)sketch.Entities["Spline1"]).OwnerSketch.Name;
                n8_1 = ((ISwSketchEllipse)sketch.Entities["Ellipse1"]).OwnerSketch.Name;
            }

            Assert.AreEqual("Sketch1", n1);
            Assert.AreEqual("Sketch1", n2);
            Assert.AreEqual("Sketch1", n3);
            Assert.AreEqual("Sketch1", n4);
            Assert.AreEqual("Sketch1", n5);
            Assert.AreEqual("Sketch1", n6);
            Assert.AreEqual("Sketch1", n7);
            Assert.AreEqual("Sketch1", n8);
            Assert.AreEqual("Sketch1", n9);

            Assert.AreEqual("Sketch3", n1_1);
            Assert.AreEqual("Sketch3", n2_1);
            Assert.AreEqual("Sketch3", n3_1);
            Assert.AreEqual("Sketch3", n4_1);
            Assert.AreEqual("Sketch3", n5_1);
            Assert.AreEqual("Sketch3", n6_1);
            Assert.AreEqual("Sketch3", n7_1);
            Assert.AreEqual("Sketch3", n8_1);
        }
    }
}
