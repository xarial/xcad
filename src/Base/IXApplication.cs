//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Documents;
using Xarial.XCad.Geometry;

namespace Xarial.XCad
{
    public interface IXApplication
    {
        IXDocumentCollection Documents { get; }
        IXGeometryBuilder GeometryBuilder { get; }
    }
}