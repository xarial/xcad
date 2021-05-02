//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Attributes;
using Xarial.XCad.Utils.PageBuilder.Base;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Constructors
{
    [DefaultType(typeof(Action))]
    internal class PropertyManagerPageButtonControlConstructor
        : PropertyManagerPageBaseControlConstructor<PropertyManagerPageButtonControl, IPropertyManagerPageButton>
    {
        public PropertyManagerPageButtonControlConstructor(ISldWorks app, IIconsCreator iconsConv)
            : base(app, swPropertyManagerPageControlType_e.swControlType_Button, iconsConv)
        {
        }

        protected override PropertyManagerPageButtonControl CreateControl(
            IPropertyManagerPageButton swCtrl, IAttributeSet atts, IMetadata metadata, 
            SwPropertyManagerPageHandler handler, short height)
        {
            swCtrl.Caption = atts.Name;

            return new PropertyManagerPageButtonControl(atts.Id, atts.Tag, swCtrl, handler);
        }
    }
}