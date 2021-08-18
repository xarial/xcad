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
