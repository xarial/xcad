//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Geometry
{
    /// <summary>
    /// Represents the base itnerface for gemetrical entities
    /// </summary>
    public interface IXEntity : IXSelObject
    {
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