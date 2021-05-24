//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace Xarial.XCad.Geometry.Structures
{
    /// <summary>
    /// Represents the 3D bounding box
    /// </summary>
    public class Box3D
    {
        /// <summary>
        /// Width of the bounding box relative to X axis
        /// </summary>
        public double Width { get; }

        /// <summary>
        /// Width of the bounding box relative to Y axis
        /// </summary>
        public double Height { get; }

        /// <summary>
        /// Width of the bounding box relative to Z axis
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