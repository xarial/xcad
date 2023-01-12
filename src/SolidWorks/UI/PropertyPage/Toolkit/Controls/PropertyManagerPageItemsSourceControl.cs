//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
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

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    internal abstract class PropertyManagerPageItemsSourceControl<TVal, TSwCtrl> : PropertyManagerPageBaseControl<TVal, TSwCtrl>, IItemsControl
        where TSwCtrl : class
    {
        protected override event ControlValueChangedDelegate<TVal> ValueChanged;

        private ItemsControlItem[] m_Items;

        public virtual ItemsControlItem[] Items
        {
            get => m_Items;
            set
            {
                m_Items = value;
                LoadItemsIntoControl(value);
            }
        }

        protected abstract void LoadItemsIntoControl(ItemsControlItem[] newItems);
        
        private readonly IMetadata m_SrcMetadata;
        private readonly Type m_SpecificItemType;

        public PropertyManagerPageItemsSourceControl(SwApplication app, IGroup parentGroup, IIconsCreator iconConv,
            IAttributeSet atts, IMetadata[] metadata, swPropertyManagerPageControlType_e type, ref int numberOfUsedIds)
            : base(app, parentGroup, iconConv, atts, metadata, type, ref numberOfUsedIds)
        {
            m_SpecificItemType = atts.ContextType;

            ParseItems(app, atts, metadata, out bool isStatic, out ItemsControlItem[] staticItems, out m_SrcMetadata);

            if (m_SrcMetadata != null)
            {
                m_SrcMetadata.Changed += OnMetadataChanged;
            }

            SetStaticItems(atts, isStatic, staticItems);
        }

        protected abstract void SetStaticItems(IAttributeSet atts, bool isStatic, ItemsControlItem[] staticItems);

        public override void Update()
        {
            if (m_SrcMetadata != null) 
            {
                LoadItemsFromSource(m_SrcMetadata.Value);
            }
        }

        private void OnMetadataChanged(IMetadata metadata, object value)
        {
            LoadItemsFromSource(value);
        }

        private void LoadItemsFromSource(object value)
        {
            var items = new List<ItemsControlItem>();

            if (value is IEnumerable)
            {
                foreach (var item in value as IEnumerable)
                {
                    items.Add(new ItemsControlItem(item));
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

            Items = items.ToArray();
        }

        private void ParseItems(IXApplication app, IAttributeSet atts, IMetadata[] metadata,
            out bool isStatic, out ItemsControlItem[] staticItems, out IMetadata itemsSourceMetadata)
        {
            if (atts.ContextType.IsEnum)
            {
                var items = EnumExtension.GetEnumFields(atts.ContextType);
                staticItems = items.Select(i => new ItemsControlItem()
                {
                    DisplayName = i.Value,
                    Value = i.Key
                }).ToArray();

                isStatic = true;
                itemsSourceMetadata = null;
            }
            else
            {
                var customItemsAtt = atts.Get<ItemsSourceControlAttribute>();

                if (customItemsAtt.StaticItems?.Any() == true)
                {
                    staticItems = customItemsAtt
                        .StaticItems.Select(i => new ItemsControlItem(i)).ToArray();

                    isStatic = true;
                    itemsSourceMetadata = null;
                }
                else if (customItemsAtt.CustomItemsProvider != null)
                {
                    itemsSourceMetadata = null;

                    if (customItemsAtt.Dependencies?.Any() != true)
                    {
                        var provider = customItemsAtt.CustomItemsProvider;
                        staticItems = provider.ProvideItems(app, new IControl[0]).Select(i => new ItemsControlItem(i)).ToArray();
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