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
    /// Represents the revolved element
    /// </summary>
    public interface IXRevolve : IXPrimitive
    {
        /// <summary>
        /// Profiles of revolve
        /// </summary>
        IXPlanarRegion[] Profiles { get; set; }
        
        /// <summary>
        /// Axis of the revolve
        /// </summary>
        IXLine Axis { get; set; }

        /// <summary>
        /// Revolution angle
        /// </summary>
        double Angle { get; set; }
    }
}
