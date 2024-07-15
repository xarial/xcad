//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad.Geometry.Structures
{
    /// <summary>
    /// Triangle representing a tesselation
    /// </summary>

    [DebuggerDisplay("{" + nameof(FirstPoint) + "} - {" + nameof(SecondPoint) + "} - {" + nameof(ThirdPoint) + "} [{" + nameof(Normal) + "}]")]
    public class TesselationTriangle
    {
        /// <summary>
        /// Normal of the triangle
        /// </summary>
        public Vector Normal { get; }

        /// <summary>
        /// First point of the triangle
        /// </summary>
        public Point FirstPoint { get; }

        /// <summary>
        /// Second point of the triangle
        /// </summary>
        public Point SecondPoint { get; }

        /// <summary>
        /// Third point of the triangle
        /// </summary>
        public Point ThirdPoint { get; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public TesselationTriangle(Vector normal, Point firstPoint, Point secondPoint, Point thirdPoint)
        {
            Normal = normal;
            FirstPoint = firstPoint;
            SecondPoint = secondPoint;
            ThirdPoint = thirdPoint;
        }
    }
}
