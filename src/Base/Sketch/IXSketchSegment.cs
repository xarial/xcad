//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace Xarial.XCad.Sketch
{
    public interface IXSketchSegment : IXSketchEntity
    {
        IXSketchPoint StartPoint { get; }
        IXSketchPoint EndPoint { get; }
    }
}