using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Geometry.Structures;

namespace Base.Tests
{
    public class PointTest
    {
        [Test]
        public void TransformTest() 
        {
            //sw transform data: -0.708267430806948, 0.347623567688267, 0.614422575794384, -0.705944223333632, -0.348767569783138, -0.616444592651635, 8.88178419700125E-16, -0.870355695940025, 0.492423560103246, 0.232454472653307, 0.28957519769969, 5.93062288453039E-02, 1, 0, 0, 0
            var matrix = new TransformMatrix(-0.708267430806948, 0.347623567688267, 0.614422575794384, 0, -0.705944223333632, -0.348767569783138, -0.616444592651635, 0, 8.88178419700125E-16, -0.870355695940025, 0.492423560103246, 0, 0.232454472653307, 0.28957519769969, 0.0593062288453039, 1);
            var point = new Point(1, 3, 5);

            var point1 = point.Transform(matrix);

            Assert.That(-2.59364562815453, Is.EqualTo(point1.X).Within(0.00001).Percent);
            Assert.That(-4.76088242366158, Is.EqualTo(point1.Y).Within(0.00001).Percent);
            Assert.That(1.28651282720101, Is.EqualTo(point1.Z).Within(0.00001).Percent);
        }

        [Test]
        public void TransformWithScaleTest()
        {
            //sw transform data: 0.8899781799633, 0.258097541614499, -0.375931507327816, 4.48886450650032E-02, -0.869988052665455, -0.491025251655756, -0.453788330318707, 0.420126703783421, -0.7858560326387, 724.374297695573, 431.891671932574, 13.2759519466606, 634.33320578319, 0, 0, 0
            var matrix = new TransformMatrix(564.542711973209, 163.719840977085, -238.46583819816, 0, 28.4743581273473, -551.862310440353, -311.473622003293, 0, -287.853006318067, 266.500318846062, -498.494576467766, 0, 724.374297695573, 431.891671932574, 13.2759519466606, 1);
            var point = new Point(-0.2, 0.35, -0.7);

            var point1 = point.Transform(matrix);

            Assert.That(822.92888506815, Is.EqualTo(point1.X).Within(0.00001).Percent);
            Assert.That(19.4456718907902, Is.EqualTo(point1.Y).Within(0.00001).Percent);
            Assert.That(300.899555412576, Is.EqualTo(point1.Z).Within(0.00001).Percent);
        }

        [Test]
        public void TransformWithScaleToPlaneTest()
        {
            //sw transform data: 0.795342405339345, 0.455413697145403, 0.400036026779314, -0.606108810937849, 0.606108810937846, 0.515038074910018, -7.90996665808835E-03, -0.652096981863791, 0.758094294050287, 0.700972369348501, 0.438557054675365, -9.46540940610755E-02, 3, 0, 0, 0
            var matrix = new TransformMatrix(2.38602721601803, 1.36624109143621, 1.20010808033794, 0, -1.81832643281355, 1.81832643281354, 1.54511422473005, 0, -0.023729899974265, -1.95629094559137, 2.27428288215086, 0, 0.700972369348501, 0.438557054675365, -0.0946540940610755, 1);
            var point = new Point(0, 0, 0);

            var point1 = point.Transform(matrix);

            Assert.That(0.700972369348501, Is.EqualTo(point1.X).Within(0.00001).Percent);
            Assert.That(0.438557054675365, Is.EqualTo(point1.Y).Within(0.00001).Percent);
            Assert.That(-9.46540940610755E-02, Is.EqualTo(point1.Z).Within(0.00001).Percent);
        }
    }
}
