//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
using System.ComponentModel;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Services;
using Xarial.XCad.UI.PropertyPage.Structures;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    /// <summary>
    /// Represents the control which has items source
    /// </summary>
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class ItemsSourceControlAttribute : Attribute, IDependentOnAttribute, IHasMetadataAttribute
    {
        /// <inheritdoc/>
        public ICustomItemsProvider CustomItemsProvider { get; }

        /// <inheritdoc/>
        public IDependencyHandler DependencyHandler { get; }

        /// <inheritdoc/>
        public object[] Dependencies { get; }

        /// <inheritdoc/>
        public object[] StaticItems { get; }

        /// <inheritdoc/>
        public object LinkedMetadataTag => ItemsSource;

        /// <summary>
        /// Tag of the metadata property (decorated with <see cref="MetadataAttribute"/>) which contains the items source for this combo box
        /// </summary>
        /// <remarks>Target property must be <see cref="IEnumerable"/> </remarks>
        public object ItemsSource { get; set; }

        /// <summary>
        /// Property path of the item name to display in the source control
        /// </summary>
        public string DisplayMemberPath { get; set; }

        /// <inheritdoc/>
        public bool HasMetadata => ItemsSource != null;

        /// <inheritdoc/>
        public object Parameter { get; set; }

        /// <summary>
        /// Creates a combo box with custom items provider
        /// </summary>
        /// <param name="customItemsProviderType">Type of the <see cref="ICustomItemsProvider"/> which creates items source</param>
        /// <param name="dependencies">Optional control dependencies</param>
        protected ItemsSourceControlAttribute(Type customItemsProviderType, params object[] dependencies)
        {
            if (!typeof(ICustomItemsProvider).IsAssignableFrom(customItemsProviderType))
            {
                throw new InvalidCastException($"{customItemsProviderType.FullName} doesn't implement {typeof(ICustomItemsProvider).FullName}");
            }

            Dependencies = dependencies;

            CustomItemsProvider = (ICustomItemsProvider)Activator.CreateInstance(customItemsProviderType);
            DependencyHandler = new CustomItemsAttributeDependencyHandler(CustomItemsProvider, () => DisplayMemberPath);
        }

        /// <summary>
        /// Creates combo box with static items
        /// </summary>
        /// <param name="items"></param>
        protected ItemsSourceControlAttribute(params object[] items)
        {
            StaticItems = items;
        }
    }
}
