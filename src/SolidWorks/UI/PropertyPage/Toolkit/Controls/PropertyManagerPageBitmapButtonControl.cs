//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    internal class PropertyManagerPageBitmapButtonControl : PropertyManagerPageBaseControl<object, IPropertyManagerPageBitmapButton>
    {
        protected override event ControlValueChangedDelegate<object> ValueChanged;

        private Action m_ButtonClickHandler;

        public PropertyManagerPageBitmapButtonControl(int id, object tag,
            IPropertyManagerPageBitmapButton bmpButton,
            SwPropertyManagerPageHandler handler, IPropertyManagerPageLabel label, IMetadata[] metadata)
            : base(bmpButton, id, tag, handler, label, metadata)
        {
            m_Handler.ButtonPressed += OnButtonPressed;
        }

        private void OnButtonPressed(int id)
        {
            if (Id == id)
            {
                if (SwSpecificControl.IsCheckable)
                {
                    ValueChanged?.Invoke(this, SwSpecificControl.Checked);
                }
                else
                {
                    if (m_ButtonClickHandler == null)
                    {
                        throw new NullReferenceException("Button click handler is not specified. Set the value of the delegate to the handler method");
                    }

                    m_ButtonClickHandler.Invoke();
                }
            }
        }

        protected override object GetSpecificValue()
        {
            if (SwSpecificControl.IsCheckable)
            {
                return SwSpecificControl.Checked;
            }
            else
            {
                return m_ButtonClickHandler;
            }
        }

        protected override void SetSpecificValue(object value)
        {
            if (SwSpecificControl.IsCheckable)
            {
                SwSpecificControl.Checked = (bool)value;
            }
            else 
            {
                m_ButtonClickHandler = (Action)value;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_Handler.ButtonPressed -= OnButtonPressed;
            }
        }
    }
}