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
    internal class PropertyManagerPageButtonControl : PropertyManagerPageBaseControl<Action, IPropertyManagerPageButton>
    {
#pragma warning disable CS0067

        protected override event ControlValueChangedDelegate<Action> ValueChanged;

#pragma warning restore CS0067

        private Action m_ButtonClickHandler;

        public PropertyManagerPageButtonControl(int id, object tag,
            IPropertyManagerPageButton button,
            SwPropertyManagerPageHandler handler, IPropertyManagerPageLabel label, IMetadata[] metadata)
            : base(button, id, tag, handler, label, metadata)
        {
            m_Handler.ButtonPressed += OnButtonPressed;
        }

        private void OnButtonPressed(int id)
        {
            if (Id == id)
            {
                if (m_ButtonClickHandler == null) 
                {
                    throw new NullReferenceException("Button click handler is not specified. Set the value of the delegate to the handler method");
                }

                m_ButtonClickHandler.Invoke();
            }
        }

        protected override Action GetSpecificValue()
        {
            return m_ButtonClickHandler;
        }

        protected override void SetSpecificValue(Action value)
        {
            m_ButtonClickHandler = value;
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