//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;

namespace Xarial.XCad.Utils.PageBuilder.Base
{
    public interface IBindingManager
    {
        IEnumerable<IBinding> Bindings { get; }
        IDependencyManager Dependency { get; }

        void Load(IXApplication app, IEnumerable<IBinding> bindings, IRawDependencyGroup dependencies);
    }
}