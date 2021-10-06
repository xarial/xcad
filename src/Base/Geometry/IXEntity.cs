//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Geometry
{
    /// <summary>
    /// Represents the base itnerface for gemetrical entities
    /// </summary>
    public interface IXEntity : IXSelObject
    {
        /// <summary>
        /// Gets the component associated with this entity in the context of the assembly
        /// </summary>
        /// <remarks>Null is returned if entity is not associated with the component (e.g. assembly level feature or entity is in the context of the part)</remarks>
        IXComponent Component { get; }

        /// <summary>
        /// Returns the body which owns this entity
        /// </summary>
        IXBody Body { get; }

        /// <summary>
        /// Returns all adjacent entitites of this entity
        /// </summary>
        IEnumerable<IXEntity> AdjacentEntities { get; }

        /// <summary>
        /// Finds the closes point on the specified face
        /// </summary>
        /// <param name="point">Input point</param>
        /// <returns>Closest point</returns>
        Point FindClosestPoint(Point point);
    }
}