using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.SolidWorks.Geometry.Curves
{
    public class SwPoint : IXPoint
    {
        public Point Coordinate { get; set; }

        public bool IsCommitted => true;

        public void Commit()
        {
        }
    }
}
