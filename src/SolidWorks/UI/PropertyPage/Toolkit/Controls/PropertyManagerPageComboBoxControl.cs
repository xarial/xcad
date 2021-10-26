//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

        private readonly bool m_IsStatic;
        private readonly ItemsControlItem[] m_StaticItems;
        private ItemsControlItem[] m_DynamicItems;

        private TVal m_CurrentValueCached;
        private bool m_IsPageOpened;

        public PropertyManagerPageComboBoxControl(int id, object tag,
            IPropertyManagerPageCombobox comboBox,
            SwPropertyManagerPageHandler handler, IMetadata srcMetadata, IPropertyManagerPageLabel label,
            Type specificItemType, bool isStatic, ItemsControlItem[] staticItems, IMetadata[] metadata)
            : base(id, tag, comboBox, handler, srcMetadata, label, specificItemType, metadata)
        {
            m_IsStatic = isStatic;
            m_StaticItems = staticItems;

            m_Handler.ComboBoxChanged += OnComboBoxChanged;
            m_Handler.Opened += OnPageOpened;
            m_Handler.Closed += OnPageClosed;
            m_IsPageOpened = false;
        }

        public override ItemsControlItem[] Items
        {
            get
            {
                if (!m_IsStatic)
                {
                    return m_DynamicItems;
                }
                else
                {
                    return m_StaticItems;
                }
            }
            set
            {
                if (!m_IsStatic)
                {
                    m_DynamicItems = value;

                    if (m_IsPageOpened)
                    {
                        LoadItemsIntoControl(value);
                    }
                }
                else 
                {
                    Debug.Assert(false, "Static items cannot be changed");
                }
            }
        }

        //NOTE: ComboBox in SOLIDWORKS Property Manager page behaves differently depending when the values are added to the control
        //if values are added before the page is opened than ComboBox cannot have empty value, if after - then it can be empty
        //as ComboBox can load items dynamically after page is opened for hte consistency all items will be added after page is displayed
        private void OnPageOpened()
        {
            m_IsPageOpened = true;

            if (m_IsStatic)
            {
                LoadItemsIntoControl(m_StaticItems);
            }
            else 
            {
                LoadItemsIntoControl(m_DynamicItems);
            }
        }

        private void OnPageClosed(swPropertyManagerPageCloseReasons_e reason)
        {
            m_IsPageOpened = false;
            SwSpecificControl.Clear();
            SwSpecificControl.CurrentSelection = -1;
            m_CurrentValueCached = GetDefaultItemValue();
            m_DynamicItems = null;
        }

        private void OnComboBoxChanged(int id, int selIndex)
        {
            if (Id == id)
            {
                var val = GetItem(selIndex);
                m_CurrentValueCached = val;
                ValueChanged?.Invoke(this, val);
            }
        }

        protected override TVal GetSpecificValue()
        {
            if (!m_IsPageOpened)
            {
                return m_CurrentValueCached;
            }
            else
            {
                return GetItem(SwSpecificControl.CurrentSelection);
            }
        }

        protected override void SetSpecificValue(TVal value)
        {
            m_CurrentValueCached = value;

            var index = GetItemIndex(value);

            SwSpecificControl.CurrentSelection = (short)index;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_Handler.ComboBoxChanged -= OnComboBoxChanged;
            }
        }

        protected override void LoadItemsIntoControl(ItemsControlItem[] newItems)
        {
            SwSpecificControl.Clear();

            if (newItems?.Any() == true)
            {
                SwSpecificControl.AddItems(newItems.Select(x => x.DisplayName).ToArray());

                if (!newItems.Any(i => object.Equals(i.Value, m_CurrentValueCached)))
                {
                    //if items source changed dynamically previously cached value might not fit new source
                    var defVal = GetDefaultItemValue();

                    if (!object.Equals(m_CurrentValueCached, defVal))
                    {
                        m_CurrentValueCached = defVal;
                        ValueChanged?.Invoke(this, m_CurrentValueCached);
                    }
                }

                SetSpecificValue(m_CurrentValueCached);
            }
        }
    }
}