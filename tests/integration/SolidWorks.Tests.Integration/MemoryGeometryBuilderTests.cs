using NUnit.Framework;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Curves;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.Geometry.Curves;

namespace SolidWorks.Tests.Integration
{
    public class MemoryGeometryBuilderTests : IntegrationTests
    {
        [Test]
        public void CircleSweepTest()
        {
            int faceCount;
            double[] massPrps;

            using (var doc = NewDocument(swDocumentTypes_e.swDocPART))
            {
                var sweepCircle = m_App.MemoryGeometryBuilder.WireBuilder.PreCreateCircle();
                sweepCircle.Center = new Point(0, 0, 0);
                sweepCircle.Axis = new Vector(0, 0, 1);
                sweepCircle.Diameter = 0.01;
                sweepCircle.Commit();

                var sweepLine = m_App.MemoryGeometryBuilder.WireBuilder.PreCreateLine();
                sweepLine.StartCoordinate = new Point(0, 0, 0);
                sweepLine.EndCoordinate = new Point(1, 1, 1);
                sweepLine.Commit();

                var sweep = m_App.MemoryGeometryBuilder.SolidBuilder.PreCreateSweep();
                sweep.Profiles = new IXPlanarRegion[] { m_App.MemoryGeometryBuilder.CreatePlanarSheet(
                    m_App.MemoryGeometryBuilder.CreateRegionFromSegments(sweepCircle)).Bodies.First() };
                sweep.Path = sweepLine;
                sweep.Commit();

                var body = (sweep.Bodies.First() as ISwBody).Body;

                var feat = (m_App.Documents.Active as ISwPart).Part.CreateFeatureFromBody3(body, false, 0) as IFeature;

                faceCount = feat.GetFaceCount();
                massPrps = feat.IGetBody2().GetMassProperties(0) as double[];
            }

            Assert.AreEqual(3, faceCount);
            Assert.That(0.49999999396850558, Is.EqualTo(massPrps[0]).Within(0.001).Percent);
            Assert.That(0.50000000621319451, Is.EqualTo(massPrps[1]).Within(0.001).Percent);
            Assert.That(0.4999999999985747, Is.EqualTo(massPrps[2]).Within(0.001).Percent);
            Assert.That(7.8539510270934476E-05, Is.EqualTo(massPrps[3]).Within(0.001).Percent);
            Assert.That(0.043845670222023812, Is.EqualTo(massPrps[4]).Within(0.001).Percent);
        }

        [Test]
        public void ConeTest()
        {
            int faceCount;
            double[] massPrps;

            using (var doc = NewDocument(Interop.swconst.swDocumentTypes_e.swDocPART))
            {
                var cone = m_App.MemoryGeometryBuilder.CreateSolidCone(
                    new Point(0, 0, 0),
                    new Vector(1, 1, 1),
                    0.1, 0.05, 0.2);

                var body = (cone.Bodies.First() as ISwBody).Body;

                var feat = (m_App.Documents.Active as ISwPart).Part.CreateFeatureFromBody3(body, false, 0) as IFeature;

                faceCount = feat.GetFaceCount();
                massPrps = feat.IGetBody2().GetMassProperties(0) as double[];
            }

            Assert.AreEqual(3, faceCount);
            Assert.That(0.045363235436327753, Is.EqualTo(massPrps[0]).Within(0.001).Percent);
            Assert.That(0.045363235436327753, Is.EqualTo(massPrps[1]).Within(0.001).Percent);
            Assert.That(0.045363235436327753, Is.EqualTo(massPrps[2]).Within(0.001).Percent);
            Assert.That(0.00091629785729702325, Is.EqualTo(massPrps[3]).Within(0.001).Percent);
            Assert.That(0.057308095255097079, Is.EqualTo(massPrps[4]).Within(0.001).Percent);
        }

