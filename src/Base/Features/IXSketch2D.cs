//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;

namespace Xarial.XCad.Features
{
    /// <summary>
    /// Represents specific 2D sketch
    /// </summary>
    public interface IXSketch2D : IXSketchBase
    {
        /// <summary>
        /// Regions in this 2D sketch
        /// </summary>
        IEnumerable<IXSketchRegion> Regions { get; }
    }
}