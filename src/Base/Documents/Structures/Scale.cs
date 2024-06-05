//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xarial.XCad.Documents.Structures
{
    /// <summary>
    /// Represents the scale
    /// </summary>
    [DebuggerDisplay("{" + nameof(Numerator) + "}" + ":{" + nameof(Denominator) + "}")]
    public class Scale
    {
        /// <summary>
        /// Numerator of this scale
        /// </summary>
        public double Numerator { get; }

        /// <summary>
        /// Denominator of this scale
        /// </summary>
        public double Denominator { get; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Scale(double numerator, double denominator)
        {
            Numerator = numerator;
            Denominator = denominator;
        }
    }

    /// <summary>
    /// Extensions of <see cref="Scale"/>
    /// </summary>
    public static class ScaleExtension 
    {
        /// <summary>
        /// Returns the scale as double value
        /// </summary>
        /// <param name="scale">Scale</param>
        /// <returns>Scale</returns>
        public static double AsDouble(this Scale scale)
            => scale.Numerator / scale.Denominator;
    }
}
