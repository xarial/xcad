//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI.PropertyPage;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    internal class WpfCustomControl : IXCustomControl, IDisposable
    {
        public event Action<IXCustomControl, object> DataContextChanged;

        private readonly FrameworkElement m_Elem;

        private readonly WpfControlKeystrokePropagator m_KeystrokePropagator;

        internal WpfCustomControl(FrameworkElement elem) 
        {
            m_Elem = elem;
            m_KeystrokePropagator = new WpfControlKeystrokePropagator(elem);
        }

        public object DataContext 
        {
            get => m_Elem.DataContext;
            set => m_Elem.DataContext = value;
        }

        public void Dispose() 
        {
            m_KeystrokePropagator.Dispose();
        }
    }
}