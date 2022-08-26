//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.SolidWorks.Services
{
    /// <summary>
    /// Service provides tolerance values to perform operations in the <see cref="XCad.Geometry.IXMemoryGeometryBuilder"/>
    /// </summary>
    public interface IMemoryGeometryBuilderToleranceProvider
    {
        /// <summary>
        /// Tolerance for bodies knitting operations
        /// </summary>
        /// <remarks>Used in IModeler::CreateBodiesFromSheets2</remarks>
        double Knitting { get; }

        /// <summary>
        /// Tolerance for trimming the sheet bodies
        /// </summary>
        /// <remarks>Used in ISurface::CreateTrimmedSheet5</remarks>
        double Trimming { get; }

        /// <summary>
        /// Tolerance for comparing numbers
        /// </summary>
        double Numeric { get; }

        /// <summary>
        /// Tolerance for comparing lengths
        /// </summary>
        double Length { get; }

        /// <summary>
        /// Tolerance for comparing angles
        /// </summary>
        double Angle { get; }
    }

    internal class DefaultMemoryGeometryBuilderToleranceProvider : IMemoryGeometryBuilderToleranceProvider
    {
        public double Knitting => 1E-16;
        public double Trimming => 1E-05;

        public double Numeric => 1E-12;
        public double Length => 1E-8;
        public double Angle => 1E-6;
    }
}
