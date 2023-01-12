//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.Utils.PageBuilder.Base
{
    public interface IBindingManager
    {
        IEnumerable<IBinding> Bindings { get; }
        IDependencyManager Dependency { get; }
        IMetadata[] Metadata { get; }

        void Load(IXApplication app, IEnumerable<IBinding> bindings, IRawDependencyGroup dependencies, IMetadata[] metadata);
    }
}