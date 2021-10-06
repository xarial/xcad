//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
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
    /// Represents the plane reference geometry
    /// </summary>
    public interface IXPlane : IXFeature
    {
        /// <summary>
        /// Definition of this plane
        /// </summary>
        Plane Definition { get; }
    }
}
