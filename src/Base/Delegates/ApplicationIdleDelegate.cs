//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Delegates
{
    /// <summary>
    /// Delegate of <see cref="IXApplication.Idle"/> event
    /// </summary>
    /// <param name="app">Pointer to the application</param>
    public delegate void ApplicationIdleDelegate(IXApplication app);
}
