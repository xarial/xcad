using NUnit.Framework;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry;

namespace SolidWorks.Tests.Integration
{
    public class MemoryGeometryBuilderTests : IntegrationTests
    {
        [Test]
        public void ArcSweepTest()
        {
            int faceCount;
            double[] massPrps;

            using (var doc = NewDocument(Interop.swconst.swDocumentTypes_e.swDocPART))
            {
                var sweepArc = m_App.MemoryGeometryBuilder.WireBuilder.PreCreateArc();
                sweepArc.Center = new Point(0, 0, 0);
                sweepArc.Axis = new Vector(0, 0, 1);
                sweepArc.Diameter = 0.01;
                sweepArc.Commit();

                var sweepLine = m_App.MemoryGeometryBuilder.WireBuilder.PreCreateLine();
                sweepLine.StartCoordinate = new Point(0, 0, 0);
                sweepLine.EndCoordinate = new Point(1, 1, 1);
                sweepLine.Commit();

                var sweep = m_App.MemoryGeometryBuilder.SolidBuilder.PreCreateSweep();
                sweep.Profiles = new IXRegion[] { m_App.MemoryGeometryBuilder.CreatePlanarSheet(sweepArc).Bodies.First() };
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
        public void ArcRevolveTest()
        {
            int faceCount;
            double[] massPrps;

            using (var doc = NewDocument(Interop.swconst.swDocumentTypes_e.swDocPART))
            {
                var arc = m_App.MemoryGeometryBuilder.WireBuilder.PreCreateArc();
                arc.Center = new Point(-0.1, 0, 0);
                arc.Axis = new Vector(0, 0, 1);
                arc.Diameter = 0.01;
                arc.Commit();

                var axis = m_App.MemoryGeometryBuilder.WireBuilder.PreCreateLine();
                axis.StartCoordinate = new Point(0, 0, 0);
                axis.EndCoordinate = new Point(0, 1, 0);
                axis.Commit();

                var rev = m_App.MemoryGeometryBuilder.SolidBuilder.PreCreateRevolve();
                rev.Angle = Math.PI * 2;
                rev.Axis = axis;
                rev.Profiles = new IXRegion[] { m_App.MemoryGeometryBuilder.CreatePlanarSheet(arc).Bodies.First() };
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

            using (var doc = NewDocument(Interop.swconst.swDocumentTypes_e.swDocPART))
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
                extr.Profiles = new IXRegion[] { m_App.MemoryGeometryBuilder.CreatePlanarSheet(polyline).Bodies.First() };
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
    }
}
