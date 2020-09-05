//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Services;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    public interface ICustomItemsComboBoxControlConstructor
    {
    }

    public class ItemsControlItem 
    {
        public string DisplayName { get; set; }
        public object Value { get; set; }
    }

    public interface IItemsControl : IControl
    {
        ItemsControlItem[] Items { get; set; }
    }

    internal class CustomItemsAttributeDependencyHandler : IDependencyHandler
    {
        private readonly ICustomItemsProvider m_ItemsProvider;

        internal CustomItemsAttributeDependencyHandler(ICustomItemsProvider itemsProvider) 
        {
            m_ItemsProvider = itemsProvider;
        }

        public void UpdateState(IControl source, IControl[] dependencies)
        {
            var itemsCtrl = (IItemsControl)source;
            //TODO: get app
            itemsCtrl.Items = m_ItemsProvider.ProvideItems(null, dependencies)
                ?.Select(i => new ItemsControlItem()
                {
                    DisplayName = i.ToString(),
                    Value = i
                }).ToArray();
        }
    }

    public class CustomItemsAttribute : Attribute, ISpecificConstructorAttribute, IDependentOnAttribute
    {
        public Type ConstructorType { get; }

        public ICustomItemsProvider CustomItemsProvider { get; }

        public IDependencyHandler DependencyHandler { get; }

        public object[] Dependencies { get; }

        public CustomItemsAttribute(Type customItemsProviderType, params object[] dependencies)
        {
            if (!typeof(ICustomItemsProvider).IsAssignableFrom(customItemsProviderType))
            {
                throw new InvalidCastException($"{customItemsProviderType.FullName} doesn't implement {typeof(ICustomItemsProvider).FullName}");
            }

            Dependencies = dependencies;

            CustomItemsProvider = (ICustomItemsProvider)Activator.CreateInstance(customItemsProviderType);
            ConstructorType = typeof(ICustomItemsComboBoxControlConstructor);
            DependencyHandler = new CustomItemsAttributeDependencyHandler(CustomItemsProvider);
        }
    }
}
