using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Geometry.Wires
{
    public interface IXPoint : IXTransaction
    {
        Point Coordinate { get; set; }
    }
}
