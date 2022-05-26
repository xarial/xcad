//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.Geometry.Primitives
{
    public interface IXSweep : IXPrimitive
    {
        IXPlanarRegion[] Profiles { get; set; }
        IXSegment Path { get; set; }
    }
}
