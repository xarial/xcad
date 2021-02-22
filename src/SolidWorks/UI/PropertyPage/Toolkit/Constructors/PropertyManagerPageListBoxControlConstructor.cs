//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System.Collections;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Utils.PageBuilder.Attributes;
using Xarial.XCad.Utils.PageBuilder.Base;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Constructors
{
    [DefaultType(typeof(IEnumerable))]
    internal class PropertyManagerPageListBoxControlConstructor
        : PropertyManagerPageBaseControlConstructor<PropertyManagerPageListBoxControl, IPropertyManagerPageListbox>
    {
        public PropertyManagerPageListBoxControlConstructor(ISldWorks app, IIconsCreator iconsConv)
            : base(app, swPropertyManagerPageControlType_e.swControlType_Listbox, iconsConv)
        {
        }

        protected override PropertyManagerPageListBoxControl CreateControl(
            IPropertyManagerPageListbox swCtrl, IAttributeSet atts, SwPropertyManagerPageHandler handler, short height)
        {
            swCtrl.Height = height;
            return new PropertyManagerPageListBoxControl(atts.Id, atts.Tag, swCtrl, handler);
        }
    }
}