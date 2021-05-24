//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Geometry.Primitives;

namespace Xarial.XCad.Geometry
{
    public interface IX3DGeometryBuilder 
    {
        IXExtrusion PreCreateExtrusion();
        IXSweep PreCreateSweep();
        IXLoft PreCreateLoft();
        IXRevolve PreCreateRevolve();
    }
}
