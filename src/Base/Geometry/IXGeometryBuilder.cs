//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Geometry
{
    public interface IXGeometryBuilder
    {
        IXBody CreateBox(Point center, Vector dir, Vector refDir,
            double width, double length, double height);

        IXBody CreateCylinder(Point center, Vector axis, Vector refDir,
            double radius, double height);

        IXBody CreateCone(Point center, Vector axis, Vector refDir,
            double baseRadius, double topRadius, double height);
    }
}