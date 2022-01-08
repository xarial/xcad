//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace Xarial.XCad.Geometry.Structures
{
    /// <summary>
    /// Principal moment of inertia used in <see cref="IXMassProperty"/>
    /// </summary>
    public class PrincipalMomentOfInertia
    {
        /// <summary>
        /// Px (mass * square length)
        /// </summary>
        public double Px { get; }

        /// <summary>
        /// Py (mass * square length)
        /// </summary>
        public double Py { get; }

        /// <summary>
        /// Pz (mass * square length)
        /// </summary>
        public double Pz { get; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public PrincipalMomentOfInertia(double px, double py, double pz)
        {
            Px = px;
            Py = py;
            Pz = pz;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p">Array of 3 doubles (Px, Py, Pz)</param>
        public PrincipalMomentOfInertia(double[] p) : this(p[0], p[1], p[2])
        {
        }
    }
}
