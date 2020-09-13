//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using Xarial.XCad.UI.PropertyPage.Services;
using Xarial.XCad.Utils.PageBuilder.Base;

namespace Xarial.XCad.Utils.PageBuilder.Core
{
    public class RawDependencyGroup : IRawDependencyGroup
    {
        public Dictionary<object, IBinding> TaggedBindings { get; private set; }
        public Dictionary<IBinding, Tuple<object[], IDependencyHandler>> DependenciesTags { get; private set; }

        public RawDependencyGroup()
        {
            TaggedBindings = new Dictionary<object, IBinding>();
            DependenciesTags = new Dictionary<IBinding, Tuple<object[], IDependencyHandler>>();
        }

        public void RegisterBindingTag(IBinding binding, object tag)
        {
            if (!TaggedBindings.ContainsKey(tag))
            {
                TaggedBindings.Add(tag, binding);
            }
            else
            {
                throw new Exception("Tag is not unique");
            }
        }

        public void RegisterDependency(IBinding binding, object[] dependentOnTags, IDependencyHandler dependencyHandler)
        {
            DependenciesTags.Add(binding, new Tuple<object[], IDependencyHandler>(dependentOnTags, dependencyHandler));
        }
    }
}