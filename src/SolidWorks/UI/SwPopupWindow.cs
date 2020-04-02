//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI;

namespace Xarial.XCad.SolidWorks.UI
{
    public abstract class SwPopupWindow<TControl> : IXCustomPanel<TControl>, IDisposable
    {
        public bool Modal { get; set; }
        public abstract bool IsActive { get; set; }
        public abstract TControl Control { get; }
        public abstract void Dispose();
    }

#if NET461
    public class SwPopupWpfWindow<TControl> : SwPopupWindow<TControl>
    {
        public override bool IsActive
        {
            get => m_WpfWindow.IsVisible;
            set
            {
                if (value)
                {
                    if (Modal)
                    {
                        m_WpfWindow.ShowDialog();
                    }
                    else 
                    {
                        m_WpfWindow.Show();
                    }

                    m_WpfWindow.BringIntoView();
                }
                else
                {
                    m_WpfWindow.Hide();
                } 
            }
        }

        public override TControl Control { get; }

        private readonly System.Windows.Window m_WpfWindow;

        private readonly System.Windows.Interop.WindowInteropHelper m_Owner;

        internal SwPopupWpfWindow(TControl wpfWindow, IntPtr parent) 
        {
            Control = wpfWindow;
            m_WpfWindow = (System.Windows.Window)(object)wpfWindow;
            m_Owner = new System.Windows.Interop.WindowInteropHelper(m_WpfWindow);
            m_Owner.Owner = parent;
        }

        public override void Dispose()
        {
            m_WpfWindow.Close();
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
                    if (Modal)
                    {
                        m_Form.ShowDialog(m_Owner);
                    }
                    else
                    {
                        m_Form.Show(m_Owner);
                    }

                    m_Form.BringToFront();
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

        internal SwPopupWinForm(TControl winForm, IntPtr parent)
        {
            Control = winForm;
            m_Form = (System.Windows.Forms.Form)(object)winForm;
            m_Owner = new Toolkit.Windows.Win32Window(parent);
        }

        public override void Dispose()
        {
            m_Form.Close();
        }
    }
#endif
}
