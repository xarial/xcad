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
    internal class WpfPropertyManagerPageTextBlock : WpfPropertyManagerPageControl<object>
    {
        protected override event ControlValueChangedDelegate<object> ValueChanged;

        public object Text
        {
            get => m_Text;
            private set
            {
                m_Text = value;
                this.NotifyPropertyChanged();
            }
        }

        public override DataTemplate Template { get; }

        private object m_Text;

        internal WpfPropertyManagerPageTextBlock(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, IIconsCreator iconConv)
            : base(parentGroup, atts, metadata, iconConv)
        {
            Template = ControlTemplates.TextBlock;
        }

        protected override object GetSpecificValue() => null;

        protected override void SetSpecificValue(object value)
        {
            Text = value;
        }

        protected override void Dispose(bool disposing)
        {
        }
    }
}