//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Reflection;
using Xarial.XCad.UI;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.Utils
{
    internal static class CustomControlHelper
    {
        internal static TWrapper HostControl<TControl, TWrapper>(
                Func<TControl, System.Windows.Forms.Control, string, IXImage, TWrapper> ctrlHost,
                Func<string, string, IXImage, TWrapper> comCtrlHost)
        {
            return HostControl<TWrapper>(typeof(TControl), (c, h, t, i) 
                => ctrlHost.Invoke((TControl)c, h, t, i), comCtrlHost);
        }

        internal static TWrapper HostControl<TWrapper>(Type ctrlType,
                Func<object, System.Windows.Forms.Control, string, IXImage, TWrapper> ctrlHost,
                Func<string, string, IXImage, TWrapper> comCtrlHost)
        {
            var title = "";

            if (ctrlType.TryGetAttribute(out DisplayNameAttribute att))
            {
                title = att.DisplayName;
            }

            if (string.IsNullOrEmpty(title))
            {
                title = ctrlType.Name;
            }

            IXImage icon = null;

            if (ctrlType.TryGetAttribute(out IconAttribute iconAtt))
            {
                icon = iconAtt.Icon;
            }

            if (icon == null)
            {
                icon = Defaults.Icon;
            }

            if (typeof(System.Windows.Forms.Control).IsAssignableFrom(ctrlType))
            {
                if (typeof(System.Windows.Forms.UserControl).IsAssignableFrom(ctrlType) && ctrlType.IsComVisible())
                {
                    return comCtrlHost.Invoke(ctrlType.GetProgId(), title, icon);
                }
                else
                {
                    var winCtrl = (System.Windows.Forms.Control)Activator.CreateInstance(ctrlType);
                    return ctrlHost.Invoke(winCtrl, winCtrl, title, icon);
                }
            }
            else if (typeof(System.Windows.UIElement).IsAssignableFrom(ctrlType))
            {
                var wpfCtrl = (System.Windows.UIElement)Activator.CreateInstance(ctrlType);
                var host = new System.Windows.Forms.Integration.ElementHost();
                host.Child = wpfCtrl;
                return ctrlHost.Invoke(wpfCtrl, host, title, icon);
            }
            else
            {
                throw new NotSupportedException($"Only {typeof(System.Windows.Forms.Control).FullName} or {typeof(System.Windows.UIElement).FullName} are supported");
            }
        }
    }
}