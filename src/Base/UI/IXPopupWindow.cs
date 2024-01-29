//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.UI.Enums;
using Xarial.XCad.UI.PopupWindow.Delegates;

namespace Xarial.XCad.UI
{
    /// <summary>
    /// Represents the popup window with custom hosted control
    /// </summary>
    /// <typeparam name="TWindow">Window to host</typeparam>
    public interface IXPopupWindow<TWindow> : IXCustomPanel<TWindow>
    {
        /// <summary>
        /// Event raised when popup is about to be closed
        /// </summary>
        event PopupWindowClosedDelegate<TWindow> Closed;
        
        /// <summary>
        /// Shows window in modal state
        /// </summary>
        /// <param name="dock">Dock of hte window</param>
        /// <returns>True if user clicks Yes, False if user clicks No, Null for Cancel</returns>
        bool? ShowDialog(PopupDock_e dock = PopupDock_e.Center);

        /// <summary>
        /// Shows window in modeless state
        /// </summary>
        /// <param name="dock">Dock of the window</param>
        void Show(PopupDock_e dock = PopupDock_e.Center);
    }
}
