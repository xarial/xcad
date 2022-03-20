using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Features;

namespace Xarial.XCad.Sketch
{
    /// <summary>
    /// Represents the defintion of <see cref="IXSketchBlockInstance"/>
    /// </summary>
    public interface IXSketchBlockDefinition : IXFeature
    {
        /// <summary>
        /// All instances of this sketch block defintion
        /// </summary>
        IEnumerable<IXSketchBlockInstance> Instances { get; }
    }
}
