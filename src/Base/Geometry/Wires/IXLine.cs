//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Geometry.Wires
{
    public interface IXLine : IXSegment
    {
        Point StartCoordinate { get; set; }
        Point EndCoordinate { get; set; }
    }
}
