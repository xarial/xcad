using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Extensions.Delegates
{
    /// <summary>
    /// Delegate for <see cref="IXExtension.StartupCompleted"/> event
    /// </summary>
    /// <param name="ext">Extension</param>
    public delegate void ExtensionStartupCompletedDelegate(IXExtension ext);
}
