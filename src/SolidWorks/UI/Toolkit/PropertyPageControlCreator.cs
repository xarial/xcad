//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Xarial.XCad.SolidWorks.UI.Commands.Exceptions;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Windows.UI;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.UI;
using Xarial.XCad.UI.PropertyPage;

namespace Xarial.XCad.SolidWorks.UI.Toolkit
{
    internal class PropertyPageControlCreator<TControl>
        : CustomControlHost<IXCustomControl, TControl>
    {
        private readonly IPropertyManagerPageWindowFromHandle m_PmpCtrl;

        internal PropertyPageControlCreator(IPropertyManagerPageWindowFromHandle pmpCtrl)
        {
            m_PmpCtrl = pmpCtrl;
        }

        protected override IXCustomControl HostWinFormsControl(Control winCtrl, string title, IXImage image)
        {
            if (m_PmpCtrl.SetWindowHandlex64(winCtrl.Handle.ToInt64()))
            {
                TControl ctrl;

                if (winCtrl is ElementHost elemHost)
                {
                    ctrl = (TControl)(object)elemHost.Child;
                }
                else 
                {
                    ctrl = (TControl)(object)winCtrl;
                }

                if (ctrl is IXCustomControl)
                {
                    if (ctrl is System.Windows.FrameworkElement)
                    {
                        return new WpfCustomControlWrapper((IXCustomControl)ctrl);
                    }
                    else
                    {
                        return (IXCustomControl)ctrl;
                    }
                }
                else
                {
                    if (ctrl is System.Windows.FrameworkElement)
                    {
                        return new WpfCustomControl((System.Windows.FrameworkElement)(object)ctrl, winCtrl);
                    }

                    throw new NotSupportedException($"'{ctrl.GetType()}' must implement '{typeof(IXCustomControl).FullName}' or inherit '{typeof(System.Windows.FrameworkElement).FullName}'");
                }
            }
            else
            {
                throw new NetControlHostException(winCtrl.Handle);
            }
        }

        protected override IXCustomControl HostComControl(string progId, string title, IXImage image, out TControl specCtrl)
            => throw new NotImplementedException("ActiveX controls are not implemented yet");
    }
}
