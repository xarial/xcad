using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Geometry.Structures
{
    public class Plane
    {
        public Point Point { get; set; }
        public Vector Normal { get; set; }
        public Vector Direction { get; set; }
        public Vector Reference => Normal.Cross(Direction);

        public Plane(Point point, Vector normal, Vector direction) 
        {
            Point = point;
            Normal = normal;
            Direction = direction;
        }
    }
}
