//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System.Collections;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Structures;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    internal class PropertyManagerPageListBoxControl : PropertyManagerPageBaseControl<object, IPropertyManagerPageListbox>, IItemsControl
    {
        public ItemsControlItem[] Items { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        protected override event ControlValueChangedDelegate<object> ValueChanged;

        public PropertyManagerPageListBoxControl(int id, object tag,
            IPropertyManagerPageListbox listBox,
            SwPropertyManagerPageHandler handler) : base(listBox, id, tag, handler)
        {
            //m_Handler.CheckChanged += OnCheckChanged;
        }

        //private void OnCheckChanged(int id, bool isChecked)
        //{
        //    if (Id == id)
        //    {
        //        ValueChanged?.Invoke(this, isChecked);
        //    }
        //}

        protected override object GetSpecificValue()
        {
            return null;
        }

        protected override void SetSpecificValue(object value)
        {
            //SwSpecificControl.Checked = value;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //m_Handler.CheckChanged -= OnCheckChanged;
            }
        }
    }
}