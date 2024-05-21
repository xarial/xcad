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
    /// <inheritdoc/>
    public class RawDependencyGroup : IRawDependencyGroup
    {
        /// <inheritdoc/>
        public IReadOnlyDictionary<object, IBinding> TaggedBindings => m_TaggedBindings;

        /// <inheritdoc/>
        public IReadOnlyList<DependencyInfo> DependenciesTags => m_DependenciesTags;

        /// <inheritdoc/>
        public IReadOnlyList<MetadataDependencyInfo> MetadataDependencies => m_MetadataDependencies;

        private readonly Dictionary<object, IBinding> m_TaggedBindings;
        private readonly List<DependencyInfo> m_DependenciesTags;
        private readonly List<MetadataDependencyInfo> m_MetadataDependencies;

        /// <summary>
        /// Default constructor
        /// </summary>
        public RawDependencyGroup()
        {
            m_TaggedBindings = new Dictionary<object, IBinding>();
            m_DependenciesTags = new List<DependencyInfo>();
            m_MetadataDependencies = new List<MetadataDependencyInfo>();
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public void RegisterDependency(DependencyInfo info)
            => m_DependenciesTags.Add(info);

        /// <inheritdoc/>
        public void RegisterMetadataDependency(MetadataDependencyInfo info)
            => m_MetadataDependencies.Add(info);
    }
}