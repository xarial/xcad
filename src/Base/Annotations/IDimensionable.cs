//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Annotations
{
    /// <summary>
    /// Indicates that this object can have dimensions
    /// </summary>
    public interface IDimensionable
    {
        /// <summary>
        /// Dimensions repository
        /// </summary>
        IXDimensionRepository Dimensions { get; }
    }
}
