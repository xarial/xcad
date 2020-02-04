//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Documents
{
    public interface IXDocument3D : IXDocument
    {
        Box3D CalculateBoundingBox();

        IXView ActiveView { get; }
    }
}