using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base;

namespace Xarial.XCad.Geometry
{
    /// <summary>
    /// Represents the result of the collision
    /// </summary>
    public interface IXCollisionResult 
    {
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
    }
}
