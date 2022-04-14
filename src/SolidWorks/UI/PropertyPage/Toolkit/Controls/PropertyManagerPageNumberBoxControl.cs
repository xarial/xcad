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
    internal class PropertyManagerPageNumberBoxControl : PropertyManagerPageBaseControl<object, IPropertyManagerPageNumberbox>
    {
        protected override event ControlValueChangedDelegate<object> ValueChanged;

        private readonly Type m_ValType;

        public PropertyManagerPageNumberBoxControl(int id, object tag, Type valType,
            IPropertyManagerPageNumberbox numberBox,
            SwPropertyManagerPageHandler handler, IPropertyManagerPageLabel label, IMetadata[] metadata)
            : base(numberBox, id, tag, handler, label, metadata)
        {
            m_ValType = valType;
            m_Handler.NumberChanged += OnNumberChanged;
        }

        private void OnNumberChanged(int id, double value)
        {
            if (Id == id)
            {
                ValueChanged?.Invoke(this, value);
            }
        }

        protected override object GetSpecificValue()
        {
            var val = SwSpecificControl.Value;

            if (m_ValType == typeof(double))
            {
                return val;
            }
            else 
            {
                return Convert.ChangeType(val, m_ValType);
            }
        }

        protected override void SetSpecificValue(object value)
        {
            double val;

            if (value is double)
            {
                val = (double)value;
            }
            else 
            {
                val = (double)Convert.ChangeType(value, typeof(double));
            }

            SwSpecificControl.Value = val;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_Handler.NumberChanged -= OnNumberChanged;
            }
        }
    }
}