        [Test]
        public void CircleRevolveTest()
        {
            int faceCount;
            double[] massPrps;

            using (var doc = NewDocument(Interop.swconst.swDocumentTypes_e.swDocPART))
            {
                var circle = m_App.MemoryGeometryBuilder.WireBuilder.PreCreateCircle();
                circle.Center = new Point(-0.1, 0, 0);
                circle.Axis = new Vector(0, 0, 1);
                circle.Diameter = 0.01;
                circle.Commit();

                var axis = m_App.MemoryGeometryBuilder.WireBuilder.PreCreateLine();
                axis.StartCoordinate = new Point(0, 0, 0);
                axis.EndCoordinate = new Point(0, 1, 0);
                axis.Commit();

                var rev = m_App.MemoryGeometryBuilder.SolidBuilder.PreCreateRevolve();
                rev.Angle = Math.PI * 2;
                rev.Axis = axis;
                rev.Profiles = new IXPlanarRegion[] { m_App.MemoryGeometryBuilder.CreatePlanarSheet(
                    m_App.MemoryGeometryBuilder.CreateRegionFromSegments(circle)).Bodies.First() };
                rev.Commit();

                var body = (rev.Bodies.First() as ISwBody).Body;

                var feat = (m_App.Documents.Active as ISwPart).Part.CreateFeatureFromBody3(body, false, 0) as IFeature;

                faceCount = feat.GetFaceCount();
                massPrps = feat.IGetBody2().GetMassProperties(0) as double[];
            }

            Assert.AreEqual(1, faceCount);
            Assert.That(3.9006081899147006E-18, Is.EqualTo(massPrps[0]).Within(0.001).Percent);
            Assert.That(0, Is.EqualTo(massPrps[1]).Within(0.001).Percent);
            Assert.That(0, Is.EqualTo(massPrps[2]).Within(0.001).Percent);
            Assert.That(4.9348022005446818E-05, Is.EqualTo(massPrps[3]).Within(0.001).Percent);
            Assert.That(0.019739208802178717, Is.EqualTo(massPrps[4]).Within(0.001).Percent);
        }

        [Test]
        public void BoxTest()
        {
            int faceCount;
            double[] massPrps;

            using (var doc = NewDocument(Interop.swconst.swDocumentTypes_e.swDocPART))
            {
                var box = m_App.MemoryGeometryBuilder.CreateSolidBox(
                    new Point(0, 0, 0),
                    new Vector(1, 1, 1),
                    new Vector(1, 1, 1).CreateAnyPerpendicular(),
                    0.1, 0.2, 0.3);

                var body = (box.Bodies.First() as ISwBody).Body;

                var feat = (m_App.Documents.Active as ISwPart).Part.CreateFeatureFromBody3(body, false, 0) as IFeature;

                faceCount = feat.GetFaceCount();
                massPrps = feat.IGetBody2().GetMassProperties(0) as double[];
            }

            Assert.AreEqual(6, faceCount);
            Assert.That(0.086602540378443879, Is.EqualTo(massPrps[0]).Within(0.001).Percent);
            Assert.That(0.086602540378443879, Is.EqualTo(massPrps[1]).Within(0.001).Percent);
            Assert.That(0.086602540378443879, Is.EqualTo(massPrps[2]).Within(0.001).Percent);
            Assert.That(0.0060000000000000036, Is.EqualTo(massPrps[3]).Within(0.001).Percent);
            Assert.That(0.22000000000000014, Is.EqualTo(massPrps[4]).Within(0.001).Percent);
        }

