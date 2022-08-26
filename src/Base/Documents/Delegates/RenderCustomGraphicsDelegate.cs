//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Documents.Delegates
{
    /// <summary>
    /// Delegate of <see cref="IXModelView.RenderCustomGraphics"/> event
    /// </summary>
    /// <param name="sender">Model view which sends this event</param>
    /// <param name="context">Custom graphics context</param>
    public delegate bool RenderCustomGraphicsDelegate(IXModelView sender, IXCustomGraphicsContext context);
}
