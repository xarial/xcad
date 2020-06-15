//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI;
using Xarial.XCad.UI.PopupWindow.Delegates;

namespace Xarial.XCad.SolidWorks.UI
{
    public abstract class SwPopupWindow<TWindow> : IXPopupWindow<TWindow>, IDisposable
    {
        public abstract bool IsActive { get; set; }
        public abstract TWindow Control { get; }

        public abstract event PopupWindowClosedDelegate<TWindow> Closed;

        public abstract void Close();

        public abstract void Dispose();

        public abstract bool? ShowDialog();
        public abstract void Show();
    }

#if NET461
    public class SwPopupWpfWindow<TWindow> : SwPopupWindow<TWindow>
    {
        public override event PopupWindowClosedDelegate<TWindow> Closed;

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

        private readonly System.Windows.Window m_WpfWindow;

        private readonly System.Windows.Interop.WindowInteropHelper m_Owner;

        private bool m_IsDisposed;

        internal SwPopupWpfWindow(TWindow wpfWindow, IntPtr parent) 
        {
            Control = wpfWindow;
            m_WpfWindow = (System.Windows.Window)(object)wpfWindow;
            m_WpfWindow.Closed += OnWpfWindowClosed;
            m_Owner = new System.Windows.Interop.WindowInteropHelper(m_WpfWindow);
            m_Owner.Owner = parent;

            m_IsDisposed = false;
        }

        private void OnWpfWindowClosed(object sender, EventArgs e)
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
                m_WpfWindow.Close();
            }
        }

        public override bool? ShowDialog()
        {
            return m_WpfWindow.ShowDialog();
        }

        public override void Show()
        {
            m_WpfWindow.Show();
            m_WpfWindow.BringIntoView();
        }
    }

    public class SwPopupWinForm<TControl> : SwPopupWindow<TControl>
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

        private readonly System.Windows.Forms.Form m_Form;

        private readonly System.Windows.Forms.IWin32Window m_Owner;

        public override event PopupWindowClosedDelegate<TControl> Closed;

        private bool m_IsDisposed;

        internal SwPopupWinForm(TControl winForm, IntPtr parent)
        {
            Control = winForm;
            m_Form = (System.Windows.Forms.Form)(object)winForm;
            m_Form.FormClosed += OnFormClosed;
            m_Owner = new Toolkit.Windows.Win32Window(parent);

            m_IsDisposed = false;
        }

        private void OnFormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
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
                case System.Windows.Forms.DialogResult.OK:
                    return true;

                case System.Windows.Forms.DialogResult.Cancel:
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
#endif
}