        [Test]
        public void ExtrusionTest() 
        {
            int faceCount;
            double[] massPrps;

            using (var doc = NewDocument(swDocumentTypes_e.swDocPART))
            {
                var polyline = m_App.MemoryGeometryBuilder.WireBuilder.PreCreatePolyline();
                polyline.Points = new Point[]
                {
                    new Point(0, 0, 0),
                    new Point(0.1, 0.1, 0),
                    new Point(0.2, 0, 0),
                    new Point(0, 0, 0)
                };
                polyline.Commit();

                var extr = m_App.MemoryGeometryBuilder.SolidBuilder.PreCreateExtrusion();
                extr.Depth = 0.5;
                extr.Direction = new Vector(1, 1, 1);
                extr.Profiles = new IXPlanarRegion[] { m_App.MemoryGeometryBuilder.CreatePlanarSheet(
                    m_App.MemoryGeometryBuilder.CreateRegionFromSegments(polyline)).Bodies.First() };
                extr.Commit();

                var body = (extr.Bodies.First() as ISwBody).Body;

                var feat = (m_App.Documents.Active as ISwPart).Part.CreateFeatureFromBody3(body, false, 0) as IFeature;

                faceCount = feat.GetFaceCount();
                massPrps = feat.IGetBody2().GetMassProperties(0) as double[];
            }

            Assert.AreEqual(5, faceCount);
            Assert.That(0.24433756729740647, Is.EqualTo(massPrps[0]).Within(0.001).Percent);
            Assert.That(0.17767090063073979, Is.EqualTo(massPrps[1]).Within(0.001).Percent);
            Assert.That(0.14433756729740646, Is.EqualTo(massPrps[2]).Within(0.001).Percent);
            Assert.That(0.002886751345948132, Is.EqualTo(massPrps[3]).Within(0.001).Percent);
            Assert.That(0.21318516525781389, Is.EqualTo(massPrps[4]).Within(0.001).Percent);
        }

        [Test]
        public void ExtrusionPolylineTest() 
        {
            var profile = m_App.MemoryGeometryBuilder.WireBuilder.PreCreatePolyline();

            profile.Points = new Point[] 
            {
                new Point(0, 0, 0),
                new Point(0.1, 0, 0),
                new Point(0.1, 0.1, 0),
                new Point(0, 0.1, 0),
                new Point(0, 0, 0),
            };

            profile.Commit();

            var profileReg = m_App.MemoryGeometryBuilder.CreateRegionFromSegments(profile);
            
            var ext = m_App.MemoryGeometryBuilder.CreateSolidExtrusion(0.1, new Vector(0, 0, 1), profileReg);
            
            var body = (IXSolidBody)ext.Bodies.First();
            
            var vol = body.Volume;
            Assert.That(0.001, Is.EqualTo(vol).Within(0.001).Percent);
        }

        [Test]
        public void CylinderTest() 
        {
            int faceCount;
            double[] massPrps;

            using (var doc = NewDocument(Interop.swconst.swDocumentTypes_e.swDocPART))
            {
                var cyl = m_App.MemoryGeometryBuilder.CreateSolidCylinder(
                new Point(0, 0, 0), new Vector(1, 0, 0), 0.1, 0.2);

                var body = (cyl.Bodies.First() as ISwBody).Body;

                var feat = (m_App.Documents.Active as ISwPart).Part.CreateFeatureFromBody3(body, false, 0) as IFeature;

                faceCount = feat.GetFaceCount();
                massPrps = feat.IGetBody2().GetMassProperties(0) as double[];
            }

            Assert.AreEqual(3, faceCount);
            Assert.That(0.10000000000000002, Is.EqualTo(massPrps[0]).Within(0.001).Percent);
            Assert.That(1.949085916259688E-18, Is.EqualTo(massPrps[1]).Within(0.001).Percent);
            Assert.That(0, Is.EqualTo(massPrps[2]).Within(0.001).Percent);
            Assert.That(0.0062831853071795875, Is.EqualTo(massPrps[3]).Within(0.001).Percent);
            Assert.That(0.18849555921538758, Is.EqualTo(massPrps[4]).Within(0.001).Percent);
        }

