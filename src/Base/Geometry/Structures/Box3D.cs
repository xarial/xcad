//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace Xarial.XCad.Geometry.Structures
{
    public class Box3D
    {
        public Point LeftBottomFront { get; set; }
        public Point LeftBottomBack { get; set; }
        public Point LeftTopFront { get; set; }
        public Point LeftTopBack { get; set; }
        public Point RightBottomFront { get; set; }
        public Point RightBottomBack { get; set; }
        public Point RightTopFront { get; set; }
        public Point RightTopBack { get; set; }

        public Point[] Points => new Point[]
        {
            LeftBottomFront,
            LeftBottomBack,
            LeftTopFront,
            LeftTopBack,
            RightBottomFront,
            RightBottomBack,
            RightTopFront,
            RightTopBack
        };

        public Box3D(double minX, double minY, double minZ, double maxX, double maxY, double maxZ)
        {
            LeftBottomBack = new Point(minX, minY, minZ);
            LeftBottomFront = new Point(minX, minY, maxZ);
            LeftTopFront = new Point(minX, maxY, maxZ);
            LeftTopBack = new Point(minX, maxY, minZ);

            RightBottomBack = new Point(maxX, minY, minZ);
            RightBottomFront = new Point(maxX, minY, maxZ);
            RightTopFront = new Point(maxX, maxY, maxZ);
            RightTopBack = new Point(maxX, maxY, minZ);
        }
    }
}