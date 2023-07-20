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

namespace Xarial.XCad.Geometry.Evaluation
{
    /// <summary>
    /// Represents the measure utility
    /// </summary>
    public interface IXMeasure : IEvaluation
    {
        /// <summary>
        /// Angle
        /// </summary>
        double Angle { get; }
        
        /// <summary>
        /// Area
        /// </summary>
        double Area { get; }
        
        /// <summary>
        /// Diameter
        /// </summary>
        double Diameter { get; }
        
        /// <summary>
        /// Distance
        /// </summary>
        double Distance { get; }
        
        /// <summary>
        /// Length
        /// </summary>
        double Length { get; }

        /// <summary>
        /// Perimeter
        /// </summary>
        double Perimeter { get; }

        /// <summary>
        /// Scope of entities to measure
        /// </summary>
        /// <remarks>Specify null to measure selected entities</remarks>
        new IXEntity[] Scope { get; set; }
    }
}
