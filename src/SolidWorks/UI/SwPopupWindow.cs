//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using Xarial.XCad.UI;
using Xarial.XCad.UI.PopupWindow.Delegates;

namespace Xarial.XCad.SolidWorks.UI
{
    public class Win32Window : IWin32Window
    {
        public IntPtr Handle { get; }

        public Win32Window(IntPtr handle)
        {
            Handle = handle;
        }
    }

    public interface ISwPopupWindow<TWindow> : IXPopupWindow<TWindow>, IDisposable 
    {
    }

    internal abstract class SwPopupWindow<TWindow> : ISwPopupWindow<TWindow>
    {
        public abstract bool IsActive { get; set; }
        public abstract TWindow Control { get; }

        public abstract event PopupWindowClosedDelegate<TWindow> Closed;
        public abstract event ControlCreatedDelegate<TWindow> ControlCreated;
        public abstract event PanelActivatedDelegate<TWindow> Activated;

        public bool IsControlCreated { get; protected set; }

        public abstract void Close();

        public abstract void Dispose();

        public abstract bool? ShowDialog();
        public abstract void Show();
    }

    internal static class SwPopupWpfWindowWinAPI
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);
    }

    internal class SwPopupWpfWindow<TWindow> : SwPopupWindow<TWindow>
    {
        public override event PopupWindowClosedDelegate<TWindow> Closed;
        public override event ControlCreatedDelegate<TWindow> ControlCreated;
        public override event PanelActivatedDelegate<TWindow> Activated;

        public override bool IsActive
        {
            get => m_WpfWindow.IsVisible;
            set
            {
                if (value)
                {
                    Show();
                }
                else
                {
                    m_WpfWindow.Hide();
                } 
            }
        }

        public override TWindow Control { get; }

        private readonly Window m_WpfWindow;

        private readonly System.Windows.Interop.WindowInteropHelper m_Owner;

        private bool m_IsDisposed;

        private readonly IntPtr m_ParentWnd;

        internal SwPopupWpfWindow(TWindow wpfWindow, IntPtr parent) 
        {
            m_ParentWnd = parent;

            Control = wpfWindow;
            m_WpfWindow = (Window)(object)wpfWindow;
            m_WpfWindow.Activated += OnWindowActivated;
            m_WpfWindow.Closed += OnWpfWindowClosed;
            m_Owner = new System.Windows.Interop.WindowInteropHelper(m_WpfWindow);
            m_Owner.Owner = parent;

            m_IsDisposed = false;
        }

        private void OnWindowActivated(object sender, EventArgs e)
        {
            m_WpfWindow.Activated -= OnWindowActivated;
            ControlCreated?.Invoke(Control);
            Activated?.Invoke(this);
            IsControlCreated = true;
        }

        private void OnWpfWindowClosed(object sender, EventArgs e)
        {
            Closed?.Invoke(this);
            IsControlCreated = false;
        }

        public override void Dispose()
        {
            Close();
        }

        public override void Close()
        {
            if (!m_IsDisposed)
            {
                m_IsDisposed = true;
                m_WpfWindow.Close();
            }
        }

        public override bool? ShowDialog()
        {
            var startupLoc = m_WpfWindow.WindowStartupLocation;

            PositionWindow();

            var res = m_WpfWindow.ShowDialog();

            m_WpfWindow.WindowStartupLocation = startupLoc;

            return res;
        }

        public override void Show()
        {
            var startupLoc = m_WpfWindow.WindowStartupLocation;

            PositionWindow();

            m_WpfWindow.Show();
            m_WpfWindow.BringIntoView();

            m_WpfWindow.WindowStartupLocation = startupLoc;
        }

        /// <remarks>
        /// CenterOwner does not properly position the WPF on the Win32 parent
        /// Instead calculating and setting the position manually
        /// </remarks>
        private void PositionWindow() 
        {
            if (m_WpfWindow.WindowStartupLocation == WindowStartupLocation.CenterOwner)
            {
                if (m_ParentWnd != IntPtr.Zero)
                {
                    SwPopupWpfWindowWinAPI.GetWindowRect(m_ParentWnd, out var rect);

                    Point pos;

                    using (var graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
                    {
                        const int DPI = 96;

                        var scaleX = graphics.DpiX / DPI;
                        var scaleY = graphics.DpiY / DPI;

                        var left = rect.Left / scaleX;
                        var top = rect.Top / scaleY;

                        var wndWidth = (rect.Right - rect.Left) / scaleX;
                        var wndHeight = (rect.Bottom - rect.Top) / scaleY;

                        pos = new Point(left + wndWidth / 2 - m_WpfWindow.Width / 2, top + wndHeight / 2 - m_WpfWindow.Height / 2);
                    }

                    m_WpfWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                    m_WpfWindow.Left = pos.X;
                    m_WpfWindow.Top = pos.Y;
                }
            }
        }
    }

    internal class SwPopupWinForm<TControl> : SwPopupWindow<TControl>
    {
        public override bool IsActive
        {
            get => m_Form.Visible;
            set
            {
                if (value)
                {
                    Show();
                }
                else
                {
                    m_Form.Hide();
                }
            }
        }

        public override TControl Control { get; }

        private readonly Form m_Form;

        private readonly IWin32Window m_Owner;

        public override event PopupWindowClosedDelegate<TControl> Closed;
        public override event ControlCreatedDelegate<TControl> ControlCreated;
        public override event PanelActivatedDelegate<TControl> Activated;

        private bool m_IsDisposed;

        internal SwPopupWinForm(TControl winForm, IntPtr parent)
        {
            Control = winForm;
            m_Form = (Form)(object)winForm;
            m_Form.Shown += OnFormShown;
            m_Form.FormClosed += OnFormClosed;
            m_Owner = new Win32Window(parent);

            m_IsDisposed = false;
        }

        private void OnFormShown(object sender, EventArgs e)
        {
            ControlCreated?.Invoke(Control);
            Activated?.Invoke(this);
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            Closed?.Invoke(this);
        }

        public override void Dispose()
        {
            Close();
        }

        public override void Close()
        {
            if (m_IsDisposed)
            {
                m_IsDisposed = true;
                m_Form.Close();
            }
        }

        public override bool? ShowDialog()
        {
            var res = m_Form.ShowDialog(m_Owner);

            switch (res) 
            {
                case DialogResult.OK:
                    return true;

                case DialogResult.Cancel:
                    return false;

                default:
                    return null;
            }
        }

        public override void Show()
        {
            m_Form.Show(m_Owner);
            m_Form.BringToFront();
        }
    }
}
