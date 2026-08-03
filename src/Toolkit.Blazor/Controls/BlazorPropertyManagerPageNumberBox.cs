//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Globalization;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.Toolkit.Blazor.Controls
{
    /// <summary>
    /// Number box control
    /// </summary>
    public class BlazorPropertyManagerPageNumberBox : BlazorPropertyManagerPageControl<object>
    {
        private const string DEFAULT_TEXT = "0";

        protected override event ControlValueChangedDelegate<object> ValueChanged;

        /// <summary>
        /// Last successfully parsed value. Not updated while <see cref="IsInvalid"/> is true -
        /// the model keeps its last valid value until the user types something parseable again
        /// </summary>
        public object Value => m_Value;

        /// <summary>
        /// Raw text currently shown in the box - may be non-numeric while the user is mid-edit
        /// </summary>
        public string Text => m_Text;

        /// <summary>
        /// True when <see cref="Text"/> could not be parsed as a number
        /// </summary>
        public bool IsInvalid { get; private set; }

        private object m_Value;
        private string m_Text;

        private readonly Type m_ValueType;

        public BlazorPropertyManagerPageNumberBox(IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata)
            : base(parentGroup, atts, metadata)
        {
            m_ValueType = atts.ContextType;
            m_Text = DEFAULT_TEXT;
        }

        internal void SetTextFromUi(string text)
        {
            object value;
            bool isValid;

            try
            {
                value = Convert.ChangeType(text, m_ValueType);
                isValid = true;
            }
            catch 
            {
                value = null;
                isValid = false;
            }
            
            m_Text = text;

            if (isValid)
            {
                IsInvalid = false;

                if (!Equals(m_Value, value))
                {
                    m_Value = value;
                    NotifyInvalidated();
                    ValueChanged?.Invoke(this, value);
                    return;
                }
            }
            else
            {
                IsInvalid = true;
            }

            NotifyInvalidated();
        }

        internal void Increment() => Step(1);

        internal void Decrement() => Step(-1);

        private void Step(int direction)
        {
            decimal current;

            try
            {
                current = m_Value != null ? Convert.ToDecimal(m_Value, CultureInfo.InvariantCulture) : 0m;
            }
            catch
            {
                current = 0m;
            }

            SetTextFromUi((current + direction).ToString(CultureInfo.InvariantCulture));
        }

        protected override object GetSpecificValue() => Value;

        protected override void SetSpecificValue(object value)
        {
            m_Value = Convert.ChangeType(value ?? DEFAULT_TEXT, m_ValueType);
            m_Text = m_Value?.ToString();
            IsInvalid = false;
            NotifyInvalidated();
        }
    }
}
