//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
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

namespace Xarial.XCad.SolidWorks.UI
{
    public interface ISwTaskPane<TControl> : IXTaskPane<TControl>, IDisposable 
    {
        ITaskpaneView TaskPaneView { get; }
    }

    internal class SwTaskPane<TControl> : ISwTaskPane<TControl>
    {
        private const int S_OK = 0;

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

        internal SwTaskPane(TaskPaneTabCreator<TControl> creator, IXLogger logger)
        {
            m_Creator = creator;

            TControl ctrl;
            TaskPaneView = m_Creator.CreateControl(typeof(TControl), out ctrl);
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
            Activated?.Invoke(this);
            return 0;
        }

        private int OnTaskPaneToolbarButtonClicked(int buttonIndex)
        {
            m_Logger.Log($"Task pane button clicked: {buttonIndex}");

            if (m_Spec.Buttons?.Length > buttonIndex)
            {
                ButtonClick?.Invoke(m_Spec.Buttons[buttonIndex]);
            }
            else
            {
                m_Logger.Log($"Invalid task pane button id is clicked: {buttonIndex}");
                Debug.Assert(false, "Invalid command id");
            }

            return S_OK;
        }

        private int OnTaskPaneDestroyNotify()
        {
            m_Logger.Log("Destroying task pane");

            Dispose();
            return S_OK;
        }

        public void Dispose()
        {
            Close();
        }

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
            }
        }
    }
}
