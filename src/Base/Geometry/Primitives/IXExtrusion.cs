//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.Geometry.Primitives
{
    public interface IXExtrusion : IXPrimitive
    {
        IXRegion[] Profiles { get; set; }
        double Depth { get; set; }
        Vector Direction { get; set; }
    }
}
