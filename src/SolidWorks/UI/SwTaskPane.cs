//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.Commands.Toolkit.Structures;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI;
using Xarial.XCad.UI.TaskPane;
using Xarial.XCad.UI.TaskPane.Delegates;
using Xarial.XCad.Toolkit;
using Xarial.XCad.SolidWorks.UI.Toolkit;
using Xarial.XCad.Base;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Xarial.XCad.SolidWorks.UI
{
    public interface ISwTaskPane<TControl> : IXTaskPane<TControl>, IDisposable 
    {
        ITaskpaneView TaskPaneView { get; }
    }

    internal static class WinAPI
    {
        private const int WmPaint = 0x000F;

        [DllImport("User32.dll")]
        public static extern long SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        public static void ForcePaint(IntPtr hWnd)
        {
            SendMessage(hWnd, WmPaint, IntPtr.Zero, IntPtr.Zero);
        }
    }

    internal class SwTaskPane<TControl> : ISwTaskPane<TControl>, IAutoDisposable
    {
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event Action<IAutoDisposable> Disposed;

        public event TaskPaneButtonClickDelegate ButtonClick;
        public event ControlCreatedDelegate<TControl> ControlCreated;
        public event PanelActivatedDelegate<TControl> Activated;

        private readonly TaskPaneSpec m_Spec;

        private bool m_IsDisposed;

        public bool IsActive 
        {
            get => throw new NotImplementedException();
            set 
            {
                if (value)
                {
                    TaskPaneView.ShowView();
                }
                else 
                {
                    TaskPaneView.HideView();
                }
            }
        }

        public ITaskpaneView TaskPaneView { get; }
        public TControl Control { get; }

        public bool IsControlCreated => true;

        private WpfControlKeystrokePropagator m_KeystrokePropagator;

        private readonly TaskPaneTabCreator<TControl> m_Creator;
        private readonly IXLogger m_Logger;

        private readonly System.Windows.Forms.Control m_WinCtrl;

        internal SwTaskPane(TaskPaneTabCreator<TControl> creator, IXLogger logger)
        {
            m_Creator = creator;

            TControl ctrl;
            TaskPaneView = m_Creator.CreateControl(typeof(TControl), out ctrl, out m_WinCtrl);
            Control = ctrl;
            
            m_Logger = logger;

            if (ctrl is FrameworkElement)
            {
                m_KeystrokePropagator = new WpfControlKeystrokePropagator(ctrl as FrameworkElement);
            }

            m_Spec = m_Creator.Spec;

            (TaskPaneView as TaskpaneView).TaskPaneDestroyNotify += OnTaskPaneDestroyNotify;

            (TaskPaneView as TaskpaneView).TaskPaneActivateNotify += OnTaskPaneViewActivate;

            if (m_Spec.Buttons?.Any() == true)
            {
                (TaskPaneView as TaskpaneView).TaskPaneToolbarButtonClicked += OnTaskPaneToolbarButtonClicked;
            }
            
            m_IsDisposed = false;
            ControlCreated?.Invoke(Control);
        }

        private int OnTaskPaneViewActivate()
        {
            var hWnd = m_WinCtrl.Handle;

            WinAPI.ForcePaint(hWnd);

            Activated?.Invoke(this);

            return 0;
        }

        private int OnTaskPaneToolbarButtonClicked(int buttonIndex)
        {
            m_Logger.Log($"Task pane button clicked: {buttonIndex}", XCad.Base.Enums.LoggerMessageSeverity_e.Debug);

            if (m_Spec.Buttons?.Length > buttonIndex)
            {
                ButtonClick?.Invoke(m_Spec.Buttons[buttonIndex]);
            }
            else
            {
                m_Logger.Log($"Invalid task pane button id is clicked: {buttonIndex}", XCad.Base.Enums.LoggerMessageSeverity_e.Error);
                Debug.Assert(false, "Invalid command id");
            }

            return HResult.S_OK;
        }

        private int OnTaskPaneDestroyNotify()
        {
            m_Logger.Log("Destroying task pane", XCad.Base.Enums.LoggerMessageSeverity_e.Debug);

            Dispose();
            return HResult.S_OK;
        }

        public void Dispose() => Close();

        public void Close()
        {
            if (!m_IsDisposed)
            {
                m_IsDisposed = true;

                m_KeystrokePropagator?.Dispose();

                (TaskPaneView as TaskpaneView).TaskPaneActivateNotify -= OnTaskPaneViewActivate;
                (TaskPaneView as TaskpaneView).TaskPaneDestroyNotify -= OnTaskPaneDestroyNotify;
                (TaskPaneView as TaskpaneView).TaskPaneToolbarButtonClicked -= OnTaskPaneToolbarButtonClicked;

                try
                {
                    if (Control is IDisposable)
                    {
                        (Control as IDisposable).Dispose();
                    }
                }
                finally 
                {
                    if (!TaskPaneView.DeleteView())
                    {
                        throw new InvalidOperationException("Failed to remove TaskPane");
                    }
                }

                Disposed?.Invoke(this);
            }
        }
    }
}
