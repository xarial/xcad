//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Windows;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Templates;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls
{
    internal class WpfPropertyManagerPageTextBox : WpfPropertyManagerPageControl<string>
    {
        protected override event ControlValueChangedDelegate<string> ValueChanged;
        
        public string Text
        {
            get => m_Text;
            set 
            {
                m_Text = value;
                this.NotifyPropertyChanged();
                this.ValueChanged?.Invoke(this, value);
            }
        }

        public override DataTemplate Template { get; }

        private string m_Text;

        internal WpfPropertyManagerPageTextBox(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, IIconsCreator iconConv)
            : base(parentGroup, atts, metadata, iconConv)
        {
            Template = ControlTemplates.TextBox;
        }

        protected override string GetSpecificValue() => Text;

        protected override void SetSpecificValue(string value) => Text = value;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }
    }
}