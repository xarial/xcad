//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Windows;
using System.Windows.Interop;

namespace Xarial.XCad.SolidWorks.Utils
{
    /// <summary>
    /// Fixes the blocked keystrokes in WPF controls
    /// </summary>
    /// <remarks>WPF controls hosted in SOLIDWORKS will not handle keystrokes. This class fixes this issue</remarks>
    internal class WpfControlKeystrokePropagator : IDisposable
    {
        private readonly FrameworkElement m_Elem;

        private HwndSource m_HwndSrc;
        private HwndSourceHook m_HwndSrcHook;

        internal WpfControlKeystrokePropagator(FrameworkElement elem) 
        {
            m_Elem = elem;

            m_Elem.Loaded += OnElementLoaded;
            m_Elem.Unloaded += OnElementUnloaded;
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
