using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Geometry.Primitives
{
    /// <summary>
    /// Represents the knit premitive
    /// </summary>
    public interface IXKnit : IXPrimitive
    {
        /// <summary>
        /// Faces representing this knit
        /// </summary>
        IXFace[] Faces { get; set; }
    }
}
