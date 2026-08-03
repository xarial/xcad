//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.Toolkit.Blazor.Controls
{
    public class BlazorPropertyManagerPageTextBox : BlazorPropertyManagerPageControl<string>
    {
        protected override event ControlValueChangedDelegate<string> ValueChanged;

        public string Text => m_Text;

        private string m_Text;

        public BlazorPropertyManagerPageTextBox(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata)
            : base(parentGroup, atts, metadata)
        {
        }

        internal void SetTextFromUi(string value)
        {
            if (m_Text == value)
            {
                return;
            }

            m_Text = value;
            NotifyInvalidated();
            ValueChanged?.Invoke(this, value);
        }

        protected override string GetSpecificValue() => Text;

        protected override void SetSpecificValue(string value)
        {
            m_Text = value;
            NotifyInvalidated();
        }
    }
}
