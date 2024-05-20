//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Services;
using Xarial.XCad.Utils.PageBuilder.Base;

namespace Xarial.XCad.Utils.PageBuilder.Core
{
    public class RawDependencyGroup : IRawDependencyGroup
    {
        public IReadOnlyDictionary<object, IBinding> TaggedBindings => m_TaggedBindings;
        public IReadOnlyDictionary<IBinding, DependencyInfo> DependenciesTags => m_DependenciesTags;
        public IReadOnlyDictionary<IControl, MetadataDependencyInfo> MetadataDependencies => m_MetadataDependencies;

        private readonly Dictionary<object, IBinding> m_TaggedBindings;
        private readonly Dictionary<IBinding, DependencyInfo> m_DependenciesTags;
        private readonly Dictionary<IControl, MetadataDependencyInfo> m_MetadataDependencies;

        public RawDependencyGroup()
        {
            m_TaggedBindings = new Dictionary<object, IBinding>();
            m_DependenciesTags = new Dictionary<IBinding, DependencyInfo>();
            m_MetadataDependencies = new Dictionary<IControl, MetadataDependencyInfo>();
        }

        public void RegisterBindingTag(IBinding binding, object tag)
        {
            if (!TaggedBindings.ContainsKey(tag))
            {
                m_TaggedBindings.Add(tag, binding);
            }
            else
            {
                throw new Exception("Tag is not unique");
            }
        }

        public void RegisterDependency(IBinding binding, DependencyInfo info)
            => m_DependenciesTags.Add(binding, info);

        public void RegisterMetadataDependency(IControl ctrl, MetadataDependencyInfo info)
            => m_MetadataDependencies.Add(ctrl, info);
    }
}