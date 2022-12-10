//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

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
        /// Connected and closed segments of this loop
        /// </summary>
        IXSegment[] Segments { get; set; }
    }
}
