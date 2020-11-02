using System;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;

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
                center.Move(refDir, width / 2).Move(secondRefDir, length / 2),
                center.Move(refDir * -1, width / 2).Move(secondRefDir, length / 2),
                center.Move(refDir * -1, width / 2).Move(secondRefDir * -1, length / 2),
                center.Move(refDir, width / 2).Move(secondRefDir * -1, length / 2),
                center.Move(refDir, width / 2).Move(secondRefDir, length / 2),
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

        public static IXExtrusion CreateExtrusion(this IX3DGeometryBuilder builder, 
            double depth, Vector direction, IXSegment[] profiles) 
        {
            var extr = builder.PreCreateExtrusion();
            extr.Depth = depth;
            extr.Direction = direction;
            extr.Profiles = profiles;
            extr.Commit();

            return extr;
        }

        public static IXRevolve CreateRevolve(this IX3DGeometryBuilder builder, IXSegment profile, IXLine axis, double angle)
        {
            var rev = builder.PreCreateRevolve();
            rev.Angle = angle;
            rev.Axis = axis;
            rev.Profile = profile;
            rev.Commit();

            return rev;
        }

        public static IXSweep CreateSweep(this IX3DGeometryBuilder builder, IXSegment profile, IXSegment path)
        {
            var sweep = builder.PreCreateSweep();
            sweep.Profile = profile;
            sweep.Path = path;
            sweep.Commit();

            return sweep;
        }

        public static IXPlanarSurface CreatePlanarSurface(this IXSurfaceGeometryBuilder builder, IXSegment boundary)
        {
            var surf = builder.PreCreatePlanarSurface();
            surf.Boundary = boundary;
            surf.Commit();

            return surf;
        }
    }
}
