//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xarial.XCad.Geometry.Structures
{
    /// <summary>
    /// Represents the 2D rectangle
    /// </summary>
    [DebuggerDisplay("{" + nameof(Width) + "} x {" + nameof(Height) + "}")]
    public class Rect2D
    {
        /// <summary>
        /// Width of the rectangle relative to X axis
        /// </summary>
        public double Width { get; }

        /// <summary>
        /// Height of the rectangle relative to Y axis
        /// </summary>
        public double Height { get; }

        /// <summary>
        /// Center point of the rectangle box
        /// </summary>
        /// <remarks>This is the center point of the diagonal</remarks>
        public Point CenterPoint { get; }

        /// <summary>
        /// X axis of the rectangle
        /// </summary>
        public Vector AxisX { get; }

        /// <summary>
        /// Y axis of the reactangle
        /// </summary>
        public Vector AxisY { get; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Rect2D(double width, double height, Point centerPt, Vector axisX, Vector axisY)
        {
            Width = width;
            Height = height;

            CenterPoint = centerPt;
            AxisX = axisX;
            AxisY = axisY;
        }

        /// <summary>
        /// Constructor with default axes
        /// </summary>
        public Rect2D(double width, double height, Point centerPt)
            : this(width, height, centerPt, new Vector(1, 0, 0), new Vector(0, 1, 0))
        {
        }
    }

    /// <summary>
    /// Additional methods for <see cref="Rect2D"/>
    /// </summary>
    public static class Rect2DExtension
    {
        /// <summary>
        /// Left-Bottom point of the rectangle
        /// </summary>
        public static Point GetLeftBottom(this Rect2D rect)
            => GetEndPoint(rect, false, false);

        /// <summary>
        /// Left-Top point of the rectangle
        /// </summary>
        public static Point GetLeftTop(this Rect2D rect)
            => GetEndPoint(rect, false, true);

        /// <summary>
        /// Right-Bottom point of the rectangle
        /// </summary>
        public static Point GetRightBottom(this Rect2D rect)
            => GetEndPoint(rect, true, false);

        /// <summary>
        /// Right-Top point of the rectangle
        /// </summary>
        public static Point GetRightTop(this Rect2D box)
            => GetEndPoint(box, true, true);

        private static Point GetEndPoint(Rect2D box, bool dirX, bool dirY)
            => box.CenterPoint
                .Move(box.AxisX * (dirX ? 1 : -1), box.Width / 2)
                .Move(box.AxisY * (dirY ? 1 : -1), box.Height / 2);
    }
}
