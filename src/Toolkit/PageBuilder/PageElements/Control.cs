//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Diagnostics;
using System.Linq;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.Utils.PageBuilder.PageElements
{
    public delegate void ControlValueChangedDelegate<TVal>(Control<TVal> sender, TVal newValue);

    public abstract class Control<TVal> : IControl
    {
        event ControlObjectValueChangedDelegate IControl.ValueChanged
        {
            add
            {
                this.ValueChanged += OnValueChanged;
                m_ValueChangedHandler += value;
            }
            remove
            {
                this.ValueChanged -= OnValueChanged;
                m_ValueChangedHandler -= value;
            }
        }

        object IControl.GetValue() => GetSpecificValue();

        /// <inheritdoc/>
        protected abstract event ControlValueChangedDelegate<TVal> ValueChanged;

        private ControlObjectValueChangedDelegate m_ValueChangedHandler;

        /// <inheritdoc/>
        public int Id { get; private set; }

        /// <inheritdoc/>
        public object Tag { get; private set; }

        /// <inheritdoc/>
        public abstract bool Enabled { get; set; }

        /// <inheritdoc/>
        public abstract bool Visible { get; set; }

        /// <inheritdoc/>
        public IMetadata[] Metadata { get; }

        public virtual Type ValueType => typeof(TVal);

        protected Control(int id, object tag, IMetadata[] metadata)
        {
            Id = id;
            Tag = tag;
            Metadata = metadata;
        }

        /// <inheritdoc/>
        public virtual void Update() 
        {
        }

        public void Dispose() => Dispose(true);

        /// <inheritdoc/>
        public void SetValue(object value)
        {
            var destVal = (TVal)value.Cast(ValueType);

            SetSpecificValue(destVal);
        }

        /// <inheritdoc/>
        public abstract void ShowTooltip(string title, string msg);

        /// <inheritdoc/>
        public abstract void Focus();

        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Gets control specific value
        /// </summary>
        /// <returns></returns>
        protected abstract TVal GetSpecificValue();

        /// <summary>
        /// Sets the control specific value
        /// </summary>
        /// <param name="value">Value to set</param>
        protected abstract void SetSpecificValue(TVal value);

        private void OnValueChanged(Control<TVal> sender, TVal newValue)
        {
            if (m_ValueChangedHandler != null)
            {
                m_ValueChangedHandler.Invoke(sender, newValue);
            }
            else
            {
                Debug.Assert(false, "Generic event handler and specific event handler should be synchronised");
            }
        }
    }
}