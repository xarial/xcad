#if NET461
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using Xarial.XCad.UI.PropertyPage;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    internal class WpfCustomControl : IXCustomControl, IDisposable
    {
        public event Action<IXCustomControl, object> DataContextChanged;

        private readonly FrameworkElement m_Elem;
        private HwndSource m_HwndSrc;
        private HwndSourceHook m_HwndSrcHook;

        internal WpfCustomControl(FrameworkElement elem) 
        {
            m_Elem = elem;
            m_Elem.Loaded += OnElementLoaded;
            m_Elem.Unloaded += OnElementUnloaded;
        }

        public object DataContext 
        {
            get => m_Elem.DataContext;
            set => m_Elem.DataContext = value;
        }

        private void OnElementLoaded(object sender, RoutedEventArgs e)
        {
            //If messages are not passed than all keystrokes will be ignored
            m_HwndSrc = HwndSource.FromVisual(m_Elem) as HwndSource;

            if (m_HwndSrc != null)
            {
                m_HwndSrcHook = new HwndSourceHook(OnChildHwndSourceHook);
                m_HwndSrc.AddHook(m_HwndSrcHook);
            }
            else
            {
                throw new Exception("Failed to create HwndSource");
            }
        }

        private void OnElementUnloaded(object sender, RoutedEventArgs e)
        {
            m_HwndSrc.RemoveHook(m_HwndSrcHook);
        }

        private IntPtr OnChildHwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const uint DLGC_WANTARROWS = 0x0001;
            const uint DLGC_HASSETSEL = 0x0008;
            const uint DLGC_WANTCHARS = 0x0080;
            const uint WM_GETDLGCODE = 0x0087;

            if (msg == WM_GETDLGCODE)
            {
                handled = true;
                return new IntPtr(DLGC_WANTCHARS | DLGC_WANTARROWS | DLGC_HASSETSEL);
            }

            return IntPtr.Zero;
        }

        public void Dispose()
        {
            m_Elem.Loaded -= OnElementLoaded;
            m_Elem.Unloaded -= OnElementUnloaded;
        }
    }
}
#endif