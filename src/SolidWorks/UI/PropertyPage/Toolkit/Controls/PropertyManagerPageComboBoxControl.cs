//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Structures;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{   
    internal class PropertyManagerPageComboBoxControl<TVal> : PropertyManagerPageBaseControl<TVal, IPropertyManagerPageCombobox>, IItemsControl
    {
        protected override event ControlValueChangedDelegate<TVal> ValueChanged;
        
        private ItemsControlItem[] m_Items;

        public ItemsControlItem[] Items 
        {
            get => m_Items;
            set
            {
                m_Items = value;
                SwSpecificControl.Clear();
                SwSpecificControl.AddItems(value.Select(x => x.DisplayName).ToArray());
            }
        }

        private readonly bool m_SelectDefaultValue;

        private readonly IMetadata m_Metadata;

        public PropertyManagerPageComboBoxControl(int id, object tag, bool selDefVal,
            IPropertyManagerPageCombobox comboBox,
            SwPropertyManagerPageHandler handler, IMetadata metadata) : base(comboBox, id, tag, handler)
        {
            m_Metadata = metadata;
            if (m_Metadata != null)
            {
                m_Metadata.Changed += OnMetadataChanged;
            }

            m_SelectDefaultValue = selDefVal;
            m_Handler.ComboBoxChanged += OnComboBoxChanged;
        }

        private void OnMetadataChanged(IMetadata metadata, object value)
        {
            var items = new List<ItemsControlItem>();

            if (value is IEnumerable)
            {
                foreach (var item in value as IEnumerable) 
                {
                    items.Add(new ItemsControlItem()
                    {
                        Value = item,
                        DisplayName = item?.ToString()
                    });
                }
            }
            else 
            {
                throw new NotSupportedException("Source property must be enumerable");
            }

            Items = items.ToArray();
        }

        private void OnComboBoxChanged(int id, int selIndex)
        {
            if (Id == id)
            {
                ValueChanged?.Invoke(this, (TVal)m_Items[selIndex].Value);
            }
        }

        protected override TVal GetSpecificValue()
        {
            var curSelIndex = SwSpecificControl.CurrentSelection;

            if (curSelIndex >= 0 && curSelIndex < m_Items.Length)
            {
                return (TVal)m_Items[curSelIndex].Value;
            }
            else
            {
                return default;
            }
        }

        protected override void SetSpecificValue(TVal value)
        {
            short index = -1;

            for (int i = 0; i < m_Items.Length; i++) 
            {
                if (object.Equals(m_Items[i].Value, value))
                {
                    index = (short)i;
                    break;
                }
            }

            if (index == -1 && m_SelectDefaultValue && m_Items.Any())
            {
                index = 0;
                ValueChanged?.Invoke(this, (TVal)m_Items[index].Value);
            }

            SwSpecificControl.CurrentSelection = index;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_Handler.ComboBoxChanged -= OnComboBoxChanged;
            }
        }
    }
}