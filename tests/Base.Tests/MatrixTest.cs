using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Geometry.Structures;

namespace Base.Tests
{
    public class MatrixTest
    {
        [Test]
        public void CreateFromRotationAroundAxisTest() 
        {
            //sw transform data: 0.900202513118371, -0.424567507790233, -0.096839386120319, 0.383554841948467, 0.878329091336096, -0.285348366966756, 0.206206495031693, 0.219728101619931, 0.953518978712666, 0, 0, 0, 1, 0, 0, 0
            var matrix = new TransformMatrix(0.900202513118371, -0.424567507790233, -0.096839386120319, 0, 0.383554841948467, 0.878329091336096, -0.285348366966756, 0, 0.206206495031693, 0.219728101619931, 0.953518978712666, 0, 0, 0, 0, 1);

            var rotMatrix = TransformMatrix.CreateFromRotationAroundAxis(new Vector(-0.5, 0.3, -0.8), 0.523599, new Point(0, 0, 0));

            CollectionAssert.AreEqual(matrix.ToArray(), rotMatrix.ToArray(), new DoubleComparer());
        }

        [Test]
        public void MultiplyTest() 
        {
            //sw transform data: -0.708267430806948, 0.347623567688267, 0.614422575794384, -0.705944223333632, -0.348767569783138, -0.616444592651635, 8.88178419700125E-16, -0.870355695940025, 0.492423560103246, 0.232454472653307, 0.28957519769969, 5.93062288453039E-02, 1, 0, 0, 0
            var matrix1 = new TransformMatrix(-0.708267430806948, 0.347623567688267, 0.614422575794384, 0, -0.705944223333632, -0.348767569783138, -0.616444592651635, 0, 8.88178419700125E-16, -0.870355695940025, 0.492423560103246, 0, 0.232454472653307, 0.28957519769969, 0.0593062288453039, 1);

            //sw transform data: 0.974788926484518, 0, -0.223128995881668, 0.223128995881668, 0, 0.974788926484518, 0, -1, 0, 0.18810566875436, 0.01, 0.123668090384383, 1, 0, 0, 0
            var matrix2 = new TransformMatrix(0.974788926484518, 0, -0.223128995881668, 0, 0.223128995881668, 0, 0.974788926484518, 0, 0, -1, 0, 0, 0.18810566875436, 0.01, 0.123668090384383, 1);

            //sw transform data: -0.612846350937167, -0.614422575794384, 0.496894605019207, -0.765966769263138, 0.616444592651635, -0.182458139240622, -0.194201592494987, -0.492423560103246, -0.848413094505063, 0.479312337703569, -4.93062288453039E-02, 0.354075453415271, 1, 0, 0, 0
            var expMatrix = new double[] { -0.612846350937167, -0.614422575794384, 0.496894605019207, 0, -0.765966769263138, 0.616444592651635, -0.182458139240622, 0, -0.194201592494987, -0.492423560103246, -0.848413094505063, 0, 0.479312337703569, -0.0493062288453039, 0.354075453415271, 1 };

            var resMatrix = matrix1.Multiply(matrix2);

            CollectionAssert.AreEqual(expMatrix, resMatrix.ToArray(), new DoubleComparer());
        }

        [Test]
        public void DeterminantTest() 
        {
            var matrix = new TransformMatrix(
                -1, 3, 3.5, 9,
                1, 2, 1.5, 1,
                -3, 7.6, 9, 7.45,
                5, 2, 4, 9);

            var d = matrix.Determinant;

            Assert.That(-366.875, Is.EqualTo(d).Within(0.00001).Percent);
        }

        [Test]
        public void InverseTest()
        {
            var matrix = new TransformMatrix(
                -1, 3, 3.5, 9,
                1, 2, 1.5, 1,
                -3, 7.6, 9, 7.45,
                5, 2, 4, 9);

            var inversedMatrix = matrix.Inverse();

            var expMatrix = new double[] { -0.0941737649063033, 0.212470187393527, -0.0477001703577513, 0.110051107325383, 0.246882453151618, 0.924906303236797, -0.149914821124361, -0.225553662691652, -0.376149914821124, -0.699829642248722, 0.272572402044293, 0.228279386712095, 0.164633730834753, -0.0125383304940375, -0.0613287904599659, -0.00136286201022147 };

            CollectionAssert.AreEqual(expMatrix, inversedMatrix.ToArray(), new DoubleComparer());            
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
