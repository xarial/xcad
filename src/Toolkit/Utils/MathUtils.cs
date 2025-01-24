//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Toolkit.Utils
{
    /// <summary>
    /// Collection of utils for maths operations
    /// </summary>
    public static class MathUtils
    {
        /// <summary>
        /// Rounds the vector with the specified tolerance
        /// </summary>
        /// <param name="vec">Vector to round</param>
        /// <param name="tol">Rounding tolerance</param>
        /// <returns>Rounded vector</returns>
        public static Vector Round(Vector vec, double tol)
        {
            var digits = ToleranceToPrecision(tol);

            return new Vector(Math.Round(vec.X, digits), Math.Round(vec.Y, digits), Math.Round(vec.Z, digits));
        }

        /// <summary>
        /// Rounds the point with the specified tolerance
        /// </summary>
        /// <param name="pt">Point to round</param>
        /// <param name="tol">Rounding tolerance</param>
        /// <returns>Rounded point</returns>
        public static Point Round(Point pt, double tol)
        {
            var digits = ToleranceToPrecision(tol);

            return new Vector(Math.Round(pt.X, digits), Math.Round(pt.Y, digits), Math.Round(pt.Z, digits));
        }

        /// <summary>
        /// Converts tolerance to precision (digits)
        /// </summary>
        /// <param name="tol">Tolerance</param>
        /// <returns>Precision digits</returns>
        /// <example>0.01 is converted to 2, 0.00001 is converted to 5</example>
        public static int ToleranceToPrecision(double tol)
            => (int)Math.Round(Math.Abs(Math.Log10(tol)));

        /// <summary>
        /// Converts precision (digits) to tolerance
        /// </summary>
        /// <param name="prec">Precision</param>
        /// <returns>Tolerance</returns>
        /// <example>2 is converter to 0.01, 5 is converted to 0.00001</example>
        public static double PrecisionToTolerance(int prec)
            => 1 / Math.Pow(10, prec);
    }
}
