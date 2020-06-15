//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Geometry
{
    public interface IXEdge : IXEntity
    {
    }

    public interface IXCircularEdge : IXEdge 
    {
        Point Center { get; }
        Vector Axis { get; }
        double Radius { get; }
    }

    public interface IXLinearEdge : IXEdge
    {
        Point RootPoint { get; }
        Vector Direction { get; }
    }
}