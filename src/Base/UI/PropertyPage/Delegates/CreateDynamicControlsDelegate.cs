//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.UI.PropertyPage.Delegates
{
    /// <summary>
    /// Handler of dynamic controls in the proeprty page
    /// </summary>
    /// <param name="tag">Control tag</param>
    /// <returns>Dynamic control descriptors</returns>
    public delegate IControlDescriptor[] CreateDynamicControlsDelegate(object tag);
}
