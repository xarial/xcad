using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Curves;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.SolidWorks.Geometry.Curves
{
    public class SwArc : SwCurve, IXArcCurve
    {
        internal SwArc(ICurve curve) : base(curve)
        {
        }

        public double Diameter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Point Center { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
