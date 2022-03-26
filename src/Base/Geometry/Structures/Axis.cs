﻿using System;
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
        public Point RefPoint { get; set; }

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
            RefPoint = refPt;
            Direction = dir;
        }
    }
}