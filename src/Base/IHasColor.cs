//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Xarial.XCad
{
    /// <summary>
    /// Identifies the visual object which can have color
    /// </summary>
    public interface IHasColor : IXObject
    {
        /// <summary>
        /// Color of visual object
        /// </summary>
        Color? Color { get; set; }
    }
}
