using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base;

namespace Xarial.XCad.Geometry.Wires
{
    /// <summary>
    /// Represents the common entity for <see cref="IXPoint"/> and <see cref="IXSegment"/>
    /// </summary>
    public interface IXWireEntity : IXTransaction, IXObject
    {
    }
}
