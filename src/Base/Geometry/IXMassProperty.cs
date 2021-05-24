//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Geometry
{
    /// <summary>
    /// Evaluates mass properties of the document
    /// </summary>
    public interface IXMassProperty : IEvaluation
    {
        /// <summary>
        /// Center of Gravity
        /// </summary>
        Point CenterOfGravity { get; }

        /// <summary>
        /// Surface area
        /// </summary>
        double SurfaceArea { get; }

        /// <summary>
        /// Volume
        /// </summary>
        double Volume { get; }

        /// <summary>
        /// Mass
        /// </summary>
        double Mass { get; }

        /// <summary>
        /// Density
        /// </summary>
        double Density { get; }

        /// <summary>
        /// Principal axes of inertia
        /// </summary>
        PrincipalAxesOfInertia PrincipalAxesOfInertia { get; }

        /// <summary>
        /// Principal moment of inertia
        /// </summary>
        PrincipalMomentOfInertia PrincipalMomentOfInertia { get; }

        /// <summary>
        /// Moment of inertia
        /// </summary>
        MomentOfInertia MomentOfInertia { get; }
    }

    /// <summary>
    /// Evaluates mass properties of the assembly
    /// </summary>
    public interface IXAssemblyMassProperty : IXMassProperty, IAssemblyEvaluation
    {
    }
}