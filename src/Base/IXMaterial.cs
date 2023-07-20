//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base;

namespace Xarial.XCad
{
    /// <summary>
    /// Represents the material
    /// </summary>
    public interface IXMaterial : IXTransaction
    {
        /// <summary>
        /// Material database
        /// </summary>
        IXMaterialsDatabase Database { get; }

        /// <summary>
        /// Category of the material
        /// </summary>
        string Category { get; }

        /// <summary>
        /// Elastic modulus
        /// </summary>
        double ElasticModulus { get; }

        /// <summary>
        /// Poisson's ratio
        /// </summary>
        double PoissonRatio { get; }

        /// <summary>
        /// Shear modulus
        /// </summary>
        double ShearModulus { get; }

        /// <summary>
        /// Thermal expansion coefficient
        /// </summary>
        double ThermalExpansionCoefficient { get; }

        /// <summary>
        /// Mass density
        /// </summary>
        double MassDensity { get; }

        /// <summary>
        /// Thermal conductivity
        /// </summary>
        double ThermalConductivity { get; }

        /// <summary>
        /// Specific heat
        /// </summary>
        double SpecificHeat { get; }

        /// <summary>
        /// Tensile strength
        /// </summary>
        double TensileStrength { get; }

        /// <summary>
        /// Yield strength
        /// </summary>
        double YieldStrength { get; }

        /// <summary>
        /// Hardening factor (0.0-1.0; 0.0=isotropic; 1.0=kinematic)
        /// </summary>
        double HardeningFactor { get; }

        /// <summary>
        /// Name of the material
        /// </summary>
        string Name { get; }

        //TODO: Add support for custom properties
    }
}
