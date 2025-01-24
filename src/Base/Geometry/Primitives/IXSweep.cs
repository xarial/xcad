//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.Geometry.Primitives
{
    /// <summary>
    /// Represents the wept element
    /// </summary>
    public interface IXSweep : IXPrimitive
    {
        /// <summary>
        /// Sweep profile
        /// </summary>
        IXPlanarRegion[] Profiles { get; set; }

        /// <summary>
        /// Sweep path
        /// </summary>
        IXSegment Path { get; set; }
    }
}
