//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Base
{
    /// <summary>
    /// This interfaces represents the geometrical evaluation features
    /// </summary>
    public interface IEvaluation : IXTransaction, IDisposable
    {
        /// <summary>
        /// Evaluates geometry relative to the specified transformation matrix
        /// </summary>
        TransformMatrix RelativeTo { get; set; }

        /// <summary>
        /// True to use user specific units, false to use system units
        /// </summary>
        bool UserUnits { get; set; }

        /// <summary>
        /// True to only use visible bodies or components
        /// </summary>
        bool VisibleOnly { get; set; }

        /// <summary>
        /// Bodies to include into the evaluation
        /// </summary>
        IXBody[] Scope { get; set; }

        /// <summary>
        /// True to calculate precise data, false to calculate approximate data
        /// </summary>
        bool Precise { get; set; }
    }

    /// <summary>
    /// Assembly specific geometrical evaluation feature
    /// </summary>
    public interface IAssemblyEvaluation : IEvaluation
    {
        /// <summary>
        /// Components to include into the evaluation
        /// </summary>
        new IXComponent[] Scope { get; set; }
    }
}
