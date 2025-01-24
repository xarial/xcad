//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Extensions.Delegates
{
    /// <summary>
    /// Delegate for <see cref="IXExtension.Disconnect"/> event
    /// </summary>
    /// <param name="ext">Extension</param>
    public delegate void ExtensionDisconnectDelegate(IXExtension ext);
}
