using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Memory;
using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public class SwMemoryWireGeometryBuilder : IXMemoryWireGeometryBuilder
    {
        public IXArc PreCreateArc()
        {
            throw new NotImplementedException();
        }

        public IXLine PreCreateLine()
        {
            throw new NotImplementedException();
        }

        public IXPoint PreCreatePoint()
        {
            throw new NotImplementedException();
        }
    }
}
