﻿//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad.Geometry.Structures
{
    /// <summary>
    /// Represents the line element
    /// </summary>
    public class Line
    {
        /// <summary>
        /// Start point of the line
        /// </summary>
        public Point StartPoint { get; set; }

        /// <summary>
        /// End point of the line
        /// </summary>
        public Point EndPoint { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Line() 
        {
        }

        /// <summary>
        /// Constructor with input coordinates
        /// </summary>
        /// <param name="startPt">Start point</param>
        /// <param name="endPt">End point</param>
        public Line(Point startPt, Point endPt) 
        {
            StartPoint = startPt;
            EndPoint = endPt;
        }
    }
}
