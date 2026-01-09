//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Windows;
using System.Windows.Forms.Integration;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Templates;
using Xarial.XCad.UI;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls
{
    internal class WpfPropertyManagerPageCustomControl : WpfPropertyManagerPageControl<object>
    {
        private class PropertyPageControlCreator
            : CustomControlHost<IXCustomControl, UIElement>
        {
            internal PropertyPageControlCreator()
            {
            }

            protected override IXCustomControl HostWpfControl(UIElement wpfCtrl, string title, IXImage image, out UIElement specCtrl)
            {
                specCtrl = wpfCtrl;

                if (wpfCtrl is IXCustomControl)
                {
                    return (IXCustomControl)wpfCtrl;
                }
                else 
                {
                    return new WpfCustomControl((FrameworkElement)(object)wpfCtrl);
                }
                    
            }
        }

        public override DataTemplate Template { get; }

        public UIElement UI { get; }

        protected override event ControlValueChangedDelegate<object> ValueChanged;

        private readonly PropertyPageControlCreator m_CtrlCreator;

        private readonly IXCustomControl m_Ctrl;

        public WpfPropertyManagerPageCustomControl(IGroup parentGroup, IIconsCreator iconConv,
            IAttributeSet atts, IMetadata[] metadata)
            : base(parentGroup, atts, metadata, iconConv)
        {
            Template = ControlTemplates.CustomControl;

            var ctrlType = atts.Get<CustomControlAttribute>().ControlType;

            m_CtrlCreator = new PropertyPageControlCreator();

            m_Ctrl = m_CtrlCreator.HostControl(ctrlType, out var uiElem);

            UI = uiElem;

            m_Ctrl.ValueChanged += OnDataContextChanged;
        }

        protected override object GetSpecificValue()
            => m_Ctrl.Value;

        protected override void SetSpecificValue(object value)
            => m_Ctrl.Value = value;

        private void OnDataContextChanged(IXCustomControl ctrl, object newVal)
            => ValueChanged?.Invoke(this, newVal);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                base.Dispose(disposing);

                m_Ctrl.ValueChanged -= OnDataContextChanged;
            }
        }
    }
}