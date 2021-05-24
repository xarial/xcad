//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Attributes;
using Xarial.XCad.Utils.PageBuilder.Base;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Constructors
{
    [DefaultType(typeof(bool))]
    internal class PropertyManagerPageCheckBoxControlConstructor
        : PropertyManagerPageBaseControlConstructor<PropertyManagerPageCheckBoxControl, IPropertyManagerPageCheckbox>
    {
        public PropertyManagerPageCheckBoxControlConstructor(ISldWorks app, IIconsCreator iconsConv)
            : base(app, swPropertyManagerPageControlType_e.swControlType_Checkbox, iconsConv)
        {
        }

        protected override PropertyManagerPageCheckBoxControl CreateControl(
            IPropertyManagerPageCheckbox swCtrl, IAttributeSet atts, IMetadata metadata, 
            SwPropertyManagerPageHandler handler, short height)
        {
            swCtrl.Caption = atts.Name;
            
            return new PropertyManagerPageCheckBoxControl(atts.Id, atts.Tag, swCtrl, handler);
        }
    }
}