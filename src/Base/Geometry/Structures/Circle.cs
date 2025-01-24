//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Utils;

namespace Xarial.XCad.Geometry.Structures
{
    /// <summary>
    /// Represents the circle geometry
    /// </summary>
    [DebuggerDisplay("d{" + nameof(Diameter) + "} ({" + nameof(CenterAxis) + "})")]
    public class Circle
    {
        /// <summary>
        /// Creates circle from 3 points
        /// </summary>
        /// <param name="firstPt">First point on circle</param>
        /// <param name="secondPt">Second point on circle</param>
        /// <param name="thirdPt">Third point on circle</param>
        /// <param name="tol">Tolerance</param>
        /// <returns>Created circle</returns>
        /// <exception cref="Exception">When circle cannot be created (e.g. points are collinear)</exception>
        public static Circle From3Points(Point firstPt, Point secondPt, Point thirdPt,
            double tol = Numeric.DEFAULT_LENGTH_TOLERANCE)
        {
            var firstVec = firstPt - secondPt;
            var secondVec = secondPt - thirdPt;
            var norm = firstVec.Cross(secondVec);

            var transformToXY = TransformMatrix.Compose(firstVec, norm.Cross(firstVec), norm, firstPt).Inverse();

            firstPt = firstPt * transformToXY;
            secondPt = secondPt * transformToXY;
            thirdPt = thirdPt * transformToXY;

            var x1 = (secondPt.X + firstPt.X) / 2;
            var y1 = (secondPt.Y + firstPt.Y) / 2;
            var dy1 = secondPt.X - firstPt.X;
            var dx1 = -(secondPt.Y - firstPt.Y);

            var x2 = (thirdPt.X + secondPt.X) / 2;
            var y2 = (thirdPt.Y + secondPt.Y) / 2;
            var dy2 = thirdPt.X - secondPt.X;
            var dx2 = -(thirdPt.Y - secondPt.Y);

            if (new Axis(new Point(x1, y1, 0), new Vector(dx1, dy1, 0)).Intersects(
                new Axis(new Point(x2, y2, 0), new Vector(dx2, dy2, 0)),
                out var interPt, tol))
            {
                var dx = interPt.X - firstPt.X;
                var dy = interPt.Y - firstPt.Y;
                var radius = Math.Sqrt(dx * dx + dy * dy);

                return new Circle(new Axis(interPt.Transform(transformToXY.Inverse()), norm), radius * 2);
            }
            else
            {
                throw new Exception("Failed to create circle from provided points");
            }
        }

        /// <summary>
        /// Diameter of the circle
        /// </summary>
        public double Diameter { get; set; }

        /// <summary>
        /// Axis perpendicular to the circle's plane
        /// </summary>
        public Axis CenterAxis { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Circle() 
        {
        }

        /// <summary>
        /// Constructor with geometry
        /// </summary>
        /// <param name="centerAxis">Axis</param>
        /// <param name="diam">Diameter</param>
        public Circle(Axis centerAxis, double diam) 
        {
            CenterAxis = centerAxis;
            Diameter = diam;
        }
    }
}
