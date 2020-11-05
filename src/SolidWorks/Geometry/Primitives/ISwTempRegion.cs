using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.SolidWorks.Geometry.Curves;

namespace Xarial.XCad.SolidWorks.Geometry.Primitives
{
    public interface ISwTempRegion : ISwRegion
    {
        SwTempPlanarSheetBody Body { get; }
    }
}
