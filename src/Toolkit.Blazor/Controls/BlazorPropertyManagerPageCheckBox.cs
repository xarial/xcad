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
    public class BlazorPropertyManagerPageCheckBox : BlazorPropertyManagerPageControl<bool>
    {
        protected override event ControlValueChangedDelegate<bool> ValueChanged;

        public bool Checked => m_Checked;

        private bool m_Checked;

        public BlazorPropertyManagerPageCheckBox(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata)
            : base(parentGroup, atts, metadata)
        {
        }

        internal void SetCheckedFromUi(bool value)
        {
            if (m_Checked == value)
            {
                return;
            }

            m_Checked = value;
            NotifyInvalidated();
            ValueChanged?.Invoke(this, value);
        }

        protected override bool GetSpecificValue() => Checked;

        protected override void SetSpecificValue(bool value)
        {
            m_Checked = value;
            NotifyInvalidated();
        }
    }
}
