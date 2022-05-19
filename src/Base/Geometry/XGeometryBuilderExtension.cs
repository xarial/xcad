//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Linq;
using Xarial.XCad.Base;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.Geometry
{
    /// <summary>
    /// Additional methods for <see cref="IXGeometryBuilder"/>
    /// </summary>
    public static class XGeometryBuilderExtension
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
        /// <returns>Box extrusion</returns>
        public static IXExtrusion CreateSolidBox(this IXGeometryBuilder builder, Point center, Vector dir, Vector refDir,
            double width, double length, double height)
        {
            var secondRefDir = dir.Cross(refDir);

            var polyline = builder.WireBuilder.PreCreatePolyline();
            polyline.Points = new Point[]
            {
                center.Move(refDir, width / 2).Move(secondRefDir, length / 2),
                center.Move(refDir * -1, width / 2).Move(secondRefDir, length / 2),
                center.Move(refDir * -1, width / 2).Move(secondRefDir * -1, length / 2),
                center.Move(refDir, width / 2).Move(secondRefDir * -1, length / 2),
                center.Move(refDir, width / 2).Move(secondRefDir, length / 2),
            };
            polyline.Commit();

            var extr = builder.SolidBuilder.PreCreateExtrusion();
            extr.Depth = height;
            extr.Direction = dir;
            extr.Profiles = new IXRegion[] { builder.CreatePlanarSheet(builder.CreateRegionFromSegments(polyline)).Bodies.First() };
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
        /// <returns>Cylindrical extrusion</returns>
        public static IXExtrusion CreateSolidCylinder(this IXGeometryBuilder builder, Point center, Vector axis,
            double radius, double height)
        {
            var arc = builder.WireBuilder.PreCreateCircle();
            arc.Center = center;
            arc.Axis = axis;
            arc.Diameter = radius * 2;
            arc.Commit();

            var extr = builder.SolidBuilder.PreCreateExtrusion();
            extr.Depth = height;
            extr.Direction = arc.Axis;
            extr.Profiles = new IXRegion[] { builder.CreatePlanarSheet(builder.CreateRegionFromSegments(arc)).Bodies.First() };
            extr.Commit();

            return extr;
        }

        /// <summary>
        /// Create a conical revolve body
        /// </summary>
        /// <param name="builder">Geometry builder</param>
        /// <param name="center">Center of the cone base</param>
        /// <param name="axis">Cone axis</param>
        /// <param name="baseDiam">Base diameter of the cone</param>
        /// <param name="topDiam">Top diameter of the cone</param>
        /// <param name="height">Height of the cone</param>
        /// <returns></returns>
        public static IXRevolve CreateSolidCone(this IXGeometryBuilder builder, Point center, Vector axis,
            double baseDiam, double topDiam, double height)
        {
            var refDir = axis.CreateAnyPerpendicular();

            var profile = builder.WireBuilder.PreCreatePolyline();
            profile.Points = new Point[]
            {
                center,
                center.Move(axis, height),
                center.Move(axis, height).Move(refDir, topDiam / 2),
                center.Move(refDir, baseDiam / 2),
                center
            };
            profile.Commit();

            var revLine = builder.WireBuilder.PreCreateLine();
            revLine.StartCoordinate = center;
            revLine.EndCoordinate = center.Move(axis, 1);
            revLine.Commit();

            var rev = builder.SolidBuilder.PreCreateRevolve();
            rev.Axis = revLine;
            rev.Angle = Math.PI * 2;
            rev.Profiles = new IXRegion[] { builder.CreatePlanarSheet(builder.CreateRegionFromSegments(profile)).Bodies.First() };
            rev.Commit();

            return rev;
        }

        public static IXExtrusion CreateSolidExtrusion(this IXGeometryBuilder builder, 
            double depth, Vector direction, params IXRegion[] profiles) 
        {
            var extr = builder.SolidBuilder.PreCreateExtrusion();
            extr.Depth = depth;
            extr.Direction = direction;
            extr.Profiles = profiles;
            extr.Commit();

            return extr;
        }

        public static IXRevolve CreateSolidRevolve(this IXGeometryBuilder builder, IXLine axis, double angle, params IXRegion[] profiles)
        {
            var rev = builder.SolidBuilder.PreCreateRevolve();
            rev.Angle = angle;
            rev.Axis = axis;
            rev.Profiles = profiles;
            rev.Commit();

            return rev;
        }

        public static IXSweep CreateSolidSweep(this IXGeometryBuilder builder, IXSegment path, params IXRegion[] profiles)
        {
            var sweep = builder.SolidBuilder.PreCreateSweep();
            sweep.Profiles = profiles;
            sweep.Path = path;
            sweep.Commit();

            return sweep;
        }

        public static IXPlanarSheet CreatePlanarSheet(this IXGeometryBuilder builder, IXRegion boundary)
        {
            var surf = builder.SheetBuilder.PreCreatePlanarSheet();
            surf.Region = boundary;
            surf.Commit();

            return surf;
        }

        public static IXLine CreateLine(this IXGeometryBuilder builder, Point startPt, Point endPt)
        {
            var line = builder.WireBuilder.PreCreateLine();
            line.StartCoordinate = startPt;
            line.EndCoordinate = endPt;
            line.Commit();

            return line;
        }

        public static IXCircle CreateCircle(this IXGeometryBuilder builder, Point centerPt, Vector axis, double diameter)
        {
            var circle = builder.WireBuilder.PreCreateCircle();
            circle.Center = centerPt;
            circle.Axis = axis;
            circle.Diameter = diameter;
            circle.Commit();

            return circle;
        }
    }
}
