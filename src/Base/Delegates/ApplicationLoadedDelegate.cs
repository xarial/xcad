//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Delegates
{
    /// <summary>
    /// Delegate handler of <see cref="IXApplication.Loaded"/> event
    /// </summary>
    /// <param name="app">Extension</param>
    public delegate void ApplicationLoadedDelegate(IXApplication app);
}
