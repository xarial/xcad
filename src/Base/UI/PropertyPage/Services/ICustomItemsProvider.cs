//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
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
        /// <param name="dependencies">Control dependencies</param>
        /// <returns>Items</returns>
        IEnumerable<object> ProvideItems(IXApplication app, IControl[] dependencies);
    }

    /// <summary>
    /// Type safe version of <see cref="ICustomItemsProvider"/>
    /// </summary>
    /// <typeparam name="TItem">Item type</typeparam>
    public abstract class CustomItemsProvider<TItem> : ICustomItemsProvider
    {
        IEnumerable<object> ICustomItemsProvider.ProvideItems(IXApplication app, IControl[] dependencies) => ProvideItems(app, dependencies).Cast<object>();

        public abstract IEnumerable<TItem> ProvideItems(IXApplication app, IControl[] dependencies);
    }
}
