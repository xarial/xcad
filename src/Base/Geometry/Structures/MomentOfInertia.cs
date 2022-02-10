//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace Xarial.XCad.Geometry.Structures
{
    /// <summary>
    /// Moment of intertia used in <see cref="IXMassProperty"/>
    /// </summary>
    public class MomentOfInertia
    {
        /// <summary>
        /// X moment of intertia (mass * square lemgth )
        /// </summary>
        public Vector Lx { get; }

        /// <summary>
        /// Y moment of intertia (mass * square lemgth )
        /// </summary>
        public Vector Ly { get; }

        /// <summary>
        /// Z moment of intertia (mass * square lemgth )
        /// </summary>
        public Vector Lz { get; }

        /// <summary>
        /// Default construcotor
        /// </summary>
        public MomentOfInertia(Vector lx, Vector ly, Vector lz)
        {
            Lx = lx;
            Ly = ly;
            Lz = lz;
        }
    }
}
