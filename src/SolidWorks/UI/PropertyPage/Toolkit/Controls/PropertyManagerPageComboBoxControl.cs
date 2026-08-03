//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections;
using System.Linq;
using Xarial.XCad.Toolkit.PageBuilder;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Structures;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    internal class PropertyManagerPageComboBoxControl<TVal> : PropertyManagerPageItemsSourceControl<TVal, IPropertyManagerPageCombobox>
    {
        private class ComboBoxItemsControlManager : ItemsControlManager<TVal>
        {
            private readonly PropertyManagerPageComboBoxControl<TVal> m_ComboBox;

            public ComboBoxItemsControlManager(PropertyManagerPageComboBoxControl<TVal> comboBox, IXApplication app, IAttributeSet atts, IMetadata[] metadata) 
                : base(comboBox, app, atts, metadata)
            {
                m_ComboBox = comboBox;
            }

            public void ReloadItems() 
            {
                LoadItemsIntoControl(Items);
            }

            protected override void LoadItemsIntoControl(ItemsControlItem[] newItems)
            {
                if (m_ComboBox.m_IsPageOpened)
                {
                    m_ComboBox.SwSpecificControl.Clear();

                    if (newItems?.Any() == true)
                    {
                        m_ComboBox.SwSpecificControl.AddItems(newItems.Select(x => x.DisplayName).ToArray());
                    }

                    if (newItems?.Any(i => CompareValues(i.Value, m_ComboBox.m_CurrentValueCached)) != true && !m_ComboBox.IsEditableText)
                    {
                        //if items source changed dynamically previously cached value might not fit new source
                        var defVal = GetDefaultItemValue();

                        if (!CompareValues(m_ComboBox.m_CurrentValueCached, defVal))
                        {
                            m_ComboBox.m_CurrentValueCached = defVal;
                            m_ComboBox.ValueChanged?.Invoke(m_ComboBox, m_ComboBox.m_CurrentValueCached);
                        }
                    }

                    m_ComboBox.SetSpecificValue(m_ComboBox.m_CurrentValueCached);
                }
            }

            protected override void SetItemDisplayName(ItemsControlItem item, int index, string newDispName)
            {
                if (index != -1)
                {
                    m_ComboBox.m_SuspendHandlingChanged = true;

                    try
                    {
                        var curSel = m_ComboBox.SwSpecificControl.CurrentSelection;

                        m_ComboBox.SwSpecificControl.DeleteItem((short)index);
                        m_ComboBox.SwSpecificControl.InsertItem((short)index, newDispName);

                        m_ComboBox.SwSpecificControl.CurrentSelection = curSel;
                    }
                    finally
                    {
                        m_ComboBox.m_SuspendHandlingChanged = false;
                    }
                }
            }
        }

        protected override event ControlValueChangedDelegate<TVal> ValueChanged;

        private TVal m_CurrentValueCached;
        private bool m_IsPageOpened;
        private bool m_SuspendHandlingChanged;

        public PropertyManagerPageComboBoxControl(SwApplication app, IGroup parentGroup, IIconsCreator iconConv,
            IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            : base(app, parentGroup, iconConv, atts, metadata, swPropertyManagerPageControlType_e.swControlType_Combobox, ref numberOfUsedIds)
        {
            m_Handler.ComboBoxChanged += OnComboBoxChanged;
            m_Handler.ComboBoxEditChanged += OnComboBoxEditChanged;
            m_Handler.Opened += OnPageOpened;
            m_Handler.PreClosed += OnPageClosed;
            m_IsPageOpened = false;
        }

        protected override ItemsControlManager<TVal> CreateItemsControlManager(SwApplication app, IAttributeSet atts, IMetadata[] metadata)
            => new ComboBoxItemsControlManager(this, app, atts, metadata);

        protected override void SetOptions(IPropertyManagerPageCombobox ctrl, IControlOptionsAttribute opts, IAttributeSet atts)
        {
            if (opts.Height != -1)
            {
                SwSpecificControl.Height = (short)opts.Height;
            }

            if (atts.Has<ComboBoxOptionsAttribute>())
            {
                var cmbOpts = atts.Get<ComboBoxOptionsAttribute>();

                if (cmbOpts.Style != 0)
                {
                    SwSpecificControl.Style = (int)cmbOpts.Style;
                }
            }
        }

        //NOTE: ComboBox in SOLIDWORKS Property Manager page behaves differently depending when the values are added to the control
        //if values are added before the page is opened than ComboBox cannot have empty value, if after - then it can be empty
        //as ComboBox can load items dynamically after page is opened for the consistency all items will be added after page is displayed
        private void OnPageOpened()
        {
            m_IsPageOpened = true;

            ((ComboBoxItemsControlManager)ItemsControlManager).ReloadItems();
        }

        private void OnPageClosed(swPropertyManagerPageCloseReasons_e reason)
        {
            m_IsPageOpened = false;
            SwSpecificControl.Clear();
            SwSpecificControl.CurrentSelection = -1;
            m_CurrentValueCached = ItemsControlManager.GetDefaultItemValue();
        }

        private void OnComboBoxChanged(int id, int selIndex)
        {
            if (Id == id)
            {
                if (!m_SuspendHandlingChanged)
                {
                    var val = ItemsControlManager.GetItem(selIndex);
                    m_CurrentValueCached = val;
                    ValueChanged?.Invoke(this, val);
                }
            }
        }

        private void OnComboBoxEditChanged(int id, string text)
        {
            if (Id == id)
            {
                if (!m_SuspendHandlingChanged)
                {
                    var val = (TVal)text?.Cast(typeof(TVal));
                    m_CurrentValueCached = val;
                    ValueChanged?.Invoke(this, val);
                }
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
                if (SwSpecificControl.CurrentSelection != -1)
                {
                    return ItemsControlManager.GetItem(SwSpecificControl.CurrentSelection);
                }
                else if (IsEditableText)
                {
                    return (TVal)SwSpecificControl.EditText?.Cast(typeof(TVal));
                }
                else
                {
                    return ItemsControlManager.GetDefaultItemValue();
                }
            }
        }

        protected override void SetSpecificValue(TVal value)
        {
            m_CurrentValueCached = value;

            var index = ItemsControlManager.GetItemIndex(value);

            if (index != -1)
            {
                SwSpecificControl.CurrentSelection = (short)index;
            }
            else if (IsEditableText)
            {
                SwSpecificControl.EditText = value?.ToString();
            }
            else
            {
                SwSpecificControl.CurrentSelection = -1;
            }
        }

        private bool IsEditableText => ((swPropMgrPageComboBoxStyle_e)SwSpecificControl.Style).HasFlag(swPropMgrPageComboBoxStyle_e.swPropMgrPageComboBoxStyle_EditableText);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_Handler.ComboBoxChanged -= OnComboBoxChanged;
                m_Handler.ComboBoxEditChanged -= OnComboBoxEditChanged;
            }
        }
    }
}