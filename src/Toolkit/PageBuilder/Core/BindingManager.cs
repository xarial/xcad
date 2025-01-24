//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;

namespace Xarial.XCad.Utils.PageBuilder.Core
{
    public class BindingManager : IBindingManager
    {
        public IEnumerable<IBinding> Bindings { get; set; }
        public IDependencyManager Dependency { get; set; }
        public IMetadata[] Metadata { get; set; }

        public void Load(IXApplication app, IEnumerable<IBinding> bindings,
            IRawDependencyGroup dependencies, IMetadata[] metadata)
        {
            Bindings = bindings;
            Dependency = new DependencyManager();
            Metadata = metadata;

            Dependency.Init(app, dependencies);
        }
    }
}