//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry;

namespace Xarial.XCad.Features.CustomFeature.Delegates
{
    /// <summary>
    /// Assigns the custom color to the preview body
    /// </summary>
    /// <param name="body">Body to assign preview to</param>
    /// <param name="color">Color of the preview body</param>
    public delegate void AssignPreviewBodyColorDelegate(IXBody body, out System.Drawing.Color color);
}
