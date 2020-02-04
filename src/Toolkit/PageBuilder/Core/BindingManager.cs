//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using Xarial.XCad.Utils.PageBuilder.Base;

namespace Xarial.XCad.Utils.PageBuilder.Core
{
    public class BindingManager : IBindingManager
    {
        public IEnumerable<IBinding> Bindings { get; private set; }
        public IDependencyManager Dependency { get; private set; }

        public void Load(IEnumerable<IBinding> bindings, IRawDependencyGroup dependencies)
        {
            Bindings = bindings;
            Dependency = new DependencyManager();
            Dependency.Init(dependencies);
        }
    }
}