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

namespace Xarial.XCad.Utils.PageBuilder.Base
{
    /// <summary>
    /// Dependency of control
    /// </summary>
    public class DependencyInfo
    {
        /// <summary>
        /// Tags of dependencies
        /// </summary>
        public object[] DependentOnTags { get; }

        /// <summary>
        /// User parameter
        /// </summary>
        public object Parameter { get; }

        /// <summary>
        /// Handler to resolve dependency
        /// </summary>
        public IDependencyHandler DependencyHandler { get; }

        /// <param name="dependentOnTags">Tags of dependencies</param>
        /// <param name="parameter">User parameter</param>
        /// <param name="dependencyHandler">Handler to resolve dependency</param>
        public DependencyInfo(object[] dependentOnTags, object parameter, IDependencyHandler dependencyHandler)
        {
            DependentOnTags = dependentOnTags;
            Parameter = parameter;
            DependencyHandler = dependencyHandler;
        }
    }

    /// <summary>
    /// Dependency of metadata
    /// </summary>
    public class MetadataDependencyInfo
    {
        /// <summary>
        /// Metadata
        /// </summary>
        public IMetadata[] Metadata { get; }

        /// <summary>
        /// User parameter
        /// </summary>
        public object Parameter { get; }

        /// <summary>
        /// Handler to resolve dependency
        /// </summary>
        public IMetadataDependencyHandler DependencyHandler { get; }

        /// <param name="metadata">Metadata</param>
        /// <param name="parameter">User parameter</param>
        /// <param name="dependencyHandler">Handler to resolve dependency</param>
        public MetadataDependencyInfo(IMetadata[] metadata, object parameter, IMetadataDependencyHandler dependencyHandler) 
        {
            Metadata = metadata;
            Parameter = parameter;
            DependencyHandler = dependencyHandler;
        }
    }

    /// <summary>
    /// Manages dependencies
    /// </summary>
    public interface IRawDependencyGroup
    {
        /// <summary>
        /// Binding which have contorl tags
        /// </summary>
        IReadOnlyDictionary<object, IBinding> TaggedBindings { get; }

        /// <summary>
        /// Dependencies of controls
        /// </summary>
        IReadOnlyDictionary<IBinding, DependencyInfo> DependenciesTags { get; }

        /// <summary>
        /// Dependencies of metadata
        /// </summary>
        IReadOnlyDictionary<IControl, MetadataDependencyInfo> MetadataDependencies { get; }

        /// <summary>
        /// Registers binding with a tag
        /// </summary>
        /// <param name="binding">Binding</param>
        /// <param name="tag">Tag of this binding</param>
        void RegisterBindingTag(IBinding binding, object tag);

        /// <summary>
        /// Registers dependency of control
        /// </summary>
        /// <param name="binding">Binding</param>
        /// <param name="info">Dependency info</param>
        void RegisterDependency(IBinding binding, DependencyInfo info);

        /// <summary>
        /// Registers dependency of metadata
        /// </summary>
        /// <param name="ctrl">Source control</param>
        /// <param name="info">Metadata dependency info</param>
        void RegisterMetadataDependency(IControl ctrl, MetadataDependencyInfo info);
    }
}