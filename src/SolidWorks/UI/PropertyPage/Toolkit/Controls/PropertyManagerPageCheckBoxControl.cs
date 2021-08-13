//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    internal class PropertyManagerPageCheckBoxControl : PropertyManagerPageBaseControl<bool, IPropertyManagerPageCheckbox>
    {
        protected override event ControlValueChangedDelegate<bool> ValueChanged;

        public PropertyManagerPageCheckBoxControl(int id, object tag,
            IPropertyManagerPageCheckbox checkBox,
            SwPropertyManagerPageHandler handler, IPropertyManagerPageLabel label) : base(checkBox, id, tag, handler, label)
        {
            m_Handler.CheckChanged += OnCheckChanged;
        }

        private void OnCheckChanged(int id, bool isChecked)
        {
            if (Id == id)
            {
                ValueChanged?.Invoke(this, isChecked);
            }
        }

        protected override bool GetSpecificValue()
        {
            return SwSpecificControl.Checked;
        }

        protected override void SetSpecificValue(bool value)
        {
            SwSpecificControl.Checked = value;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_Handler.CheckChanged -= OnCheckChanged;
            }
        }
    }
}