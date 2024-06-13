using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Geometry.Structures;

namespace Base.Tests
{
    public class CircleTest
    {
        [Test]
        public void CreateFrom3PointsTest()
        {
            var p1 = new Point(-440.933511, 77.888835, 89.14838);
            var p2 = new Point(-59.388601, 119.38515, -0.458066);
            var p3 = new Point(-343.8792, -242.144204, 0.45194);

            var p4 = new Point(-0.103741027, -0.004430111, 0);
            var p5 = new Point(-0.073881588, 0.027805391, 0);
            var p6 = new Point(-0.016022961, -0.005459556, 0);

            var p7 = new Point(-0.5230197, -0.015815324, 0.123118984);
            var p8 = new Point(0.391106067, -0.016363395, -0.055299189);
            var p9 = new Point(-0.215, -0.016, 0.063);

            var c1 = Circle.From3Points(p1, p2, p3);
            var c2 = Circle.From3Points(p4, p5, p6);

            Assert.Throws<Exception>(() => Circle.From3Points(p7, p8, p9));

            Assert.That(c1.Diameter, Is.EqualTo(473).Within(0.01).Percent);
            Assert.That(c1.CenterAxis.Point.X, Is.EqualTo(-242.703255).Within(0.01).Percent);
            Assert.That(c1.CenterAxis.Point.Y, Is.EqualTo(-29.018937).Within(0.01).Percent);
            Assert.That(c1.CenterAxis.Point.Z, Is.EqualTo(16.98368).Within(0.01).Percent);
            Assert.IsTrue(c1.CenterAxis.IsCollinear(new Axis(new Point(-242.703255, -29.018937, 16.98368), new Vector(-242.703255 - -180.547946, -29.018937 - -77.319622, 16.98368 - 259.273628)), 1E-5, 1E-6));

            Assert.That(c2.Diameter, Is.EqualTo(0.09).Within(0.01).Percent);
            Assert.That(c2.CenterAxis.Point.X, Is.EqualTo(-0.06).Within(0.01).Percent);
            Assert.That(c2.CenterAxis.Point.Y, Is.EqualTo(-0.015).Within(0.01).Percent);
            Assert.That(c2.CenterAxis.Point.Z, Is.EqualTo(0).Within(0.01).Percent);
            Assert.IsTrue(c2.CenterAxis.IsCollinear(new Axis(new Point(-0.06, -0.015, 0), new Vector(0, 0, 1))));
        }
    }
}
