//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
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
        public event CustomControlValueChangedDelegate ValueChanged;

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
            
            m_Host.Dispose();
        }
    }

    internal class WpfCustomControlWrapper : IXCustomControl, IDisposable
    {
        public event CustomControlValueChangedDelegate ValueChanged 
        {
            add 
            {
                m_CustomWpfControl.ValueChanged += value;
            }
            remove 
            {
                m_CustomWpfControl.ValueChanged -= value;
            }
        }

        private readonly IXCustomControl m_CustomWpfControl;

        private readonly WpfControlKeystrokePropagator m_KeystrokePropagator;

        internal WpfCustomControlWrapper(IXCustomControl customWpfControl)
        {
            m_CustomWpfControl = customWpfControl;
            m_KeystrokePropagator = new WpfControlKeystrokePropagator((FrameworkElement)customWpfControl);
        }

        public object Value
        {
            get => m_CustomWpfControl.Value;
            set => m_CustomWpfControl.Value = value;
        }
        
        public void Dispose()
        {
            m_KeystrokePropagator.Dispose();
        }
    }
}