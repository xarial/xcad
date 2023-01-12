//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.Toolkit;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    //TODO: add support to IPropertyManagerPageActiveX
    internal class PropertyManagerPageCustomControl : PropertyManagerPageBaseControl<object, IPropertyManagerPageWindowFromHandle>
    {
        protected override event ControlValueChangedDelegate<object> ValueChanged;

        private IXCustomControl m_CurrentControl;

        private readonly PropertyPageControlCreator<object> m_Creator;

        private Type m_CtrlType;

        internal PropertyManagerPageCustomControl(SwApplication app, IGroup parentGroup, IIconsCreator iconConv,
            IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            : base(app, parentGroup, iconConv, atts, metadata, swPropertyManagerPageControlType_e.swControlType_WindowFromHandle, ref numberOfUsedIds)
        {
            m_Handler.CustomControlCreated += OnCustomControlCreated;
            m_Handler.Opening += OnPageOpening;
            m_Handler.PreClosed += OnPageClosed;

            m_Creator = new PropertyPageControlCreator<object>(SwSpecificControl);
        }

        protected override void InitData(IControlOptionsAttribute opts, IAttributeSet atts)
        {
            m_CtrlType = atts.Get<CustomControlAttribute>().ControlType;
        }

        protected override void SetOptions(IPropertyManagerPageWindowFromHandle ctrl, IControlOptionsAttribute opts, IAttributeSet atts)
        {
            var height = opts.Height;

            if (height <= 0)
            {
                height = 50;
            }

            ctrl.Height = height;
        }

        private void OnPageOpening()
        {
            if (m_CurrentControl != null)
            {
                m_CurrentControl.ValueChanged -= OnDataContextChanged;
            }

            m_CurrentControl = m_Creator.CreateControl(m_CtrlType, out _);
            m_CurrentControl.ValueChanged += OnDataContextChanged;
        }

        private void OnPageClosed(swPropertyManagerPageCloseReasons_e reason)
        {
            if (m_CurrentControl is IDisposable)
            {
                ((IDisposable)m_CurrentControl).Dispose();
            }
        }

        protected override object GetSpecificValue()
            => m_CurrentControl.Value;

        protected override void SetSpecificValue(object value)
            => m_CurrentControl.Value = value;

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
            => ValueChanged?.Invoke(this, newVal);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_Handler.CustomControlCreated -= OnCustomControlCreated;
            }
        }
    }
}