        [Test]
        public void PlanarSheetTest() 
        {
            bool isPlanar;
            bool isCircular;
            int edgeCount;
            double[] normal;
            double[] circleParams;

            using (var doc = NewDocument(swDocumentTypes_e.swDocPART))
            {
                var arc = m_App.MemoryGeometryBuilder.WireBuilder.PreCreateCircle();
                arc.Center = new Point(0.75, 0.5, 0.15);
                arc.Axis = new Vector(1E-16d, 0, 1);
                arc.Diameter = 2.5;
                arc.Commit();
                var face = m_App.MemoryGeometryBuilder.CreatePlanarSheet(
                    m_App.MemoryGeometryBuilder.CreateRegionFromSegments(arc)).Bodies.First().Faces.First();
                isPlanar = face is IXPlanarFace;
                edgeCount = (face as ISwFace).Face.GetEdgeCount();
                isCircular = (((face as ISwFace).Face.GetEdges() as object[]).First() as IEdge).IGetCurve().IsCircle();
                circleParams = (double[])(((face as ISwFace).Face.GetEdges() as object[]).First() as IEdge).IGetCurve().CircleParams;
                normal = (face as ISwFace).Face.Normal as double[];
            }

            Assert.AreEqual(true, isPlanar);
            Assert.AreEqual(true, isCircular);
            Assert.AreEqual(1, edgeCount);
            AssertCompareDoubles(0, normal[0]);
            AssertCompareDoubles(0, normal[1]);
            AssertCompareDoubles(1, normal[2]);
            Assert.That(0.75, Is.EqualTo(circleParams[0]).Within(0.001).Percent);
            Assert.That(0.5, Is.EqualTo(circleParams[1]).Within(0.001).Percent);
            Assert.That(0.15, Is.EqualTo(circleParams[2]).Within(0.001).Percent);
            Assert.That(1.25, Is.EqualTo(circleParams[6]).Within(0.001).Percent);
        }

        [Test]
        public void PlanarSheetSketchRegionTest() 
        {
            double area;

            using (var doc = OpenDataDocument("SketchRegion1.SLDPRT")) 
            {
                var part = (IXPart)m_App.Documents.Active;

                var sketch = (IXSketch2D)part.Features["Sketch1"];

                var reg = sketch.Regions.First();

                var surf = m_App.MemoryGeometryBuilder.CreatePlanarSheet(reg);

                area = surf.Bodies.First().Faces.First().Area;
            }

            Assert.That(0.00105942, Is.EqualTo(area).Within(0.001).Percent);
        }

        [Test]
        public void SurfaceKnitTest()
        {
            int b1Count;
            int f1Count;
            double a1;

            int b2Count;
            int f2_1Count;
            int f2_2Count;
            double a2_1;
            double a2_2;

            using (var doc = OpenDataDocument("FacePart.SLDPRT"))
            {
                var part = (ISwPart)m_App.Documents.Active;

                var face1 = part.CreateObjectFromDispatch<ISwFace>(part.Part.GetEntityByName("FACE1", (int)swSelectType_e.swSelFACES));//0.00743892
                var face2 = part.CreateObjectFromDispatch<ISwFace>(part.Part.GetEntityByName("FACE2", (int)swSelectType_e.swSelFACES));//0.01130973
                var face3 = part.CreateObjectFromDispatch<ISwFace>(part.Part.GetEntityByName("FACE3", (int)swSelectType_e.swSelFACES));//0.01202536
                var face4 = part.CreateObjectFromDispatch<ISwFace>(part.Part.GetEntityByName("FACE4", (int)swSelectType_e.swSelFACES));//0.01463892

                var knit1 = m_App.MemoryGeometryBuilder.SheetBuilder.PreCreateKnit();
                knit1.Regions = new IXRegion[] { face2, face3 };
                knit1.Commit();

                b1Count = knit1.Bodies.Length;
                f1Count = knit1.Bodies[0].Faces.Count();
                a1 = knit1.Bodies[0].Faces.Sum(f => f.Area);

                var knit2 = m_App.MemoryGeometryBuilder.SheetBuilder.PreCreateKnit();
                knit2.Regions = new IXRegion[] { face1, face3, face4 };
                knit2.Commit();

                b2Count = knit2.Bodies.Length;
                f2_1Count = knit2.Bodies[0].Faces.Count();
                f2_2Count = knit2.Bodies[1].Faces.Count();
                a2_1 = knit2.Bodies[0].Faces.Sum(f => f.Area);
                a2_2 = knit2.Bodies[1].Faces.Sum(f => f.Area);
            }

            Assert.AreEqual(1, b1Count);
            Assert.AreEqual(2, f1Count);
            Assert.That(0.01130973 + 0.01202536, Is.EqualTo(a1).Within(0.01).Percent);

            Assert.AreEqual(2, b2Count);
            Assert.AreEqual(2, f2_1Count);
            Assert.AreEqual(1, f2_2Count);
            Assert.That(0.00743892, Is.EqualTo(a2_2).Within(0.01).Percent);
            Assert.That(0.01202536 + 0.01463892, Is.EqualTo(a2_1).Within(0.01).Percent);
        }

