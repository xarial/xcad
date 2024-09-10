//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.Features
{
    /// <summary>
    /// Represents the coordinate system feature
    /// </summary>
    public interface IXCoordinateSystem : IXFeature
    {
        /// <summary>
        /// Entity representing origin of this coordinate system
        /// </summary>
        IXPoint Origin { get; set; }

        /// <summary>
        /// Entity representing X axis of this coordinate system
        /// </summary>
        IXLine AxisX { get; set; }

        /// <summary>
        /// Entity representing Y axis of this coordinate system
        /// </summary>
        IXLine AxisY { get; set; }

        /// <summary>
        /// Entity representing Z axis of this coordinate system
        /// </summary>
        IXLine AxisZ { get; set; }

        /// <summary>
        /// True if X axis is flipped
        /// </summary>
        bool AxisXFlipped { get; set; }

        /// <summary>
        /// True if Y axis is flipped
        /// </summary>
        bool AxisYFlipped { get; set; }

        /// <summary>
        /// True if Z axis is flipped
        /// </summary>
        bool AxisZFlipped { get; set; }

        /// <summary>
        /// Transformation of this coordinate system
        /// </summary>
        TransformMatrix Transform { get; set; }
    }
}
