//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.UI.PropertyPage.Structures;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    internal class PropertyManagerPageListBoxControl : PropertyManagerPageItemsSourceControl<object, IPropertyManagerPageListbox>
    {
        protected override event ControlValueChangedDelegate<object> ValueChanged;

        private Type m_TargetType;
        private bool m_IsMultiSelect;
        private bool m_SuspendHandlingChanged;

        private object m_CurrentValueCached;

        public PropertyManagerPageListBoxControl(SwApplication app, IGroup parentGroup, IIconsCreator iconConv,
            IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            : base(app, parentGroup, iconConv, atts, metadata, swPropertyManagerPageControlType_e.swControlType_Listbox, ref numberOfUsedIds)
        {
            m_Handler.ListBoxChanged += OnListBoxChanged;
        }

        protected override void InitData(IControlOptionsAttribute opts, IAttributeSet atts)
        {
            m_TargetType = atts.ContextType;
            m_IsMultiSelect = (atts.ContextType.IsEnum
                && atts.ContextType.GetCustomAttribute<FlagsAttribute>() != null)
                || typeof(IList).IsAssignableFrom(atts.ContextType);
        }

        protected override void SetOptions(IPropertyManagerPageListbox ctrl, IControlOptionsAttribute opts, IAttributeSet atts)
        {
            var height = opts.Height;

            if (height <= 0)
            {
                height = 50;
            }

            ctrl.Height = height;

            int style = 0;

            if (atts.Has<ListBoxOptionsAttribute>())
            {
                var lstOpts = atts.Get<ListBoxOptionsAttribute>();

                if (lstOpts.Style != 0)
                {
                    style = (int)lstOpts.Style;

                    if (lstOpts.Style.HasFlag(ListBoxStyle_e.Sorted))
                    {
                        style -= (int)ListBoxStyle_e.Sorted;
                    }
                }
            }

            if (m_IsMultiSelect)
            {
                style = style + (int)swPropMgrPageListBoxStyle_e.swPropMgrPageListBoxStyle_MultipleItemSelect;
            }

            ctrl.Style = style;
        }

        protected override ItemsControlItem[] LoadInitialItems(IAttributeSet atts, bool isStatic, ItemsControlItem[] items)
        {
            if (isStatic)
            {
                var sortItems = atts.Has<ListBoxOptionsAttribute>() && atts.Get<ListBoxOptionsAttribute>().Style.HasFlag(ListBoxStyle_e.Sorted);
                
                if (sortItems)
                {
                    items = items.OrderBy(i => i.DisplayName).ToArray();
                }
            }

            return items;
        }

        private void OnListBoxChanged(int id, int selIndex)
        {
            if (Id == id)
            {
                if (!m_SuspendHandlingChanged)
                {
                    ValueChanged?.Invoke(this, GetSpecificValue());
                }
            }
        }

        protected override object GetSpecificValue()
        {
            var selIndexes = SwSpecificControl.GetSelectedItems() as short[];

            object curVal;

            if (selIndexes?.Any() == true)
            {
                if (!m_IsMultiSelect)
                {
                    curVal = GetItem(selIndexes.First());
                }
                else 
                {
                    var values = selIndexes.Select(i => GetItem(i)).ToArray();

                    if (m_TargetType.IsEnum)
                    {
                        curVal = Enum.ToObject(m_TargetType, values.Sum(Convert.ToInt32));
                    }
                    else
                    {
                        var list = Activator.CreateInstance(m_TargetType) as IList;

                        foreach (var val in values) 
                        {
                            list.Add(val);
                        }

                        curVal = list;
                    }
                }
            }
            else 
            {
                curVal = GetDefaultItemValue();
            }

            m_CurrentValueCached = curVal;

            return curVal;
        }

        protected override void SetSpecificValue(object value)
        {
            m_CurrentValueCached = value;

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

        protected override void LoadItemsIntoControl(ItemsControlItem[] newItems)
        {
            SwSpecificControl.Clear();

            if (newItems?.Any() == true)
            {
                SwSpecificControl.AddItems(newItems.Select(i => i.DisplayName).ToArray());
            }

            var oldCachedValue = m_CurrentValueCached;

            SetSpecificValue(m_CurrentValueCached);

            var curVal = GetSpecificValue();

            if (IsValueChanged(curVal, oldCachedValue)) 
            {
                ValueChanged?.Invoke(this, curVal);
            }
        }

        private bool IsValueChanged(object oldVal, object newVal) 
        {
            if (oldVal == null && newVal == null) 
            {
                return false;
            }

            if (m_IsMultiSelect && !m_TargetType.IsEnum)
            {
                if (oldVal is IList && newVal is IList)
                {
                    var oldList = (IList)oldVal;
                    var newList = (IList)newVal;

                    if (oldList.Count == newList.Count)
                    {
                        for (int i = 0; i < oldList.Count; i++)
                        {
                            if (!m_EqualityComparer.Equals(oldList[i], newList[i]))
                            {
                                return true;
                            }
                        }

                        return false;
                    }
                    else 
                    {
                        return true;
                    }
                }
                else 
                {
                    return true;
                }
            }
            else
            {
                return !m_EqualityComparer.Equals(oldVal, newVal);
            }
        }

        protected override void SetItemDisplayName(ItemsControlItem item, int index, string newDispName)
        {
            if (index != -1 && SwSpecificControl.ItemCount > index)
            {
                m_SuspendHandlingChanged = true;

                try
                {
                    var selItems = (short[])SwSpecificControl.GetSelectedItems();

                    SwSpecificControl.DeleteItem((short)index);
                    SwSpecificControl.InsertItem((short)index, newDispName);

                    if (selItems != null)
                    {
                        for (short i = 0; i < SwSpecificControl.ItemCount; i++)
                        {
                            SwSpecificControl.SetSelectedItem(i, selItems.Contains(i));
                        }
                    }
                }
                finally
                {
                    m_SuspendHandlingChanged = false;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_Handler.ListBoxChanged -= OnListBoxChanged;
            }
        }
    }
}