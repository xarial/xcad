//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Linq;
using Xarial.XCad.Toolkit.PageBuilder;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Structures;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.Toolkit.Blazor.Controls
{
    /// <summary>
    /// Non-generic surface of <see cref="BlazorPropertyManagerPageComboBox{TVal}"/> so the
    /// Razor renderer can bind to it without dealing with open generics
    /// </summary>
    internal interface IBlazorComboBoxControl : IItemsControl, IBlazorPropertyManagerPageControl
    {
        ItemsControlItem SelectedItem { get; }

        void SetSelectedItemFromUi(ItemsControlItem item);
    }

    public class BlazorPropertyManagerPageComboBox<TVal> : BlazorPropertyManagerPageItemsSourceControl<TVal>, IBlazorComboBoxControl
    {
        private class ComboBoxItemsControlManager : ItemsControlManager<TVal>
        {
            private readonly BlazorPropertyManagerPageComboBox<TVal> m_ComboBox;

            public ComboBoxItemsControlManager(IXApplication app, IAttributeSet atts, IMetadata[] metadata, BlazorPropertyManagerPageComboBox<TVal> owner)
                : base(owner, app, atts, metadata)
            {
                m_ComboBox = owner;
            }

            protected override void LoadItemsIntoControl(ItemsControlItem[] newItems)
                => m_ComboBox.NotifyInvalidated();
        }

        protected override event ControlValueChangedDelegate<TVal> ValueChanged;

        ItemsControlItem IBlazorComboBoxControl.SelectedItem => m_SelectedItem;

        private ItemsControlItem m_SelectedItem;

        public BlazorPropertyManagerPageComboBox(IXApplication app, IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata)
            : base(app, parentGroup, atts, metadata)
        {
        }

        void IBlazorComboBoxControl.SetSelectedItemFromUi(ItemsControlItem item)
        {
            if (!ReferenceEquals(m_SelectedItem, item))
            {
                m_SelectedItem = item;
                NotifyInvalidated();
                ValueChanged?.Invoke(this, GetSpecificValue());
            }
        }

        protected override ItemsControlManager<TVal> CreateItemsControlManager(IXApplication app, IAttributeSet atts, IMetadata[] metadata)
            => new ComboBoxItemsControlManager(app, atts, metadata, this);

        protected override TVal GetSpecificValue() => m_SelectedItem != null ? (TVal)m_SelectedItem.Value : default;

        protected override void SetSpecificValue(TVal value)
        {
            m_SelectedItem = Items?.FirstOrDefault(i => ItemsControlManager.CompareValues(i.Value, value));
            NotifyInvalidated();
        }
    }
}
