//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry;
using Xarial.XCad.SolidWorks.Geometry.Curves;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public interface ISwRegion : IXRegion
    {
        new ISwCurve[] Boundary { get; }
    }
}
