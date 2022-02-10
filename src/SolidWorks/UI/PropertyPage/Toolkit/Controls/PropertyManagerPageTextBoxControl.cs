﻿//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    internal class PropertyManagerPageTextBoxControl : PropertyManagerPageBaseControl<string, IPropertyManagerPageTextbox>
    {
        protected override event ControlValueChangedDelegate<string> ValueChanged;

        internal PropertyManagerPageTextBoxControl(int id, object tag,
            IPropertyManagerPageTextbox textBox,
            SwPropertyManagerPageHandler handler, IPropertyManagerPageLabel label, IMetadata[] metadata)
            : base(textBox, id, tag, handler, label, metadata)
        {
            m_Handler.TextChanged += OnTextChanged;
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