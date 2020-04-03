//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.UI.PopupWindow.Delegates;

namespace Xarial.XCad.UI
{
    public interface IXPopupWindow<TWindow> : IXCustomPanel<TWindow>
    {
        event PopupWindowClosedDelegate<TWindow> Closed;
        bool? ShowDialog();
        void Show();
    }
}
