using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Extensions.Delegates
{
    /// <summary>
    /// Delegate for <see cref="IXExtension.Connect"/> event
    /// </summary>
    /// <param name="ext">Extension</param>
    public delegate void ExtensionConnectDelegate(IXExtension ext);
}
