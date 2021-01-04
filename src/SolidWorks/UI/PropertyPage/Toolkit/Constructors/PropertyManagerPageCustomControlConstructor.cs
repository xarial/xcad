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
using Xarial.XCad.SolidWorks.UI.Commands.Exceptions;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.SolidWorks.UI.Toolkit;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Attributes;
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
            IPropertyManagerPageWindowFromHandle swCtrl, IAttributeSet atts, SwPropertyManagerPageHandler handler, short height)
        {
            if (height <= 0)
            {
                height = 50;
            }

            swCtrl.Height = height;

            var ctrlType = atts.Get<CustomControlAttribute>().ControlType;

            //var ctrlFact = new Func<IXCustomControl>(() =>
            //    CustomControlHelperOld.HostControl(ctrlType,
            //    (c, h, t, _) =>
            //    {
            //        if (swCtrl.SetWindowHandlex64(h.Handle.ToInt64()))
            //        {
            //            if (c is IXCustomControl)
            //            {
            //                return (IXCustomControl)c;
            //            }
            //            else
            //            {
            //                if (c is System.Windows.FrameworkElement)
            //                {
            //                    return new WpfCustomControl((System.Windows.FrameworkElement)c, h);
            //                }

            //                throw new NotSupportedException($"'{c.GetType()}' must implement '{typeof(IXCustomControl).FullName}' or inherit '{typeof(System.Windows.FrameworkElement).FullName}'");
            //            }
            //        }
            //        else
            //        {
            //            throw new NetControlHostException(h.Handle);
            //        }
            //    },
            //    (p, t, _) =>
            //    {
            //        throw new NotImplementedException("ActiveX controls are not implemented yet");
            //    }));

            return new PropertyManagerPageCustomControl(ctrlType, atts.Id, atts.Tag,
                swCtrl, handler, new PropertyPageControlCreator<object>(swCtrl));
        }
    }
}