        [Test]
        public void SerializeDeserializeTest()
        {
            double mass;

            using (var doc = OpenDataDocument("Features1.SLDPRT")) 
            {
                var part = (ISwPart)m_App.Documents.Active;

                byte[] buffer;

                using (var memStr = new MemoryStream()) 
                {
                    var body = part.Bodies.First();
                    m_App.MemoryGeometryBuilder.SerializeBody(body, memStr);
                    buffer = memStr.ToArray();
                }

                using (var memStr = new MemoryStream(buffer))
                {
                    var body = (ISwBody)m_App.MemoryGeometryBuilder.DeserializeBody(memStr);
                    mass = (body.Body.GetMassProperties(0) as double[])[3];
                }

                Assert.That(2.3851693679806192E-05, Is.EqualTo(mass).Within(0.001).Percent);
            }
        }

        [Test]
        public void ArcTest()
        {
            Type curve1Type;
            double curve1Length;
            double curve2Length;

            using (var doc = OpenDataDocument("FacePart.sldprt"))
            {
                var part = (ISwPart)m_App.Documents.Active;
                var edge = part.CreateObjectFromDispatch<ISwEdge>(part.Part.GetEntityByName("EDGE1", (int)swSelectType_e.swSelEDGES));
                var curve1 = edge.Definition;
                curve1Type = curve1.GetType();
                curve1Length = curve1.Length;

                var arc2 = m_App.MemoryGeometryBuilder.WireBuilder.PreCreateArc();
                arc2.Center = new Point(0, 0, 0);
                arc2.Axis = new Vector(0, 0, -1);
                arc2.Diameter = 0.01;
                arc2.Start = new Point(-0.005, 0, 0);
                arc2.End = new Point(0, 0.005, 0);
                arc2.Commit();

                curve2Length = arc2.Length;
            }

            Assert.That(typeof(ISwArcCurve).IsAssignableFrom(curve1Type));
            Assert.That(0.09424777961, Is.EqualTo(curve1Length).Within(0.001).Percent);
            Assert.That(Math.PI * 0.01 * 0.25, Is.EqualTo(curve2Length).Within(0.001).Percent);
        }

        [Test]
        public void CircleTest()
        {
            Type curve1Type;
            double curve1Length;
            double curve2Length;

            using (var doc = OpenDataDocument("FacePart.sldprt"))
            {
                var part = (ISwPart)m_App.Documents.Active;
                var edge = part.CreateObjectFromDispatch<ISwEdge>(part.Part.GetEntityByName("EDGE2", (int)swSelectType_e.swSelEDGES));
                var curve1 = edge.Definition;
                curve1Type = curve1.GetType();
                curve1Length = curve1.Length;

                var circle2 = m_App.MemoryGeometryBuilder.WireBuilder.PreCreateCircle();
                circle2.Center = new Point(0, 0, 0);
                circle2.Axis = new Vector(0, 0, -1);
                circle2.Diameter = 0.01;
                circle2.Commit();

                curve2Length = circle2.Length;
            }

            Assert.That(typeof(ISwCircleCurve).IsAssignableFrom(curve1Type) && !typeof(ISwArcCurve).IsAssignableFrom(curve1Type));
            Assert.That(0.12214774689, Is.EqualTo(curve1Length).Within(0.001).Percent);
            Assert.That(Math.PI * 0.01, Is.EqualTo(curve2Length).Within(0.001).Percent);
        }
    }
}
