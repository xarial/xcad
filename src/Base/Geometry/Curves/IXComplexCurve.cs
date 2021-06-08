//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Geometry.Curves
{
    public interface IXComplexCurve : IXCurve
    {
        IXCurve[] Composition { get; set; }
    }
}
