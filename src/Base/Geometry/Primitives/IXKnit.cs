//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Geometry.Primitives
{
    /// <summary>
    /// Represents the knit premitive
    /// </summary>
    public interface IXKnit : IXPrimitive
    {
        /// <summary>
        /// Faces representing this knit
        /// </summary>
        IXRegion[] Regions { get; set; }
    }
}
