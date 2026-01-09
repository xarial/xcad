//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;

namespace Xarial.XCad.Utils.PageBuilder.Core
{
    /// <inheritdoc/>
    public class BindingManager : IBindingManager
    {
        /// <inheritdoc/>
        public IReadOnlyList<IBinding> Bindings { get; private set; }

        /// <inheritdoc/>
        public IDependencyManager Dependency { get; private set; }

        /// <inheritdoc/>
        public IMetadata[] Metadata { get; private set; }

        /// <inheritdoc/>
        public void Load(IXApplication app, IReadOnlyList<IBinding> bindings,
            IRawDependencyGroup dependencies, IMetadata[] metadata)
        {
            Bindings = bindings;
            Dependency = new DependencyManager();
            Metadata = metadata;

            Dependency.Init(app, dependencies);
        }
    }
}