//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    //TODO: add support to IPropertyManagerPageActiveX
    internal class PropertyManagerPageCustomControl : PropertyManagerPageBaseControl<object, IPropertyManagerPageWindowFromHandle>
    {
        protected override event ControlValueChangedDelegate<object> ValueChanged;

        private readonly Func<IXCustomControl> m_ControlFact;
        private IXCustomControl m_CurrentControl;

        internal PropertyManagerPageCustomControl(int id, object tag,
            IPropertyManagerPageWindowFromHandle wndFromHandler,
            SwPropertyManagerPageHandler handler, Func<IXCustomControl> controlFact) : base(wndFromHandler, id, tag, handler)
        {
            m_Handler.CustomControlCreated += OnCustomControlCreated;
            m_Handler.Opening += OnPageOpening;
            m_ControlFact = controlFact;
        }

        private void OnPageOpening()
        {
            if (m_CurrentControl != null)
            {
                m_CurrentControl.DataContextChanged -= OnDataContextChanged;
            }
            
            m_CurrentControl = m_ControlFact.Invoke();
            m_CurrentControl.DataContextChanged += OnDataContextChanged;
        }

        protected override object GetSpecificValue()
        {
            return m_CurrentControl.DataContext;
        }

        protected override void SetSpecificValue(object value)
        {
            m_CurrentControl.DataContext = value;
        }

        private void OnCustomControlCreated(int id, bool status)
        {
            if (Id == id) 
            {
                if (!status)
                {
                    throw new Exception("Failed to create custom control");
                }
            }
        }

        private void OnDataContextChanged(IXCustomControl ctrl, object newVal)
        {
            ValueChanged?.Invoke(this, newVal);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_Handler.CustomControlCreated -= OnCustomControlCreated;
            }
        }
    }
}