//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        /// <summary>
        /// Unions two rectangles
        /// </summary>
        /// <param name="rect">This rectangle</param>
        /// <param name="otherRect">Other rectangle</param>
        /// <returns>Union rectangle</returns>
        public static Rect2D Union(this Rect2D rect, Rect2D otherRect)
        {
            var thisLeftBottom = rect.GetLeftBottom();
            var thisRightTop = rect.GetRightTop();

            var otherLeftBottom = otherRect.GetLeftBottom();
            var otherRightTop = otherRect.GetRightTop();

            //NOTE: rectangle can have custom axis which can result in the top right point to be lover than bottom left
            //in order to consider this finding the min and max among all points

            var minX = Min(thisLeftBottom.X, otherLeftBottom.X, thisRightTop.X, otherRightTop.X);
            var maxX = Max(thisLeftBottom.X, otherLeftBottom.X, thisRightTop.X, otherRightTop.X);
            var minY = Min(thisLeftBottom.Y, otherLeftBottom.Y, thisRightTop.Y, otherRightTop.Y);
            var maxY = Max(thisLeftBottom.Y, otherLeftBottom.Y, thisRightTop.Y, otherRightTop.Y);
            
            return new Rect2D(maxX - minX, maxY - minY, new Point((minX + maxX) / 2, (minY + maxY) / 2, 0));
        }

        /// <summary>
        /// Checks if 2 rectangles overlap
        /// </summary>
        /// <param name="thisRect"></param>
        /// <param name="otherRect"></param>
        /// <returns>True if rectangles overlap</returns>
        public static bool Intersects(this Rect2D thisRect, Rect2D otherRect)
        {
            var thisBottomLeft = thisRect.GetLeftBottom();
            var thisTopRight = thisRect.GetRightTop();

            var otherBottomLeft = otherRect.GetLeftBottom();
            var otherTopRight = otherRect.GetRightTop();

            return thisBottomLeft.X <= otherTopRight.X
                && thisTopRight.X >= otherBottomLeft.X
                && thisBottomLeft.Y <= otherTopRight.Y
                && thisTopRight.Y >= otherBottomLeft.Y;
        }

        private static double Min(params double[] vals) 
        {
            if (vals?.Any() != true)
            {
                throw new ArgumentException(nameof(vals));
            }

            var min = double.MaxValue;

            foreach (var val in vals) 
            {
                if (val < min) 
                {
                    min = val;
                }
            }

            return min;
        }

        private static double Max(params double[] vals)
        {
            if (vals?.Any() != true)
            {
                throw new ArgumentException(nameof(vals));
            }

            var max = double.MinValue;

            foreach (var val in vals)
            {
                if (val > max)
                {
                    max = val;
                }
            }

            return max;
        }

        private static Point GetEndPoint(Rect2D box, bool dirX, bool dirY)
            => box.CenterPoint
            .Move(box.AxisX * (dirX ? 1 : -1), box.Width / 2)
            .Move(box.AxisY * (dirY ? 1 : -1), box.Height / 2);
    }
}
