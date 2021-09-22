﻿//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
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
    internal class PropertyManagerPageComboBoxControl<TVal> : PropertyManagerPageItemsSourceControl<TVal, IPropertyManagerPageCombobox>
    {
        protected override event ControlValueChangedDelegate<TVal> ValueChanged;
        
        public PropertyManagerPageComboBoxControl(int id, object tag,
            IPropertyManagerPageCombobox comboBox,
            SwPropertyManagerPageHandler handler, IMetadata metadata, IPropertyManagerPageLabel label)
            : base(id, tag, comboBox, handler, metadata, label)
        {
            m_Handler.ComboBoxChanged += OnComboBoxChanged;
        }
        
        private void OnComboBoxChanged(int id, int selIndex)
        {
            if (Id == id)
            {
                ValueChanged?.Invoke(this, GetItem(selIndex));
            }
        }

        protected override TVal GetSpecificValue() 
            => GetItem(SwSpecificControl.CurrentSelection);

        protected override TVal GetItem(int index)
        {
            if (Items != null)
            {
                if (index >= 0 && index < Items.Length)
                {
                    return (TVal)Items[index].Value;
                }
                else
                {
                    if (Items.Length > 0)
                    {
                        return (TVal)Items[0].Value;
                    }
                }
            }

            return default;
        }

        protected override void SetSpecificValue(TVal value)
        {
            var index = GetItemIndex(value);

            //overriding the default value if specified value is not found
            if (index == -1 && Items.Any())
            {
                index = 0;
                ValueChanged?.Invoke(this, GetItem(index));
            }

            SwSpecificControl.CurrentSelection = (short)index;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_Handler.ComboBoxChanged -= OnComboBoxChanged;
            }
        }

        protected override void SetItemsToControl(ItemsControlItem[] items)
        {
            SwSpecificControl.Clear();

            if (items?.Any() == true)
            {
                SwSpecificControl.AddItems(items.Select(x => x.DisplayName).ToArray());
            }
        }
    }
}