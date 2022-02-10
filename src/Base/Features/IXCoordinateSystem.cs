//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

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
