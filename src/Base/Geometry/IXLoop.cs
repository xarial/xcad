using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Curves;
using Xarial.XCad.Geometry.Wires;

namespace Xarial.XCad.Geometry
{
    /// <summary>
    /// Represents the connected and closed list of <see cref="IXCurve"/>
    /// </summary>
    public interface IXLoop : IXSelObject, IXWireEntity
    {
        /// <summary>
        /// Connected and closed curves of this loop
        /// </summary>
        IXCurve[] Curves { get; set; }
    }
}
