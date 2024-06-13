using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Geometry.Structures;

namespace Base.Tests
{
    public class AxisTest
    {
        [Test]
        public void IsCollinearTest() 
        {
            var pt1 = new Point(-0.1409493395, -0.07924862951, -0.08122498726);
            var pt2 = new Point(0.29214638898, 0.10968929136, 0);
            var pt3 = new Point(0.0932228447, 0.02290893682, -0.03730713856);
            var pt4 = new Point(0.36209354558, 0.14020372378, 0.01311824738);
            var pt5 = new Point(0.02541124962, -0.06801307203, -0.14991221502);
            var pt6 = new Point(-0.1409493395 + 1E-15, -0.07924862951 + 1E-15, -0.08122498726 + 1E-15);

            var a1 = new Axis(pt1, pt2 - pt1);
            var a2 = new Axis(pt3, pt2 - pt3);
            var a3 = new Axis(pt4, pt4 - pt1);
            var a4 = new Axis(pt5, pt2 - pt1);
            var a5 = new Axis(pt5, pt5 - pt1);
            var a6 = new Axis(pt6, pt2 - pt6);
            var a7 = new Axis(pt6, pt2 - pt1);

            var r1 = a1.IsCollinear(a2);
            var r2 = a1.IsCollinear(a3);
            var r3 = a1.IsCollinear(a3);
            var r4 = a1.IsCollinear(a4);
            var r5 = a1.IsCollinear(a5);
            var r6 = a1.IsCollinear(a6);
            var r7 = a1.IsCollinear(a7);

            Assert.IsTrue(r1);
            Assert.IsTrue(r2);
            Assert.IsTrue(r3);
            Assert.IsFalse(r4);
            Assert.IsFalse(r5);
            Assert.IsTrue(r6);
            Assert.IsTrue(r7);
        }

        [Test]
        public void IntersectsTest() 
        {
            var p1 = new Point(-523.0197, -15.815324, 123.118984);
            var p2 = new Point(391.106067, -16.363395, -55.299189);
            var p3 = new Point(-217.161315, -88.526508, 146.907671);
            var p4 = new Point(-213.374093, 38.560007, -0.121792);

            var p5 = new Point(-13.34668, 25.441204, 0);
            var p6 = new Point(87.756267, 71.060659, 0);
            var p7 = new Point(10.415043, 66.830558, 0);
            var p8 = new Point(73.071505, -3.01006, 0);

            var a1 = new Axis(p1, p2 - p1);
            var a2 = new Axis(p3, p4 - p3);

            var a3 = new Axis(p5, p6 - p5);
            var a4 = new Axis(p7, p8 - p7);

            var r1 = a1.Intersects(a2, out var i1, 1E-6);
            var r2 = a3.Intersects(a4, out var i2);
            var r3 = a1.Intersects(a4, out var i3);

            Assert.IsTrue(r1);
            Assert.That(i1.X, Is.EqualTo(-215).Within(0.01).Percent);
            Assert.That(i1.Y, Is.EqualTo(-16).Within(0.01).Percent);
            Assert.That(i1.Z, Is.EqualTo(63).Within(0.01).Percent);

            Assert.IsTrue(r2);
            Assert.That(i2.X, Is.EqualTo(30).Within(0.01).Percent);
            Assert.That(i2.Y, Is.EqualTo(45).Within(0.01).Percent);
            Assert.That(i2.Z, Is.EqualTo(0).Within(0.01).Percent);

            Assert.IsFalse(r3);
            Assert.IsNull(i3);
        }
    }
}
