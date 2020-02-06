//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xarial.XCad.UI.PropertyPage.Services
{
    public interface ICustomItemsProvider
    {
        IEnumerable<object> ProvideItems(IXApplication app);
    }

    public abstract class CustomItemsProvider<TItem> : ICustomItemsProvider
    {
        IEnumerable<object> ICustomItemsProvider.ProvideItems(IXApplication app) => ProvideItems(app).Cast<object>();

        public abstract IEnumerable<TItem> ProvideItems(IXApplication app);
    }
}
