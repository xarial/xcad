//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Geometry.Primitives;

namespace Xarial.XCad.Geometry
{
    public interface IXSheetGeometryBuilder : IX3DGeometryBuilder
    {
        IXPlanarSheet PreCreatePlanarSheet();
    }
}
