using System;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Geometry
{
    public static class IX3DGeometryBuilderExtension
    {
        /// <summary>
        /// Creates a box body from the specified parameters
        /// </summary>
        /// <param name="builder">Geometry builder</param>
        /// <param name="center">Center of the box base face</param>
        /// <param name="dir">Direction of the box</param>
        /// <param name="refDir">Reference direction of the box base face (this is the vector perpendicular to 'dir'</param>
        /// <param name="width">Width of the box. This size is parallel to 'refDir' vector</param>
        /// <param name="length">Length of the box</param>
        /// <param name="height">Height of the box. THis size is parallel to 'dir' vector</param>
        /// <param name="wireBuilder">Wire geometry builder</param>
        /// <returns>Box extrusion</returns>
        public static IXExtrusion CreateBox(this IX3DGeometryBuilder builder, Point center, Vector dir, Vector refDir,
            double width, double length, double height, IXWireGeometryBuilder wireBuilder)
        {
            var secondRefDir = dir.Cross(refDir);

            var polyline = wireBuilder.PreCreatePolyline();
            polyline.Points = new Point[]
            {
                center.Move(refDir, width / 2).Move(secondRefDir, height / 2),
                center.Move(refDir * -1, width / 2).Move(secondRefDir, height / 2),
                center.Move(refDir * -1, width / 2).Move(secondRefDir * -1, height / 2),
                center.Move(refDir, width / 2).Move(secondRefDir * -1, height / 2),
                center.Move(refDir, width / 2).Move(secondRefDir, height / 2),
            };
            polyline.Commit();

            var extr = builder.PreCreateExtrusion();
            extr.Depth = height;
            extr.Direction = dir;
            extr.Profiles = new Wires.IXSegment[] { polyline };
            extr.Commit();

            return extr;
        }

        /// <summary>
        /// Creates cylindrical extrusion from input parameters
        /// </summary>
        /// <param name="builder">Geometry builder</param>
        /// <param name="center">Center of the cylinder base</param>
        /// <param name="axis">Direction of the cylinder</param>
        /// <param name="radius">Radius of the cylinder</param>
        /// <param name="height">Height of the cylinder</param>
        /// <param name="wireBuilder">Wire geometry builder</param>
        /// <returns>Cylindrical extrusion</returns>
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

        /// <summary>
        /// Create a conical revolve body
        /// </summary>
        /// <param name="builder">Geometry builder</param>
        /// <param name="center">Center of the cone base</param>
        /// <param name="axis">Cone axis</param>
        /// <param name="baseRadius">Base radius of the cone</param>
        /// <param name="topRadius">Top radius of the cone</param>
        /// <param name="height">Height of the cone</param>
        /// <param name="wireBuilder">Wire geometry builder</param>
        /// <returns></returns>
        public static IXRevolve CreateCone(this IX3DGeometryBuilder builder, Point center, Vector axis,
            double baseRadius, double topRadius, double height, IXWireGeometryBuilder wireBuilder)
        {
            var refDir = axis.CreateAnyPerpendicular();

            var profile = wireBuilder.PreCreatePolyline();
            profile.Points = new Point[]
            {
                center,
                center.Move(axis, height),
                center.Move(axis, height).Move(refDir, topRadius / 2),
                center.Move(refDir, baseRadius / 2),
                center
            };
            profile.Commit();

            var revLine = wireBuilder.PreCreateLine();
            revLine.StartCoordinate = center;
            revLine.EndCoordinate = center.Move(axis, 1);
            revLine.Commit();

            var rev = builder.PreCreateRevolve();
            rev.Axis = revLine;
            rev.Angle = Math.PI * 2;
            rev.Profile = profile;
            rev.Commit();

            return rev;
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
