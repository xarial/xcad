//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Structures;
using Xarial.XCad.Utils.PageBuilder.PageElements;

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
        
        private readonly IMetadata m_Metadata;
        private readonly Type m_SpecificItemType;

        public PropertyManagerPageItemsSourceControl(int id, object tag,
            TSwCtrl ctrl, SwPropertyManagerPageHandler handler, IMetadata metadata, IPropertyManagerPageLabel label, Type specificItemType)
            : base(ctrl, id, tag, handler, label)
        {
            m_SpecificItemType = specificItemType;
            m_Metadata = metadata;

            if (m_Metadata != null)
            {
                m_Metadata.Changed += OnMetadataChanged;
            }
        }

        private void OnMetadataChanged(IMetadata metadata, object value)
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