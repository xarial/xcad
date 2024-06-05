//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Sketch
{
    /// <summary>
    /// Represents the defintion of <see cref="IXSketchBlockInstance"/>
    /// </summary>
    public interface IXSketchBlockDefinition : IXFeature
    {
        /// <summary>
        /// Insertion point of the sketch block definition
        /// </summary>
        Point InsertionPoint { get; }

        /// <summary>
        /// All instances of this sketch block defintion
        /// </summary>
        IEnumerable<IXSketchBlockInstance> Instances { get; }

        /// <summary>
        /// Entities of this sketch block definition
        /// </summary>
        IXSketchEntityRepository Entities { get; }
    }
}
