//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Constructors;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Structures;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;
using Xarial.XCad.Utils.Reflection;
using System.ComponentModel;
using Xarial.XCad.Reflection;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    internal abstract class PropertyManagerPageItemsSourceControl<TVal, TSwCtrl> : PropertyManagerPageBaseControl<TVal, TSwCtrl>, IItemsControl
        where TSwCtrl : class
    {
        protected override event ControlValueChangedDelegate<TVal> ValueChanged;

        private ItemsControlItem[] m_Items;

        public ItemsControlItem[] Items
        {
            get => m_Items;
            set
            {            
                if(m_Items != null) 
                {
                    foreach (var item in m_Items)
                    {
                        item.DisplayNameChanged -= OnItemDisplayNameChanged;
                    }
                }

                LoadItemsIntoControl(value);

                m_Items = value;

                if (m_Items != null)
                {
                    foreach (var item in m_Items)
                    {
                        item.DisplayNameChanged += OnItemDisplayNameChanged;
                    }
                }
            }
        }

        protected abstract void LoadItemsIntoControl(ItemsControlItem[] newItems);
        
        private readonly IMetadata m_SrcMetadata;
        private readonly Type m_SpecificItemType;

        private readonly string m_DispMembPath;

        private object m_CurMetadataValue;

        public PropertyManagerPageItemsSourceControl(SwApplication app, IGroup parentGroup, IIconsCreator iconConv,
            IAttributeSet atts, IMetadata[] metadata, swPropertyManagerPageControlType_e type, ref int numberOfUsedIds)
            : base(app, parentGroup, iconConv, atts, metadata, type, ref numberOfUsedIds)
        {
            m_SpecificItemType = atts.ContextType;

            ParseItems(app, atts, metadata, out bool isStatic, out ItemsControlItem[] staticItems,
                out m_SrcMetadata, out m_DispMembPath);

            if (isStatic)
            {
                m_Items = staticItems;
            }
            else
            {
                if (m_SrcMetadata != null)
                {
                    m_SrcMetadata.Changed += OnMetadataChanged;
                    m_CurMetadataValue = m_SrcMetadata.Value;
                    m_Items = LoadItemsFromSource(m_CurMetadataValue);
                }
            }

            Items = LoadInitialItems(atts, isStatic, m_Items);
        }

        private void OnItemDisplayNameChanged(ItemsControlItem item, string newDispName)
        {
            SetItemDisplayName(item, Array.IndexOf(Items, item), newDispName);
        }

        protected virtual void SetItemDisplayName(ItemsControlItem item, int index, string newDispName) 
        {
        }

        protected virtual ItemsControlItem[] LoadInitialItems(IAttributeSet atts, bool isStatic, ItemsControlItem[] items) 
        {
            return items;
        }

        public override void Update()
        {
            if (m_SrcMetadata != null) 
            {
                var thisMetadataVal = m_SrcMetadata.Value;

                if (m_CurMetadataValue != thisMetadataVal)
                {
                    m_CurMetadataValue = thisMetadataVal;

                    Items = LoadItemsFromSource(m_CurMetadataValue);
                }
            }
        }

        private void OnMetadataChanged(IMetadata metadata, object value)
        {
            Items = LoadItemsFromSource(value);
        }

        private ItemsControlItem[] LoadItemsFromSource(object value)
        {
            var items = new List<ItemsControlItem>();

            if (value is IEnumerable)
            {
                foreach (var item in value as IEnumerable)
                {
                    items.Add(new ItemsControlItem(item, m_DispMembPath));
                }
            }
            else if (value is null)
            {
                //return empty
            }
            else
            {
                throw new NotSupportedException("Source property must be enumerable");
            }

            return items.ToArray();
        }

        private void ParseItems(IXApplication app, IAttributeSet atts, IMetadata[] metadata,
            out bool isStatic, out ItemsControlItem[] staticItems, out IMetadata itemsSourceMetadata,
            out string dispMembPath)
        {
            if (atts.ContextType.IsEnum)
            {
                staticItems = CreateEnumItems(atts.ContextType);

                isStatic = true;
                itemsSourceMetadata = null;
                dispMembPath = "";
            }
            else
            {
                var customItemsAtt = atts.Get<ItemsSourceControlAttribute>();
                dispMembPath = customItemsAtt.DisplayMemberPath;

                if (customItemsAtt.StaticItems?.Any() == true)
                {
                    staticItems = customItemsAtt
                        .StaticItems.Select(i => new ItemsControlItem(i, customItemsAtt.DisplayMemberPath)).ToArray();

                    isStatic = true;
                    itemsSourceMetadata = null;
                }
                else if (customItemsAtt.CustomItemsProvider != null)
                {
                    itemsSourceMetadata = null;

                    if (customItemsAtt.Dependencies?.Any() != true)
                    {
                        var provider = customItemsAtt.CustomItemsProvider;
                        staticItems = provider.ProvideItems(app, new IControl[0]).Select(i => new ItemsControlItem(i, customItemsAtt.DisplayMemberPath)).ToArray();
                        isStatic = true;
                    }
                    else
                    {
                        isStatic = false;
                        staticItems = null;
                    }
                }
                else if (customItemsAtt.ItemsSource != null)
                {
                    isStatic = false;
                    staticItems = null;
                    itemsSourceMetadata = metadata?.FirstOrDefault(m => object.Equals(m.Tag, customItemsAtt.ItemsSource));

                    if (itemsSourceMetadata == null)
                    {
                        throw new NullReferenceException($"Failed to find the items source metadata property: {customItemsAtt.ItemsSource}");
                    }
                }
                else
                {
                    throw new NotSupportedException("Items source is not specified");
                }
            }
        }

        protected virtual ItemsControlItem[] CreateEnumItems(Type enumType)
        {
            if (!enumType.IsEnum)
            {
                throw new InvalidCastException($"{enumType.FullName} must be an enum");
            }

            var items = new List<ItemsControlItem>();

            foreach (Enum en in Enum.GetValues(enumType))
            {
                var dispName = "";

                en.TryGetAttribute<DisplayNameAttribute>(a => dispName = a.DisplayName);

                if (string.IsNullOrEmpty(dispName))
                {
                    dispName = en.ToString();
                }

                var desc = "";

                en.TryGetAttribute<DescriptionAttribute>(a => desc = a.Description);

                items.Add(new ItemsControlItem(en, dispName, desc));
            }

            return items.ToArray();
        }

        protected virtual TVal GetItem(int index) 
        {
            if (Items != null)
            {
                if (index >= 0 && index < Items.Length)
                {
                    return (TVal)Items[index].Value;
                }
            }

            return GetDefaultItemValue();
        }

        protected TVal GetDefaultItemValue() 
        {
            if (m_SpecificItemType.IsValueType)
            {
                return (TVal)Activator.CreateInstance(m_SpecificItemType);
            }
            else 
            {
                return default;
            }
        }

        protected int GetItemIndex(TVal value) 
        {
            int index = -1;

            if (Items != null)
            {
                for (int i = 0; i < Items.Length; i++)
                {
                    if (object.Equals(Items[i].Value, value))
                    {
                        index = (short)i;
                        break;
                    }
                }
            }

            return index;
        }
    }
}