//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.Geometry.Primitives
{
    public interface IXRevolve : IXPrimitive
    {
        IXRegion[] Profiles { get; set; }
        IXLine Axis { get; set; }
        double Angle { get; set; }
    }
}
