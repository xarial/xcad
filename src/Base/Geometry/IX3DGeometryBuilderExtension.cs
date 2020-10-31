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

        public static IXExtrusion CreateCylinder(this IX3DGeometryBuilder builder, Point center, Vector axis,
            double radius, double height, IXWireGeometryBuilder wireBuilder)
        {
            var arc = wireBuilder.PreCreateArc();
            arc.Center = center;
            arc.Axis = axis;
            arc.Diameter = radius * 2;
            arc.Commit();

            var extr = builder.PreCreateExtrusion();
            extr.Depth = height;
            extr.Direction = arc.Axis;
            extr.Profiles = new Wires.IXSegment[] { arc };
            extr.Commit();

            return extr;
        }

        public static IXRevolve CreateCone(this IX3DGeometryBuilder builder, Point center, Vector axis, Vector refDir,
            double baseRadius, double topRadius, double height)
        {
            throw new NotImplementedException();
        }

        //public static IXBody CreateBox(this IX3DGeometryBuilder builder, Point center, Vector dir,
        //    double width, double length, double height, out Vector refDir)
        //{
        //    refDir = FindRefDir(dir);

        //    return builder.CreateBox(center, dir, refDir, width, length, height);
        //}

        //public static IXBody CreateBox(this IX3DGeometryBuilder builder, Point center, Vector dir,
        //    double width, double length, double height)
        //{
        //    Vector refDir;
        //    return CreateBox(builder, center, dir, width, length, height, out refDir);
        //}

        //public static IXBody CreateCylinder(this IX3DGeometryBuilder builder, Point center, Vector axis,
        //    double radius, double height, out Vector refDir)
        //{
        //    refDir = FindRefDir(axis);
        //    return builder.CreateCylinder(center, axis, refDir, radius, height);
        //}

        //public static IXBody CreateCylinder(this IX3DGeometryBuilder builder, Point center, Vector axis,
        //    double radius, double height)
        //{
        //    Vector refDir;
        //    return CreateCylinder(builder, center, axis, radius, height, out refDir);
        //}

        //public static IXBody CreateCone(this IX3DGeometryBuilder builder, Point center, Vector axis,
        //    double baseRadius, double topRadius, double height, out Vector refDir)
        //{
        //    refDir = FindRefDir(axis);
        //    return builder.CreateCone(center, axis, refDir, baseRadius, topRadius, height).Body;
        //}

        //public static IXBody CreateCone(this IX3DGeometryBuilder builder, Point center, Vector axis,
        //    double baseRadius, double topRadius, double height)
        //{
        //    Vector refDir;
        //    return CreateCone(builder, center, axis, baseRadius, topRadius, height, out refDir);
        //}

        //private static Vector FindRefDir(Vector dir)
        //{
        //    Vector refDir;
        //    var zVec = new Vector(0, 0, 1);

        //    if (dir.IsSame(zVec))
        //    {
        //        refDir = new Vector(1, 0, 0);
        //    }
        //    else
        //    {
        //        refDir = dir.Cross(zVec);
        //    }

        //    return refDir;
        //}
    }
}
