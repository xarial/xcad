//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    internal class PropertyManagerPageNumberBoxControl : PropertyManagerPageBaseControl<object, IPropertyManagerPageNumberbox>
    {
        protected override event ControlValueChangedDelegate<object> ValueChanged;

        private Type m_ValType;

        public PropertyManagerPageNumberBoxControl(SwApplication app, IGroup parentGroup, IIconsCreator iconConv,
            IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            : base(app, parentGroup, iconConv, atts, metadata, swPropertyManagerPageControlType_e.swControlType_Numberbox, ref numberOfUsedIds)
        {
            m_Handler.NumberChanged += OnNumberChanged;
        }

        protected override void InitData(IControlOptionsAttribute opts, IAttributeSet atts)
        {
            m_ValType = atts.ContextType;
        }

        protected override void SetOptions(IPropertyManagerPageNumberbox ctrl, IControlOptionsAttribute opts, IAttributeSet atts)
        {
            var height = opts.Height;

            if (height != -1)
            {
                ctrl.Height = height;
            }

            if (atts.Has<NumberBoxOptionsAttribute>())
            {
                var style = atts.Get<NumberBoxOptionsAttribute>();

                if (style.Style != 0)
                {
                    SwSpecificControl.Style = (int)style.Style;
                }

                if (style.Units != 0)
                {
                    ctrl.SetRange2((int)style.Units, style.Minimum, style.Maximum,
                        style.Inclusive, style.Increment, style.FastIncrement, style.SlowIncrement);
                }
            }
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