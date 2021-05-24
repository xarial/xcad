//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Xarial.XCad.SolidWorks.UI.Commands.Exceptions;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI;
using Xarial.XCad.UI.PropertyPage;

namespace Xarial.XCad.SolidWorks.UI.Toolkit
{
    internal class PropertyPageControlCreator<TControl>
        : CustomControlCreator<IXCustomControl, TControl>
    {
        private readonly IPropertyManagerPageWindowFromHandle m_PmpCtrl;

        internal PropertyPageControlCreator(IPropertyManagerPageWindowFromHandle pmpCtrl)
        {
            m_PmpCtrl = pmpCtrl;
        }

        protected override IXCustomControl HostNetControl(Control winCtrlHost, TControl ctrl, string title, IXImage image)
        {
            if (m_PmpCtrl.SetWindowHandlex64(winCtrlHost.Handle.ToInt64()))
            {
                if (ctrl is IXCustomControl)
                {
                    return (IXCustomControl)ctrl;
                }
                else
                {
                    if (ctrl is System.Windows.FrameworkElement)
                    {
                        return new WpfCustomControl((System.Windows.FrameworkElement)(object)ctrl, winCtrlHost);
                    }

                    throw new NotSupportedException($"'{ctrl.GetType()}' must implement '{typeof(IXCustomControl).FullName}' or inherit '{typeof(System.Windows.FrameworkElement).FullName}'");
                }
            }
            else
            {
                throw new NetControlHostException(winCtrlHost.Handle);
            }
        }

        protected override IXCustomControl HostComControl(string progId, string title, IXImage image, out TControl specCtrl)
            => throw new NotImplementedException("ActiveX controls are not implemented yet");
    }
}
