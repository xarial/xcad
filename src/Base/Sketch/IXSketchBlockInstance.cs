using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Sketch
{
    /// <summary>
    /// Represents an instance of <see cref="IXSketchBlockDefinition"/>
    /// </summary>
    public interface IXSketchBlockInstance : IXSketchEntity, IXFeature
    {
        /// <summary>
        /// Definition of this sketch block instance
        /// </summary>
        IXSketchBlockDefinition Definition { get; }

        /// <summary>
        /// Transformation of this sketch block instance regarding its defintion
        /// </summary>
        TransformMatrix Transform { get; }
    }
}
