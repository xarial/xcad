using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;

namespace Xarial.XCad.Tests.Common
{
    public class PopupKiller : IDisposable
    {
        #region Windows API

        private delegate bool EnumThreadProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool EnumThreadWindows(int threadId, EnumThreadProc pfnEnum, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", ExactSpelling = true)]
        private static extern IntPtr GetAncestor(IntPtr hwnd, uint flags);

        [DllImport("user32.dll", SetLastError = false)]
        private static extern IntPtr GetDesktopWindow();

        #endregion

        private const string POPUP_CLASS_NAME = "#32770";
        private const int PING = 5000;

        private Timer m_Timer;
        private Process m_Process;
        
        private readonly object m_Lock;

        public PopupKiller(IXApplication app)
        {
            m_Lock = new object();

            m_Process = app.Process;

            m_Timer = new Timer(OnTimer, null, PING, PING);
        }

        private void OnTimer(object state)
        {
            KillPopupIfShown();
        }

        private void KillPopupIfShown()
        {
            if (Monitor.TryEnter(m_Lock))
            {
                try
                {
                    foreach (ProcessThread thread in m_Process.Threads)
                    {
                        var callbackProc = new EnumThreadProc(EnumThreadWindowsCallback);
                        EnumThreadWindows(thread.Id, callbackProc, IntPtr.Zero);
                    }
                }
                finally
                {
                    Monitor.Exit(m_Lock);
                }
            }
        }

        private bool EnumThreadWindowsCallback(IntPtr hWnd, IntPtr lParam)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_CLOSE = 0xF060;

            var className = new StringBuilder(256);
            GetClassName(hWnd, className, className.Capacity);

            if (className.ToString() == POPUP_CLASS_NAME)
            {
                if (IsWindow(hWnd) && IsModalPopup(hWnd))
                {
                    var close = true;

                    if (close)
                    {
                        SendMessage(hWnd, WM_SYSCOMMAND, SC_CLOSE, 0);
                    }

                    return false;
                }
            }

            return true;
        }

        private bool IsModalPopup(IntPtr hwnd)
        {
            const uint GW_OWNER = 4;
            const int GWL_STYLE = -16;
            const uint WS_DISABLED = 0x8000000;
            const uint GA_PARENT = 1;

            return GetAncestor(hwnd, GA_PARENT) == GetDesktopWindow()
                && (GetWindowLong(GetWindow(hwnd, GW_OWNER), GWL_STYLE) & WS_DISABLED) != 0;
        }

        public void Dispose() => m_Timer?.Dispose();
    }
}
