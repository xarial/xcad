//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

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

        protected abstract event ControlValueChangedDelegate<TVal> ValueChanged;

        private ControlObjectValueChangedDelegate m_ValueChangedHandler;

        public int Id { get; private set; }

        public object Tag { get; private set; }

        protected Control(int id, object tag)
        {
            Id = id;
            Tag = tag;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        object IControl.GetValue()
        {
            return GetSpecificValue();
        }

        public void SetValue(object value)
        {
            var destVal = value.Cast<TVal>();

            SetSpecificValue(destVal);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        protected abstract TVal GetSpecificValue();

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