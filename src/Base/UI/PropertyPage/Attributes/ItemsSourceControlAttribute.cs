//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
using System.ComponentModel;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Services;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class ItemsSourceControlAttribute : Attribute, IDependentOnAttribute, IHasMetadataAttribute
    {
        public ICustomItemsProvider CustomItemsProvider { get; }

        public IDependencyHandler DependencyHandler { get; }

        public object[] Dependencies { get; }

        public object[] StaticItems { get; }

        public object MetadataTag => ItemsSource;

        /// <summary>
        /// Tag of the metadata property (decorated with <see cref="MetadataAttribute"/>) which contains the items source for this combo box
        /// </summary>
        /// <remarks>Target property must be <see cref="IEnumerable"/> </remarks>
        public object ItemsSource { get; set; }
        
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
            DependencyHandler = new CustomItemsAttributeDependencyHandler(CustomItemsProvider);
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
