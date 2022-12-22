//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    internal class PropertyManagerPageCheckBoxControl : PropertyManagerPageBaseControl<bool, IPropertyManagerPageCheckbox>
    {
        protected override event ControlValueChangedDelegate<bool> ValueChanged;

        public PropertyManagerPageCheckBoxControl(SwApplication app, IGroup parentGroup, IIconsCreator iconConv,
            IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            : base(app, parentGroup, iconConv, atts, metadata, swPropertyManagerPageControlType_e.swControlType_Checkbox, ref numberOfUsedIds)
        {
            m_Handler.CheckChanged += OnCheckChanged;
        }

        protected override void SetOptions(IPropertyManagerPageCheckbox ctrl, IControlOptionsAttribute opts, IAttributeSet atts)
        {
            ctrl.Caption = atts.Name;
        }

        private void OnCheckChanged(int id, bool isChecked)
        {
            if (Id == id)
            {
                ValueChanged?.Invoke(this, isChecked);
            }
        }

        protected override bool GetSpecificValue()
            => SwSpecificControl.Checked;

        protected override void SetSpecificValue(bool value)
            => SwSpecificControl.Checked = value;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_Handler.CheckChanged -= OnCheckChanged;
            }
        }
    }
}