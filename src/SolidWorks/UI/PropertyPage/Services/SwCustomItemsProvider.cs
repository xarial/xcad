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
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Services;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Services
{
    public abstract class SwCustomItemsProvider<TItem> : ICustomItemsProvider
    {
        IEnumerable<object> ICustomItemsProvider.ProvideItems(IXApplication app, IControl[] dependencies) 
            => ProvideItems((SwApplication)app, dependencies)?.Cast<object>();

        public virtual IEnumerable<TItem> ProvideItems(SwApplication app, IControl[] dependencies)
            => ProvideItems(app);

        public virtual IEnumerable<TItem> ProvideItems(SwApplication app)
            => Enumerable.Empty<TItem>();
    }
}
