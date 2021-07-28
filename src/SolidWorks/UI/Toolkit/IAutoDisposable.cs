using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.SolidWorks.UI.Toolkit
{
    /// <summary>
    /// Indicates that this item will be disposed when main add-in is unloaded
    /// </summary>
    internal interface IAutoDisposable : IDisposable
    {
        event Action<IAutoDisposable> Disposed;
    }
}
