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
