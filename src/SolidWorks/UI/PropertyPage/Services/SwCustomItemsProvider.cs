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
using Xarial.XCad.UI.PropertyPage.Services;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Services
{
    public abstract class SwCustomItemsProvider<TItem> : ICustomItemsProvider
    {
        IEnumerable<object> ICustomItemsProvider.ProvideItems(IXApplication app) => ProvideItems((SwApplication)app).Cast<object>();

        public abstract IEnumerable<TItem> ProvideItems(SwApplication app);
    }
}
