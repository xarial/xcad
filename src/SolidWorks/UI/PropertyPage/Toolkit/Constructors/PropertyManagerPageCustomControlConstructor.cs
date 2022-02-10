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
using Xarial.XCad.SolidWorks.UI.Commands.Exceptions;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.SolidWorks.UI.Toolkit;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Attributes;
using Xarial.XCad.Utils.PageBuilder.Base;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Constructors
{
    internal class PropertyManagerPageCustomControlConstructor
        : PropertyManagerPageBaseControlConstructor<PropertyManagerPageCustomControl, IPropertyManagerPageWindowFromHandle>, ICustomControlConstructor
    {
        public PropertyManagerPageCustomControlConstructor(ISldWorks app, IIconsCreator iconsConv)
            : base(app, swPropertyManagerPageControlType_e.swControlType_WindowFromHandle, iconsConv)
        {
        }

        protected override PropertyManagerPageCustomControl CreateControl(
            IPropertyManagerPageWindowFromHandle swCtrl, IAttributeSet atts, IMetadata[] metadata, 
            SwPropertyManagerPageHandler handler, short height, IPropertyManagerPageLabel label)
        {
            if (height <= 0)
            {
                height = 50;
            }

            swCtrl.Height = height;

            var ctrlType = atts.Get<CustomControlAttribute>().ControlType;

            return new PropertyManagerPageCustomControl(ctrlType, atts.Id, atts.Tag,
                swCtrl, handler, new PropertyPageControlCreator<object>(swCtrl), label, metadata);
        }
    }
}