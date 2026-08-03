//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.Toolkit.Blazor.Controls
{
    /// <summary>
    /// Hosts a custom Razor component for a property decorated with
    /// <see cref="CustomControlAttribute"/>. The component type must implement both
    /// <see cref="Microsoft.AspNetCore.Components.IComponent"/> (it's a real Razor component,
    /// typically a .razor file) and <see cref="IXCustomControl"/>
    /// </summary>
    public class BlazorPropertyManagerPageCustomControl : BlazorPropertyManagerPageControl<object>
    {
        protected override event ControlValueChangedDelegate<object> ValueChanged;

        /// <summary>
        /// Type of the custom control, from <see cref="CustomControlAttribute.ControlType"/>
        /// </summary>
        public Type ComponentType { get; }

        private IXCustomControl m_Instance;
        private object m_PendingValue;
        private bool m_HasPendingValue;

        public BlazorPropertyManagerPageCustomControl(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata)
            : base(parentGroup, atts, metadata)
        {
            ComponentType = atts.Get<CustomControlAttribute>().ControlType;

            if (!typeof(IXCustomControl).IsAssignableFrom(ComponentType))
            {
                throw new InvalidCastException(
                    $"{ComponentType.FullName} must implement {typeof(IXCustomControl).FullName}");
            }
        }

        internal void AttachInstance(IXCustomControl instance)
        {
            if (!ReferenceEquals(m_Instance, instance))
            {
                if (m_Instance != null)
                {
                    m_Instance.ValueChanged -= OnInstanceValueChanged;
                }

                m_Instance = instance;

                if (m_Instance != null)
                {
                    m_Instance.ValueChanged += OnInstanceValueChanged;

                    if (m_HasPendingValue)
                    {
                        m_Instance.Value = m_PendingValue;
                        m_HasPendingValue = false;
                    }
                }
            }
        }

        private void OnInstanceValueChanged(IXCustomControl sender, object newValue)
        {
            NotifyInvalidated();
            ValueChanged?.Invoke(this, newValue);
        }

        protected override object GetSpecificValue()
            => m_Instance != null ? m_Instance.Value : m_PendingValue;

        protected override void SetSpecificValue(object value)
        {
            if (m_Instance != null)
            {
                m_Instance.Value = value;
            }
            else
            {
                m_PendingValue = value;
                m_HasPendingValue = true;
            }

            NotifyInvalidated();
        }
    }
}
