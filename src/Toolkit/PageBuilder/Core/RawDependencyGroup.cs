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
        public IReadOnlyDictionary<IBinding, Tuple<object[], IDependencyHandler>> DependenciesTags => m_DependenciesTags;
        public IReadOnlyDictionary<IControl, Tuple<IMetadata[], IMetadataDependencyHandler>> MetadataDependencies => m_MetadataDependencies;

        private readonly Dictionary<object, IBinding> m_TaggedBindings;
        private readonly Dictionary<IBinding, Tuple<object[], IDependencyHandler>> m_DependenciesTags;
        private readonly Dictionary<IControl, Tuple<IMetadata[], IMetadataDependencyHandler>> m_MetadataDependencies;

        public RawDependencyGroup()
        {
            m_TaggedBindings = new Dictionary<object, IBinding>();
            m_DependenciesTags = new Dictionary<IBinding, Tuple<object[], IDependencyHandler>>();
            m_MetadataDependencies = new Dictionary<IControl, Tuple<IMetadata[], IMetadataDependencyHandler>>();
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

        public void RegisterDependency(IBinding binding, object[] dependentOnTags, IDependencyHandler dependencyHandler)
        {
            m_DependenciesTags.Add(binding, new Tuple<object[], IDependencyHandler>(dependentOnTags, dependencyHandler));
        }

        public void RegisterMetadataDependency(IControl ctrl, IMetadata[] metadata, IMetadataDependencyHandler dependencyHandler)
        {
            m_MetadataDependencies.Add(ctrl, new Tuple<IMetadata[], IMetadataDependencyHandler>(metadata, dependencyHandler));
        }
    }
}