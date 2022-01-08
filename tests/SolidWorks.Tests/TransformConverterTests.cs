using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Utils;

namespace SolidWorks.Tests
{
    public class TransformConverterTests
    {
        [Test]
        public void RoundtripRotationTranslationTest() 
        {
            var swTransformData = new double[] 
            {
                0.499999999999998, 0.5, -0.707106781186549, -0.146446609406726,
                0.853553390593274, 0.5, 0.853553390593275, -0.146446609406726,
                0.499999999999998, -0.0974873734152917, -0.072487373415292,
                0.122487373415293, 1, 0, 0, 0 
            };

            var matrix = TransformConverter.ToTransformMatrix(swTransformData);

            var res = TransformConverter.ToMathTransformData(matrix);

            CollectionAssert.AreEqual(swTransformData, res);
        }

        [Test]
        public void RotationTest() 
        {
            //30 degs rotation around X
            var matrix = new TransformMatrix(
                1,      0,              0,              0,
                0,      0.86602540378,  -0.5,           0,
                0,      0.5,            0.86602540378,  0,
                0,      0,              0,              1);

            var data = TransformConverter.ToMathTransformData(matrix);

            CollectionAssert.AreEqual(new double[] { 1, 0, 0, 0, 0.86602540378, -0.5, 0, 0.5, 0.86602540378, 0, 0, 0, 1, 0, 0, 0 }, data);
        }

        [Test]
        public void RoundtripScaleTest()
        {
            var swTransformData = new double[]
            {
                -0.173281870700371,
                -0.568901317857332,
                -0.803943209329347,
                 0.871391797781904,
                -0.468966056249135,
                 0.144038789374787,
                -0.458965933425347,
                -0.675590207615774,
                 0.576999257650021,
                 0.118738080204305,
                 3.07942116552466E-03,
                -1.31754673526319E-02,
                 3,
                 0,
                 0,
                 0 
            };

            var matrix = TransformConverter.ToTransformMatrix(swTransformData);

            var res = TransformConverter.ToMathTransformData(matrix);

            CollectionAssert.AreEqual(swTransformData, res, new DoubleComparer());
        }

        [Test]
        public void ToMathTransformTest() 
        {
            var matrix = new TransformMatrix(0.227240687239813, 0.130118199184401, 0.114296007651233, 0, -0.173173945982243, 0.173173945982242, 0.147153735688577, 0, -0.00225999047373953, -0.186313423389655, 0.216598369728654, 0, 0.202001019294273, 0.304139434520347, -0.00901467562486433, 1);
            var transformData = TransformConverter.ToMathTransformData(matrix);

            var expTransform = new double[] { 0.795342405339345, 0.455413697145403, 0.400036026779314, -0.606108810937849, 0.606108810937846, 0.515038074910018, -7.90996665808835E-03, -0.652096981863791, 0.758094294050287, 0.202001019294273, 0.304139434520347, -9.01467562486433E-03, 0.285714285714286, 0, 0, 0 };
            CollectionAssert.AreEqual(expTransform, transformData, new DoubleComparer());
        }

        [Test]
        public void IdentityTest() 
        {
            var matrix = TransformConverter.ToTransformMatrix(new double[]
            {
                1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0
            });

            CollectionAssert.AreEqual(TransformMatrix.Identity.ToArray(), matrix.ToArray(), new DoubleComparer());
        }

        [Test]
        public void TranslationTest()
        {
            var matrix = TransformConverter.ToTransformMatrix(new double[]
            {
                1, 0, 0, 0, 1, 0, 0, 0, 1, 0.01, 0.02, 0.03, 1, 0, 0, 0
            });
            
            Assert.AreEqual(matrix.M41, 0.01);
            Assert.AreEqual(matrix.M42, 0.02);
            Assert.AreEqual(matrix.M43, 0.03);
        }

        public class DoubleComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                var d1 = (double)x;
                var d2 = (double)y;
                
                if (Math.Round(d1, 14) == Math.Round(d2, 14))
                {
                    return 0;
                }
                else
                {
                    return d1.CompareTo(d2);
                }
            }
        }
    }
}
