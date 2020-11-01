using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.Geometry.Primitives
{
    /// <summary>
    /// Represents the 3D geometry of a primitive
    /// </summary>
    public interface IXPrimitive : IXTransaction
    {
        /// <summary>
        /// Bodies associated with this primitive
        /// </summary>
        IEnumerable<IXBody> Bodies { get; }
    }
}
