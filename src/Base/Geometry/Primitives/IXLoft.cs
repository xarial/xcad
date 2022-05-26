//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.Geometry.Primitives
{
    /// <summary>
    /// Represents loft
    /// </summary>
    public interface IXLoft : IXPrimitive
    {
        /// <summary>
        /// Profiles of this loft
        /// </summary>
        IXPlanarRegion[] Profiles { get; set; }
    }
}
