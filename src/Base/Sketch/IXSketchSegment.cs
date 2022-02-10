//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Curves;
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
        IXCurve Definition { get; }

        /// <summary>
        /// Identifies if this sketch segment is construction geometry
        /// </summary>
        bool IsConstruction { get; }

        /// <summary>
        /// Start sketch point of this sketch segment
        /// </summary>
        new IXSketchPoint StartPoint { get; }

        /// <summary>
        /// End sketch point of this sketch segment
        /// </summary>
        new IXSketchPoint EndPoint { get; }
    }
}