using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Curves;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.SolidWorks.Geometry.Curves
{
    public abstract class SwPlanarCurve : SwCurve, IXPlanarCurve
    {
        internal SwPlanarCurve(IModeler modeler, ICurve curve, bool isCreated) : base(modeler, curve, isCreated)
        {
        }

        public abstract Plane Plane { get; }
    }
}
