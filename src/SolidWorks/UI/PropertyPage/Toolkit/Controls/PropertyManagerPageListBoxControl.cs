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
    internal class PropertyManagerPageListBoxControl : PropertyManagerPageItemsSourceControl<object, IPropertyManagerPageListbox>
    {
        protected override event ControlValueChangedDelegate<object> ValueChanged;

        private readonly Type m_TargetType;
        private readonly bool m_IsMultiSelect;

        public PropertyManagerPageListBoxControl(int id, object tag,
            IPropertyManagerPageListbox listBox, Type targetType, bool isMultiSel,
            SwPropertyManagerPageHandler handler, IMetadata metadata, IPropertyManagerPageLabel label)
            : base(id, tag, listBox, handler, metadata, label)
        {
            m_IsMultiSelect = isMultiSel;
            m_TargetType = targetType;
            m_Handler.ListBoxChanged += OnListBoxChanged;
        }

        private void OnListBoxChanged(int id, int selIndex)
        {
            if (Id == id)
            {
                ValueChanged?.Invoke(this, GetSpecificValue());
            }
        }

        protected override object GetSpecificValue()
        {
            var selIndexes = SwSpecificControl.GetSelectedItems() as short[];

            if (selIndexes?.Any() == true)
            {
                if (!m_IsMultiSelect)
                {
                    return GetItem(selIndexes.First());
                }
                else 
                {
                    var values = selIndexes.Select(i => GetItem(i)).ToArray();

                    if (m_TargetType.IsEnum)
                    {
                        return Enum.ToObject(m_TargetType, values.Sum(v => Convert.ToInt32(v)));
                    }
                    else
                    {
                        var list = Activator.CreateInstance(m_TargetType) as IList;

                        foreach (var val in values) 
                        {
                            list.Add(val);
                        }
                        
                        return list;
                    }
                }
            }
            else 
            {
                if (m_TargetType.IsValueType)
                {
                    return Activator.CreateInstance(m_TargetType);
                }

                return null;
            }
        }

        protected override void SetSpecificValue(object value)
        {
            var selIndices = new List<int>();

            if (m_IsMultiSelect)
            {
                if (value is IList)
                {
                    foreach (var item in (IList)value)
                    {
                        selIndices.Add(GetItemIndex(item));
                    }
                }
                else if(value is Enum)
                {
                    for (int i = 0; i < Items.Length; i++) 
                    {
                        if (((Enum)value).HasFlag((Enum)Items[i].Value)) 
                        {
                            selIndices.Add(i);
                        }
                    }
                }
                
                for (int i = 0; i < SwSpecificControl.ItemCount; i++) 
                {
                    SwSpecificControl.SetSelectedItem((short)i, selIndices.Contains(i));
                }
            }
            else 
            {
                SwSpecificControl.CurrentSelection = (short)GetItemIndex(value);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_Handler.ListBoxChanged -= OnListBoxChanged;
            }
        }

        protected override void SetItemsToControl(ItemsControlItem[] items)
        {
            SwSpecificControl.Clear();
            
            if (items?.Any() == true)
            {
                SwSpecificControl.AddItems(items.Select(i => i.DisplayName).ToArray());
            }
        }
    }
}