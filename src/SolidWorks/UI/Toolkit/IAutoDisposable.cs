//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

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
