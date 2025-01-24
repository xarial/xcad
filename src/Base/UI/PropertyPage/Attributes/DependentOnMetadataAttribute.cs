//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Services;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    /// <summary>
    /// Indicates that the state of this control depends on the <see cref="IMetadata"/>
    /// </summary>
    public interface IDependentOnMetadataAttribute : IAttribute
    {
        /// <summary>
        /// Metadata tags
        /// </summary>
        object[] Dependencies { get; }

        /// <summary>
        /// Dependency handler resolving the control state
        /// </summary>
        IMetadataDependencyHandler DependencyHandler { get; }

        /// <summary>
        /// Parameter to pass to the <see cref="IMetadataDependencyHandler"/>
        /// </summary>
        object Parameter { get; }
    }

    /// <inheritdoc/>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class DependentOnMetadataAttribute : Attribute, IDependentOnMetadataAttribute
    {
        /// <inheritdoc/>
        public object[] Dependencies { get; }

        /// <inheritdoc/>
        public IMetadataDependencyHandler DependencyHandler { get; }

        /// <inheritdoc/>
        public object Parameter { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="dependencyHandler">Dependency handler of type <see cref="IMetadataDependencyHandler"/></param>
        /// <param name="dependencies">Dependencies names marked with <see cref="MetadataAttribute"/></param>
        public DependentOnMetadataAttribute(Type dependencyHandler, params object[] dependencies)
        {
            if (!typeof(IMetadataDependencyHandler).IsAssignableFrom(dependencyHandler))
            {
                throw new InvalidCastException($"{dependencyHandler.FullName} must be assignable from {typeof(IMetadataDependencyHandler).FullName}");
            }

            DependencyHandler = (IMetadataDependencyHandler)Activator.CreateInstance(dependencyHandler);

            Dependencies = dependencies;
        }
    }
}
