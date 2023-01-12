//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry;

namespace Xarial.XCad.Features
{
    /// <summary>
    /// Represents the feature which holds the dumb body
    /// </summary>
    public interface IXDumbBody : IXFeature
    {
        /// <summary>
        /// Body geometry of the feature
        /// </summary>
        IXBody BaseBody { get; set; }
    }
}
