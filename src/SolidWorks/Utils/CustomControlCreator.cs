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
    internal abstract class CustomControlCreator<TSpecificHost, TControl>
    {
        private void GetControlAttribution(Type ctrlType, out string title, out IXImage icon) 
        {
            title = "";

            if (ctrlType.TryGetAttribute(out DisplayNameAttribute att))
            {
                title = att.DisplayName;
            }

            if (string.IsNullOrEmpty(title))
            {
                title = ctrlType.Name;
            }

            icon = null;

            if (ctrlType.TryGetAttribute(out IconAttribute iconAtt))
            {
                icon = iconAtt.Icon;
            }

            if (icon == null)
            {
                icon = Defaults.Icon;
            }
        }

        protected abstract TSpecificHost HostComControl(string progId, string title, 
            IXImage image, out TControl specCtrl);

        protected abstract TSpecificHost HostNetControl(
            System.Windows.Forms.Control winCtrlHost, TControl ctrl, string title, IXImage image);
        
        public TSpecificHost CreateControl(Type ctrlType, out TControl specCtrl) 
        {
            string title;
            IXImage icon;

            GetControlAttribution(ctrlType, out title, out icon);
            
            if (typeof(System.Windows.Forms.Control).IsAssignableFrom(ctrlType))
            {
                if (typeof(System.Windows.Forms.UserControl).IsAssignableFrom(ctrlType) && ctrlType.IsComVisible())
                {
                    return HostComControl(ctrlType.GetProgId(), title, icon, out specCtrl);
                }
                else
                {
                    var winCtrl = (System.Windows.Forms.Control)Activator.CreateInstance(ctrlType);
                    specCtrl = (TControl)(object)winCtrl;
                    return HostNetControl(winCtrl, specCtrl, title, icon);
                }
            }
            else if (typeof(System.Windows.UIElement).IsAssignableFrom(ctrlType))
            {
                var wpfCtrl = (System.Windows.UIElement)Activator.CreateInstance(ctrlType);
                var host = new System.Windows.Forms.Integration.ElementHost();
                host.Child = wpfCtrl;
                specCtrl = (TControl)(object)wpfCtrl;
                return HostNetControl(host, specCtrl, title, icon);
            }
            else
            {
                throw new NotSupportedException($"Only {typeof(System.Windows.Forms.Control).FullName} or {typeof(System.Windows.UIElement).FullName} are supported");
            }
        }
    }
}