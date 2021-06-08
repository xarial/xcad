using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Features
{
    /// <summary>
    /// Represents the coordinate system feature
    /// </summary>
    public interface IXCoordinateSystem : IXFeature
    {
        /// <summary>
        /// Transformation of this coordinate system
        /// </summary>
        TransformMatrix Transform { get; }
    }
}
