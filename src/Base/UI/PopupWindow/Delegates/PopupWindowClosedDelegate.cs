//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.UI.PopupWindow.Delegates
{
    /// <summary>
    /// Delegate of <see cref="IXPopupWindow{TWindow}.Closed"/> event
    /// </summary>
    /// <typeparam name="TWindow">Specific type of the window</typeparam>
    /// <param name="sender">Window sender</param>
    public delegate void PopupWindowClosedDelegate<TWindow>(IXPopupWindow<TWindow> sender);
}
