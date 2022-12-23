//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Geometry.Evaluation
{
    /// <summary>
    /// Represents the bounding box of the geometrical object
    /// </summary>
    public interface IXBoundingBox : IEvaluation
    {
        /// <summary>
        /// True to automatically find the best orientation to fit
        /// </summary>
        bool BestFit { get; set; }

        /// <summary>
        /// Bounding box data
        /// </summary>
        Box3D Box { get; }
    }

    /// <summary>
    /// Bounding box specific to the assembly
    /// </summary>
    public interface IXAssemblyBoundingBox : IXBoundingBox, IAssemblyEvaluation
    {
    }
}
