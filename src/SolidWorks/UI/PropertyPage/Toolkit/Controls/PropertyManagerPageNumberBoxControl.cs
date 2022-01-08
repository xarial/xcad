//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    internal class PropertyManagerPageNumberBoxControl : PropertyManagerPageBaseControl<double, IPropertyManagerPageNumberbox>
    {
        protected override event ControlValueChangedDelegate<double> ValueChanged;

        public PropertyManagerPageNumberBoxControl(int id, object tag,
            IPropertyManagerPageNumberbox numberBox,
            SwPropertyManagerPageHandler handler, IPropertyManagerPageLabel label, IMetadata[] metadata)
            : base(numberBox, id, tag, handler, label, metadata)
        {
            m_Handler.NumberChanged += OnNumberChanged;
        }

        private void OnNumberChanged(int id, double value)
        {
            if (Id == id)
            {
                ValueChanged?.Invoke(this, value);
            }
        }

        protected override double GetSpecificValue()
        {
            return SwSpecificControl.Value;
        }

        protected override void SetSpecificValue(double value)
        {
            SwSpecificControl.Value = value;
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