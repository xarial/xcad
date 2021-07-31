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

            var rotMatrix = TransformMatrix.CreateFromRotationAroundAxis(new Vector(-0.5, 0.3, -0.8), 0.523599);

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
            var expMatrix = new TransformMatrix(-0.612846350937167, -0.614422575794384, 0.496894605019207, 0, -0.765966769263138, 0.616444592651635, -0.182458139240622, 0, -0.194201592494987, -0.492423560103246, -0.848413094505063, 0, 0.479312337703569, -0.0493062288453039, 0.354075453415271, 1);

            var resMatrix = matrix1.Multiply(matrix2);

            CollectionAssert.AreEqual(expMatrix.ToArray(), resMatrix.ToArray(), new DoubleComparer());
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
