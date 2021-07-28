using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.SolidWorks.UI.Toolkit
{
    internal interface ISessionAttachedItem : IDisposable
    {
        event Action<ISessionAttachedItem> Disposed;
    }
}
