using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Curves;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.SolidWorks.Geometry.Curves
{
    public class SwLine : SwCurve, IXLineCurve
    {
        internal SwLine(ICurve curve) : base(curve)
        {
        }

        public Point StartCoordinate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Point EndCoordinate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
