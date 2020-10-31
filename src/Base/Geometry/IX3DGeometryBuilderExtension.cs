using System;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Geometry
{
    public static class IX3DGeometryBuilderExtension
    {
        public static IXExtrusion CreateBox(this IX3DGeometryBuilder builder, Point center, Vector dir, Vector refDir,
            double width, double length, double height)
        {
            throw new NotImplementedException();
        }

        public static IXExtrusion CreateCylinder(this IX3DGeometryBuilder builder, Point center, Vector axis, Vector refDir,
            double radius, double height)
        {
            throw new NotImplementedException();
        }

        public static IXRevolve CreateCone(this IX3DGeometryBuilder builder, Point center, Vector axis, Vector refDir,
            double baseRadius, double topRadius, double height)
        {
            throw new NotImplementedException();
        }
    }
}
