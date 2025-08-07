using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Enums;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Structures
{
    /// <summary>
    /// Bend information
    /// </summary>
    public class BendInfo
    {
        /// <summary>
        /// Bend direction
        /// </summary>
        public BendDirection_e Direction { get; }

        /// <summary>
        /// Bend angle
        /// </summary>
        public double Angle { get; }

        /// <summary>
        /// Bend radius
        /// </summary>
        public double Radius { get; }

        /// <summary>
        /// Bend line
        /// </summary>
        public Line Line { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dir">Bend direction</param>
        /// <param name="angle">Bend angle</param>
        /// <param name="radius">Bend radius</param>
        /// <param name="line">Bend line</param>
        public BendInfo(BendDirection_e dir, double angle, double radius, Line line)
        {
            Direction = dir;
            Angle = angle;
            Radius = radius;
            Line = line;
        }
    }
}
