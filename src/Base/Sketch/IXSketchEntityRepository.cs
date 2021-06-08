//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;
using Xarial.XCad.Geometry;

namespace Xarial.XCad.Sketch
{
    public interface IXSketchEntityRepository : IXRepository<IXSketchEntity>, IXWireGeometryBuilder
    {
    }
}