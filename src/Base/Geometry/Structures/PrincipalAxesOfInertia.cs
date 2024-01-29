//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Evaluation;

namespace Xarial.XCad.Geometry.Structures
{
    /// <summary>
    /// Principal axes of inertia of the solid geometry used in <see cref="IXMassProperty"/>
    /// </summary>
    public class PrincipalAxesOfInertia
    {
        /// <summary>
        /// X direction
        /// </summary>
        public Vector Ix { get; }

        /// <summary>
        /// Y direction
        /// </summary>
        public Vector Iy { get; }

        /// <summary>
        /// Z direction
        /// </summary>
        public Vector Iz { get; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public PrincipalAxesOfInertia(Vector ix, Vector iy, Vector iz)
        {
            Ix = ix;
            Iy = iy;
            Iz = iz;
        }
    }
}
