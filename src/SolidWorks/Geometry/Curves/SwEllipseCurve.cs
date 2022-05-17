//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Curves;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry.Curves;

namespace Xarial.XCad.SolidWorks.Geometry.Curves
{
    public interface ISwEllipseCurve : IXEllipseCurve, ISwCurve
    {
    }

    internal class SwEllipseCurve : SwCurve, ISwEllipseCurve
    {
        internal SwEllipseCurve(ICurve curve, SwDocument doc, SwApplication app, bool isCreated) 
            : base(curve, doc, app, isCreated)
        {
        }
    }
}
