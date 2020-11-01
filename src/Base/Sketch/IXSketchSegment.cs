//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.Sketch
{
    /// <summary>
    /// Represents the sketch segmetn element
    /// </summary>
    public interface IXSketchSegment : IXSketchEntity, IXSegment
    {
        /// <summary>
        /// Underlyining segment defining this sketch segment
        /// </summary>
        IXSegment Definition { get; }
    }
}