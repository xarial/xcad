//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
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
        public event Action<IXCustomControl, object> ValueChanged;

        private readonly System.Windows.Forms.Control m_Host;
        private readonly FrameworkElement m_Elem;

        private readonly WpfControlKeystrokePropagator m_KeystrokePropagator;

        internal WpfCustomControl(FrameworkElement elem, System.Windows.Forms.Control host) 
        {
            m_Elem = elem;
            m_Elem.DataContextChanged += OnDataContextChanged;
            m_Host = host;
            m_KeystrokePropagator = new WpfControlKeystrokePropagator(elem);
        }

        public object Value 
        {
            get => m_Elem.DataContext;
            set => m_Elem.DataContext = value;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
            => ValueChanged?.Invoke(this, e.NewValue);

        public void Dispose() 
        {
            m_KeystrokePropagator.Dispose();
        }
    }
}