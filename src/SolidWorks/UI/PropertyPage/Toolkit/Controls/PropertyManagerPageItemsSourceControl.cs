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

        public ItemsControlItem[] Items
        {
            get => m_Items;
            set
            {
                m_Items = value;
                SetItemsToControl(value);
            }
        }

        protected abstract void SetItemsToControl(ItemsControlItem[] items);
        
        private readonly IMetadata m_Metadata;

        public PropertyManagerPageItemsSourceControl(int id, object tag,
            TSwCtrl ctrl, SwPropertyManagerPageHandler handler, IMetadata metadata, IPropertyManagerPageLabel label)
            : base(ctrl, id, tag, handler, label)
        {
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
        
        protected TVal GetItem(int index) 
        {
            if (index >= 0 && index < m_Items.Length)
            {
                return (TVal)m_Items[index].Value;
            }
            else
            {
                return default;
            }
        }

        protected int GetItemIndex(TVal value) 
        {
            int index = -1;

            for (int i = 0; i < m_Items.Length; i++)
            {
                if (object.Equals(m_Items[i].Value, value))
                {
                    index = (short)i;
                    break;
                }
            }

            return index;
        }
    }
}