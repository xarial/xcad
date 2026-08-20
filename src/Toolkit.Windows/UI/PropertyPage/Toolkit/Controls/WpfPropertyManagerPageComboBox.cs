//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Linq;
using System.Windows;
using Xarial.XCad.Toolkit.PageBuilder;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Templates;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Structures;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls
{
    internal class WpfPropertyManagerPageComboBox<TVal> : WpfPropertyManagerPageItemsSourceControl<TVal>
    {
        private class ComboBoxItemsControlManager : ItemsControlManager<TVal>
        {
            private readonly WpfPropertyManagerPageComboBox<TVal> m_ComboBox;

            public ComboBoxItemsControlManager(WpfPropertyManagerPageComboBox<TVal> comboBox, IXApplication app, IAttributeSet atts, IMetadata[] metadata) 
                : base(comboBox, app, atts, metadata)
            {
                m_ComboBox = comboBox;
            }

            protected override void LoadItemsIntoControl(ItemsControlItem[] newItems)
            {
                if (m_ComboBox.m_SelectedItem != null)
                {
                    var curVal = m_ComboBox.m_SelectedItem.Value;
                    m_ComboBox.m_SelectedItem = newItems?.FirstOrDefault(i => CompareValues(i.Value, curVal));
                }

                m_ComboBox.NotifyPropertyChanged(nameof(m_ComboBox.Items));
                m_ComboBox.NotifyPropertyChanged(nameof(m_ComboBox.SelectedItem));
            }
        }

        public override DataTemplate Template { get; }

        public ItemsControlItem SelectedItem 
        {
            get => m_SelectedItem;
            set 
            {
                m_SelectedItem = value;
                this.NotifyPropertyChanged();
                ValueChanged?.Invoke(this, GetSpecificValue());
            }
        }

        protected override event ControlValueChangedDelegate<TVal> ValueChanged;

        private ItemsControlItem m_SelectedItem;

        public WpfPropertyManagerPageComboBox(IXApplication app, IGroup parentGroup,
            IAttributeSet atts, IMetadata[] metadata, IIconsCreator iconConv)
            : base(app, parentGroup, atts, metadata, iconConv)
        {
            Template = ControlTemplates.ComboBox;
        }

        protected override ItemsControlManager<TVal> CreateItemsControlManager(IXApplication app, IAttributeSet atts, IMetadata[] metadata)
            => new ComboBoxItemsControlManager(this, app, atts, metadata);

        protected override TVal GetSpecificValue() => (TVal)SelectedItem?.Value;

        protected override void SetSpecificValue(TVal value)
        {
            SelectedItem = Items.FirstOrDefault(i => ItemsControlManager.CompareValues(i.Value, value));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }
    }
}