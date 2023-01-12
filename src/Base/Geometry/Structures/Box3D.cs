//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Diagnostics;

namespace Xarial.XCad.Geometry.Structures
{
    /// <summary>
    /// Represents the 3D bounding box
    /// </summary>
    [DebuggerDisplay("{" + nameof(Width) + "} x {" + nameof(Height) + "} x {" + nameof(Length) + "}")]
    public class Box3D
    {
        /// <summary>
        /// Width of the bounding box relative to X axis
        /// </summary>
        public double Width { get; }

        /// <summary>
        /// Height of the bounding box relative to Y axis
        /// </summary>
        public double Height { get; }

        /// <summary>
        /// Length of the bounding box relative to Z axis
        /// </summary>
        public double Length { get; }

        /// <summary>
        /// Center point of the bounding box
        /// </summary>
        /// <remarks>This is the center point of the diagonal</remarks>
        public Point CenterPoint { get; }
        
        /// <summary>
        /// X axis of the bounding box
        /// </summary>
        public Vector AxisX { get; }

        /// <summary>
        /// Y axis of the bounding box
        /// </summary>
        public Vector AxisY { get; }

        /// <summary>
        /// Z axis of the bounding box
        /// </summary>
        public Vector AxisZ { get; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Box3D(double width, double height, double length, Point centerPt, Vector axisX, Vector axisY , Vector axisZ)
        {
            Width = width;
            Height = height;
            Length = length;

            CenterPoint = centerPt;
            AxisX = axisX;
            AxisY = axisY;
            AxisZ = axisZ;
        }

        /// <summary>
        /// Constructor based onend points of diagonal
        /// </summary>
        public Box3D(double minX, double minY, double minZ, double maxX, double maxY, double maxZ) 
        {
            Width = maxX - minX;
            Height = maxY - minY;
            Length = maxZ - minZ;

            CenterPoint = new Point((maxX + minX) / 2, (maxY + minY) / 2, (maxZ + minZ) / 2);

            AxisX = new Vector(1, 0, 0);
            AxisY = new Vector(0, 1, 0);
            AxisZ = new Vector(0, 0, 1);
        }
    }

    /// <summary>
    /// Additional methods for <see cref="Box3D"/>
    /// </summary>
    public static class Box3DExtension 
    {
        /// <summary>
        /// Left-Bottom-Front point of the bounding box
        /// </summary>
        public static Point GetLeftBottomFront(this Box3D box)
            => GetEndPoint(box, false, false, true);

        /// <summary>
        /// Left-Bottom-Back point of the bounding box
        /// </summary>
        public static Point GetLeftBottomBack(this Box3D box)
            => GetEndPoint(box, false, false, false);

        /// <summary>
        /// Left-Top-Front point of the bounding box
        /// </summary>
        public static Point GetLeftTopFront(this Box3D box)
            => GetEndPoint(box, false, true, true);

        /// <summary>
        /// Left-Top-Back point of the bounding box
        /// </summary>
        public static Point GetLeftTopBack(this Box3D box)
            => GetEndPoint(box, false, true, false);

        /// <summary>
        /// Right-Bottom-Front point of the bounding box
        /// </summary>
        public static Point GetRightBottomFront(this Box3D box)
            => GetEndPoint(box, true, false, true);

        /// <summary>
        /// Right-Bottom-Back point of the bounding box
        /// </summary>
        public static Point GetRightBottomBack(this Box3D box)
            => GetEndPoint(box, true, false, false);

        /// <summary>
        /// Right-Top-Front point of the bounding box
        /// </summary>
        public static Point GetRightTopFront(this Box3D box)
            => GetEndPoint(box, true, true, true);

        /// <summary>
        /// Right-Top-Back point of the bounding box
        /// </summary>
        public static Point GetRightTopBack(this Box3D box)
            => GetEndPoint(box, true, true, false);

        private static Point GetEndPoint(Box3D box, bool dirX, bool dirY, bool dirZ)
            => box.CenterPoint
                .Move(box.AxisX * (dirX ? 1 : -1), box.Width / 2)
                .Move(box.AxisY * (dirY ? 1 : -1), box.Height / 2)
                .Move(box.AxisZ * (dirZ ? 1 : -1), box.Length / 2);
    }
}