//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.UI.PropertyPage.Services
{
    /// <summary>
    /// Enables the providing of custom items
    /// </summary>
    public interface ICustomItemsProvider
    {
        /// <summary>
        /// Called when items need to be provided to the control
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="ctrl">Control to provide items for</param>
        /// <param name="dependencies">Control dependencies</param>
        /// <param name="parameter">User parameter</param>
        /// <returns>Items</returns>
        IEnumerable<object> ProvideItems(IXApplication app, IControl ctrl, IControl[] dependencies, object parameter);
    }
}
