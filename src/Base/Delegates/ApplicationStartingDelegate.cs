//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xarial.XCad.Delegates
{
    /// <summary>
    /// Delegate for <see cref="IXApplication.Starting"/> event
    /// </summary>
    /// <param name="sender">Application which is starting</param>
    /// <param name="process">Application process</param>
    public delegate void ApplicationStartingDelegate(IXApplication sender, Process process);
}
