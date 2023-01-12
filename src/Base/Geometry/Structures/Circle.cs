//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
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
    /// Represents the circle geometry
    /// </summary>
    public class Circle
    {
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
