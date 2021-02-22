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
using Xarial.XCad.UI.PropertyPage.Structures;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    public interface ICustomItemsComboBoxControlConstructor
    {
    }
    
    internal class CustomItemsAttributeDependencyHandler : IDependencyHandler
    {
        private readonly ICustomItemsProvider m_ItemsProvider;

        internal CustomItemsAttributeDependencyHandler(ICustomItemsProvider itemsProvider) 
        {
            m_ItemsProvider = itemsProvider;
        }

        public void UpdateState(IXApplication app, IControl source, IControl[] dependencies)
        {
            var itemsCtrl = (IItemsControl)source;
            
            itemsCtrl.Items = m_ItemsProvider.ProvideItems(app, dependencies)
                ?.Select(i => new ItemsControlItem()
                {
                    DisplayName = i.ToString(),
                    Value = i
                }).ToArray();
        }
    }

    public class ComboBoxItemsSourceAttribute : Attribute, ISpecificConstructorAttribute, IDependentOnAttribute
    {
        public Type ConstructorType { get; }

        public ICustomItemsProvider CustomItemsProvider { get; }

        public IDependencyHandler DependencyHandler { get; }

        public object[] Dependencies { get; }

        public object[] StaticItems { get; }

        private ComboBoxItemsSourceAttribute() 
        {
            ConstructorType = typeof(ICustomItemsComboBoxControlConstructor);
        }

        public ComboBoxItemsSourceAttribute(Type customItemsProviderType, params object[] dependencies) : this()
        {
            if (!typeof(ICustomItemsProvider).IsAssignableFrom(customItemsProviderType))
            {
                throw new InvalidCastException($"{customItemsProviderType.FullName} doesn't implement {typeof(ICustomItemsProvider).FullName}");
            }

            Dependencies = dependencies;

            CustomItemsProvider = (ICustomItemsProvider)Activator.CreateInstance(customItemsProviderType);
            DependencyHandler = new CustomItemsAttributeDependencyHandler(CustomItemsProvider);
        }

        public ComboBoxItemsSourceAttribute(params object[] items) : this()
        {
            StaticItems = items;
        }
    }
}
