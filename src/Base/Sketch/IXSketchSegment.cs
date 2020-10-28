//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace Xarial.XCad.Sketch
{
    /// <summary>
    /// Represents the sketch segmetn element
    /// </summary>
    public interface IXSketchSegment : IXSketchEntity
    {
        /// <summary>
        /// Start point of this sketch segment
        /// </summary>
        IXSketchPoint StartPoint { get; }

        /// <summary>
        /// End point of this sketch segment
        /// </summary>
        IXSketchPoint EndPoint { get; }
    }
}