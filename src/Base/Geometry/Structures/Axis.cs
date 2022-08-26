//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Geometry.Structures
{
    /// <summary>
    /// Represents axis - direction through the point
    /// </summary>
    public class Axis
    {
        /// <summary>
        /// Reference point of this axis
        /// </summary>
        public Point Point { get; set; }

        /// <summary>
        /// Direction of this axis
        /// </summary>
        public Vector Direction { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Axis() 
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="refPt">Reference point of this exis</param>
        /// <param name="dir">Direction of the exis</param>
        public Axis(Point refPt, Vector dir) 
        {
            Point = refPt;
            Direction = dir;
        }
    }
}
