using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Curves;
using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.SolidWorks.Geometry.Curves
{
    public class SwCurve : IXCurve
    {
        public ICurve Curve { get; }

        public IXPoint StartPoint => throw new NotImplementedException();

        public IXPoint EndPoint => throw new NotImplementedException();

        public bool IsCommitted => throw new NotImplementedException();

        internal SwCurve(ICurve curve) 
        {
            Curve = curve;
        }

        public void Commit()
        {
            throw new NotImplementedException();
        }
    }
}
