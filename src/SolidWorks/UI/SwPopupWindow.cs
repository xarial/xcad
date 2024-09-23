//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using Xarial.XCad.Toolkit.Utils;
using Xarial.XCad.UI;
using Xarial.XCad.UI.Enums;
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

        public abstract bool? ShowDialog(PopupDock_e dock = PopupDock_e.Center);
        public abstract void Show(PopupDock_e dock = PopupDock_e.Center);
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

        public override bool? ShowDialog(PopupDock_e dock = PopupDock_e.Center)
        {
            var startupLoc = m_WpfWindow.WindowStartupLocation;

            PositionWindow(dock);

            var res = m_WpfWindow.ShowDialog();

            m_WpfWindow.WindowStartupLocation = startupLoc;

            return res;
        }

        public override void Show(PopupDock_e dock = PopupDock_e.Center)
        {
            var startupLoc = m_WpfWindow.WindowStartupLocation;

            PositionWindow(dock);

            m_WpfWindow.Show();
            m_WpfWindow.BringIntoView();

            m_WpfWindow.WindowStartupLocation = startupLoc;
        }

        private void PositionWindow(PopupDock_e dock) 
        {
            var pos = PopupHelper.CalculateLocation(m_ParentWnd, dock, true, m_WpfWindow.Width, m_WpfWindow.Height,
                new XCad.Geometry.Structures.Thickness(m_WpfWindow.Padding.Left, m_WpfWindow.Padding.Right, m_WpfWindow.Padding.Top, m_WpfWindow.Padding.Bottom));
            m_WpfWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            m_WpfWindow.Left = pos.X;
            m_WpfWindow.Top = pos.Y;
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
            if (!m_IsDisposed)
            {
                m_IsDisposed = true;
                m_Form.Close();
            }
        }

        public override bool? ShowDialog(PopupDock_e dock = PopupDock_e.Center)
        {
            var startupLoc = m_Form.StartPosition;
            PositionWindow(dock);
            var res = m_Form.ShowDialog(m_Owner);

            m_Form.StartPosition = startupLoc;

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

        public override void Show(PopupDock_e dock = PopupDock_e.Center)
        {
            var startupLoc = m_Form.StartPosition;
            PositionWindow(dock);
            m_Form.Show(m_Owner);
            m_Form.BringToFront();
            m_Form.StartPosition = startupLoc;
        }

        private void PositionWindow(PopupDock_e dock)
        {
            var pos = PopupHelper.CalculateLocation(m_Owner.Handle, dock, false, m_Form.Width, m_Form.Height,
                new XCad.Geometry.Structures.Thickness(m_Form.Padding.Left, m_Form.Padding.Right, m_Form.Padding.Top, m_Form.Padding.Bottom));
            m_Form.StartPosition = FormStartPosition.Manual;
            m_Form.DesktopLocation = new System.Drawing.Point((int)pos.X, (int)pos.Y);
        }
    }
}
