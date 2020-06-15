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

        private readonly IXCustomControl m_Control;

        internal PropertyManagerPageCustomControl(int id, object tag,
            IPropertyManagerPageWindowFromHandle wndFromHandler,
            SwPropertyManagerPageHandler handler, IXCustomControl control) : base(wndFromHandler, id, tag, handler)
        {
            m_Handler.CustomControlCreated += OnCustomControlCreated;
            m_Control = control;
            m_Control.DataContextChanged += OnDataContextChanged;
        }

        protected override object GetSpecificValue()
        {
            return m_Control.DataContext;
        }

        protected override void SetSpecificValue(object value)
        {
            m_Control.DataContext = value;
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