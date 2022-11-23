//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;

namespace Xarial.XCad.Geometry
{
    /// <summary>
    /// Represents the result of the collision
    /// </summary>
    public interface IXCollisionResult 
    {
        /// <summary>
        /// Bodies that are involved in collision
        /// </summary>
        IXBody[] CollidedBodies { get; }

        /// <summary>
        /// Represents the geometry of the collision
        /// </summary>
        IXBody[] CollisionVolume { get; }
    }

    /// <summary>
    /// Represents the result of the collision in the assembly
    /// </summary>
    public interface IXAssemblyCollisionResult : IXCollisionResult 
    {
        /// <summary>
        /// Components which are involved in the collision
        /// </summary>
        IXComponent[] CollidedComponents { get; }
    }

    /// <summary>
    /// Represents collision detection between elements
    /// </summary>
    public interface IXCollisionDetection : IEvaluation
    {
        /// <summary>
        /// Collision results
        /// </summary>
        IXCollisionResult[] Results { get; }
    }

    /// <summary>
    /// Represents collision detection between components in the assembly
    /// </summary>
    public interface IXAssemblyCollisionDetection : IXCollisionDetection, IAssemblyEvaluation
    {
        /// <inheritdoc/>
        new IXAssemblyCollisionResult[] Results { get; }
    }
}
