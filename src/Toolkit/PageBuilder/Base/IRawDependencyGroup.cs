//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using Xarial.XCad.UI.PropertyPage.Services;

namespace Xarial.XCad.Utils.PageBuilder.Base
{
    public interface IRawDependencyGroup
    {
        Dictionary<object, IBinding> TaggedBindings { get; }
        Dictionary<IBinding, Tuple<object[], IDependencyHandler>> DependenciesTags { get; }

        void RegisterBindingTag(IBinding binding, object tag);

        void RegisterDependency(IBinding binding, object[] dependentOnTags, IDependencyHandler dependencyHandler);
    }
}