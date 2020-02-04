//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Geometry
{
    public static class IXGeometryBuilderExtension
    {
        public static IXBody CreateBox(this IXGeometryBuilder builder, Point center, Vector dir,
            double width, double length, double height, out Vector refDir)
        {
            refDir = FindRefDir(dir);

            return builder.CreateBox(center, dir, refDir, width, length, height);
        }

        public static IXBody CreateBox(this IXGeometryBuilder builder, Point center, Vector dir,
            double width, double length, double height)
        {
            Vector refDir;
            return CreateBox(builder, center, dir, width, length, height, out refDir);
        }

        public static IXBody CreateCylinder(this IXGeometryBuilder builder, Point center, Vector axis,
            double radius, double height, out Vector refDir)
        {
            refDir = FindRefDir(axis);
            return builder.CreateCylinder(center, axis, refDir, radius, height);
        }

        public static IXBody CreateCylinder(this IXGeometryBuilder builder, Point center, Vector axis,
            double radius, double height)
        {
            Vector refDir;
            return CreateCylinder(builder, center, axis, radius, height, out refDir);
        }

        public static IXBody CreateCone(this IXGeometryBuilder builder, Point center, Vector axis,
            double baseRadius, double topRadius, double height, out Vector refDir)
        {
            refDir = FindRefDir(axis);
            return builder.CreateCone(center, axis, refDir, baseRadius, topRadius, height);
        }

        public static IXBody CreateCone(this IXGeometryBuilder builder, Point center, Vector axis,
            double baseRadius, double topRadius, double height)
        {
            Vector refDir;
            return CreateCone(builder, center, axis, baseRadius, topRadius, height, out refDir);
        }

        private static Vector FindRefDir(Vector dir)
        {
            Vector refDir;
            var zVec = new Vector(0, 0, 1);

            if (dir.IsSame(zVec))
            {
                refDir = new Vector(1, 0, 0);
            }
            else
            {
                refDir = dir.Cross(zVec);
            }

            return refDir;
        }
    }
}