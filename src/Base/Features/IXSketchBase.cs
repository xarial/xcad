//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Sketch;

namespace Xarial.XCad.Features
{
    public interface IXSketchBase : IXFeature
    {
        bool IsEditing { get; set; }
        IXSketchEntityRepository Entities { get; }
    }
}