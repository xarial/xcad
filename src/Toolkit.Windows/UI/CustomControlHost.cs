//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using Xarial.XCad;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Reflection;
using Xarial.XCad.UI;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.Toolkit.Windows.UI
{        
    /// <summary>
    /// Helper class to host custom controls
    /// </summary>
    /// <typeparam name="TSpecificHost">Specific host (e.g. <see cref="IXTaskPane{TControl}"/>, <see cref="IXPropertyPage"/>, etc.</typeparam>
    /// <typeparam name="TControl">Specific control type</typeparam>
    public abstract class CustomControlHost<TSpecificHost, TControl>
    {
        /// <summary>
        /// Host control
        /// </summary>
        /// <param name="ctrlType">Type of control to host</param>
        /// <param name="specCtrl">Specific control created</param>
        /// <returns>Instance of the host with the control</returns>
        /// <exception cref="NotSupportedException">Type of the control is not supported</exception>
        public TSpecificHost HostControl(Type ctrlType, out TControl specCtrl)
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
                    return HostWinFormsControl(winCtrl, title, icon, out specCtrl);
                }
            }
            else if (typeof(System.Windows.UIElement).IsAssignableFrom(ctrlType))
            {
                var wpfCtrl = (System.Windows.UIElement)Activator.CreateInstance(ctrlType);
                return HostWpfControl(wpfCtrl, title, icon, out specCtrl);
            }
            else
            {
                throw new NotSupportedException($"Only {typeof(System.Windows.Forms.Control).FullName} or {typeof(System.Windows.UIElement).FullName} are supported");
            }
        }

        protected virtual TSpecificHost HostComControl(string progId, string title,
            IXImage image, out TControl specCtrl)
        {
            throw new NotSupportedException();
        }

        protected virtual TSpecificHost HostWinFormsControl(
            System.Windows.Forms.Control winCtrl, string title,
            IXImage image, out TControl specCtrl)
        {
            specCtrl = (TControl)(object)winCtrl;
            return HostWinFormsControl(winCtrl, title, image);
        }

        protected virtual TSpecificHost HostWpfControl(
            System.Windows.UIElement wpfCtrl, string title,
            IXImage image, out TControl specCtrl)
        {
            var host = new System.Windows.Forms.Integration.ElementHost();
            host.Child = wpfCtrl;
            specCtrl = (TControl)(object)wpfCtrl;

            return HostWinFormsControl(host, title, image);
        }

        protected virtual TSpecificHost HostWpfControl(
            System.Windows.UIElement wpfCtrl, string title,
            IXImage image)
        {
            return HostWpfControl(wpfCtrl, title, image);
        }

        protected virtual TSpecificHost HostWinFormsControl(
            System.Windows.Forms.Control winCtrl, string title,
            IXImage image)
        {
            throw new NotSupportedException();
        }

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
    }
}