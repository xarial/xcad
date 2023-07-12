using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Geometry.Structures;

namespace Base.Tests
{
    public class Rect2DTest
    {
        [Test]
        public void IntersectsTest() 
        {
            var r1 = new Rect2D(10, 10, new Point(5, 5, 0));
            var r2 = new Rect2D(5, 5, new Point(5, 5, 0));
            var r3 = new Rect2D(10, 10, new Point(7, 7, 0));
            var r4 = new Rect2D(10, 10, new Point(20, 20, 0));
            var r5 = new Rect2D(10, 10, new Point(5, 20, 0));
            var r6 = new Rect2D(10, 10, new Point(20, 5, 0));
            var r7 = new Rect2D(10, 10, new Point(15, 5, 0));
            var r8 = new Rect2D(10, 10, new Point(5, 15, 0));

            var i1 = r1.Intersects(r2);
            var i2 = r1.Intersects(r3);
            var i3 = r1.Intersects(r4);
            var i4 = r1.Intersects(r5);
            var i5 = r1.Intersects(r6);
            var i6 = r1.Intersects(r7);
            var i7 = r1.Intersects(r8);

            Assert.IsTrue(i1);
            Assert.IsTrue(i2);
            Assert.IsFalse(i3);
            Assert.IsFalse(i4);
            Assert.IsFalse(i5);
            Assert.IsTrue(i6);
            Assert.IsTrue(i7);
        }
    }
}
