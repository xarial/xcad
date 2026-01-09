//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Windows;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Templates;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls
{
    internal class WpfPropertyManagerPageCheckBox : WpfPropertyManagerPageControl<bool>
    {
        protected override event ControlValueChangedDelegate<bool> ValueChanged;

        public bool Checked
        {
            get => m_Checked;
            set
            {
                m_Checked = value;
                this.NotifyPropertyChanged();
                this.ValueChanged?.Invoke(this, value);
            }
        }


        public override DataTemplate Template { get; }

        private bool m_Checked;

        public WpfPropertyManagerPageCheckBox(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, IIconsCreator iconConv)
            : base(parentGroup, atts, metadata, iconConv)
        {
            Template = ControlTemplates.CheckBox;
        }

        protected override bool GetSpecificValue() => Checked;

        protected override void SetSpecificValue(bool value) => Checked = value;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }
    }
}