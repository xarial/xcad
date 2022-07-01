//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    internal class PropertyManagerPageTextBoxControl : PropertyManagerPageBaseControl<string, IPropertyManagerPageTextbox>
    {
        protected override event ControlValueChangedDelegate<string> ValueChanged;

        internal PropertyManagerPageTextBoxControl(SwApplication app, IGroup parentGroup, IIconsCreator iconConv,
            IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            : base(app, parentGroup, iconConv, atts, metadata, swPropertyManagerPageControlType_e.swControlType_Textbox, ref numberOfUsedIds)
        {
            m_Handler.TextChanged += OnTextChanged;
        }

        protected override void SetOptions(IPropertyManagerPageTextbox ctrl, IControlOptionsAttribute opts, IAttributeSet atts)
        {
            var height = opts.Height;

            if (height != -1)
            {
                ctrl.Height = height;
            }

            if (atts.Has<TextBoxOptionsAttribute>())
            {
                var style = atts.Get<TextBoxOptionsAttribute>();

                if (style.Style != 0)
                {
                    ctrl.Style = (int)style.Style;
                }
            }
        }

        private void OnTextChanged(int id, string text)
        {
            if (Id == id)
            {
                ValueChanged?.Invoke(this, text);
            }
        }

        protected override string GetSpecificValue()
            => SwSpecificControl.Text;

        protected override void SetSpecificValue(string value)
            => SwSpecificControl.Text = value;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_Handler.TextChanged -= OnTextChanged;
            }
        }
    }
}