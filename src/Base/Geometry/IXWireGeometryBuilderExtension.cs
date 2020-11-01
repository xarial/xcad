using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.Geometry
{
    public static class IXWireGeometryBuilderExtension
    {
        public static IXLine CreateLine(this IXWireGeometryBuilder builder, Point startPt, Point endPt) 
        {
            var line = builder.PreCreateLine();
            line.StartCoordinate = startPt;
            line.EndCoordinate = endPt;
            line.Commit();

            return line;
        }

        public static IXArc CreateCircle(this IXWireGeometryBuilder builder, Point centerPt, Vector axis, double diameter)
        {
            var circle = builder.PreCreateArc();
            circle.Center = centerPt;
            circle.Axis = axis;
            circle.Diameter = diameter;
            circle.Commit();

            return circle;
        }
    }
}
