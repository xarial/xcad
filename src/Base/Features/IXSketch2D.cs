//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;

namespace Xarial.XCad.Features
{
    public interface IXSketch2D : IXSketchBase
    {
        IEnumerable<IXSketchRegion> Regions { get; }
    }
}