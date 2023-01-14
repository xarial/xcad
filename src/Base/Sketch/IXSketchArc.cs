//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.Sketch
{
    /// <summary>
    /// Represents the sketch circle
    /// </summary>
    public interface IXSketchCircle : IXSketchSegment, IXCircle
    {
    }

    /// <summary>
    /// Represents the sketch arc
    /// </summary>
    public interface IXSketchArc : IXSketchCircle, IXArc
    {
    }
}
