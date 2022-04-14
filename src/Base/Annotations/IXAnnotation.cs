using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Annotations
{
    /// <summary>
    /// Represents the base interface of annotation (e.g.<see cref="IXDimension"/>, <see cref="IXNote"/> etc.)
    /// </summary>
    public interface IXAnnotation : IXSelObject
    {
        /// <summary>
        /// Position of this annotation
        /// </summary>
        Point Position { get; set; }
    }
}
