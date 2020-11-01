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
