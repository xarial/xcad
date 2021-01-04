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

namespace Xarial.XCad.SolidWorks.UI
{
    public interface ISwTaskPane<TControl> : IXTaskPane<TControl>, IDisposable 
    {
        ITaskpaneView TaskPaneView { get; }
    }

    internal class SwTaskPane<TControl> : ISwTaskPane<TControl>
    {
        const int S_OK = 0;

        public event TaskPaneButtonClickDelegate ButtonClick;
        public event ControlCreatedDelegate<TControl> ControlCreated;

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

        private WpfControlKeystrokePropagator m_KeystrokePropagator;

        private readonly IServiceProvider m_SvcProvider;

        internal SwTaskPane(ISldWorks app, ITaskpaneView taskPaneView, TControl ctrl, TaskPaneSpec spec, IServiceProvider svcProvider)
        {
            Control = ctrl;

            m_SvcProvider = svcProvider;

            if (ctrl is FrameworkElement)
            {
                m_KeystrokePropagator = new WpfControlKeystrokePropagator(ctrl as FrameworkElement);
            }

            TaskPaneView = taskPaneView;
            m_Spec = spec;

            (TaskPaneView as TaskpaneView).TaskPaneDestroyNotify += OnTaskPaneDestroyNotify;

            LoadButtons(app);

            m_IsDisposed = false;
            ControlCreated?.Invoke(Control);
        }

        private void LoadButtons(ISldWorks app) 
        {
            if (m_Spec.Buttons?.Any() == true) 
            {
                using (var iconsConv = m_SvcProvider.GetService<IIconsCreator>())
                {
                    foreach (var btn in m_Spec.Buttons)
                    {
                        var tooltip = btn.Tooltip;

                        if (string.IsNullOrEmpty(tooltip)) 
                        {
                            tooltip = btn.Title;
                        }

                        if (btn.StandardIcon.HasValue)
                        {
                            if (!TaskPaneView.AddStandardButton((int)btn.StandardIcon, tooltip))
                            {
                                throw new InvalidOperationException($"Failed to add standard button for '{tooltip}'");
                            }
                        }
                        else
                        {
                            var icon = btn.Icon;

                            if (icon == null) 
                            {
                                icon = Defaults.Icon;
                            }

                            //NOTE: unlike task pane icon, command icons must have the same transparency key as command manager commands
                            if (app.SupportsHighResIcons(CompatibilityUtils.HighResIconsScope_e.TaskPane))
                            {
                                var imageList = iconsConv.ConvertIcon(new CommandGroupHighResIcon(icon));
                                if (!TaskPaneView.AddCustomButton2(imageList, tooltip))
                                {
                                    throw new InvalidOperationException($"Failed to create task pane button for '{tooltip}' with highres icon");
                                }
                            }
                            else
                            {
                                var imagePath = iconsConv.ConvertIcon(new CommandGroupIcon(icon)).First();
                                if (!TaskPaneView.AddCustomButton(imagePath, tooltip))
                                {
                                    throw new InvalidOperationException($"Failed to create task pane button for {tooltip}");
                                }
                            }
                        }
                    }
                }

                (TaskPaneView as TaskpaneView).TaskPaneToolbarButtonClicked += OnTaskPaneToolbarButtonClicked;
            }
        }

        private int OnTaskPaneToolbarButtonClicked(int buttonIndex)
        {
            //m_Logger.Log($"Task pane button clicked: {buttonIndex}");

            if (m_Spec.Buttons?.Length > buttonIndex)
            {
                ButtonClick?.Invoke(m_Spec.Buttons[buttonIndex]);
            }
            else
            {
                //m_Logger.Log($"Invalid task pane button id is clicked: {buttonIndex}");
                Debug.Assert(false, "Invalid command id");
            }

            return S_OK;
        }

        private int OnTaskPaneDestroyNotify()
        {
            //m_Logger.Log("Destroying task pane");

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

                (TaskPaneView as TaskpaneView).TaskPaneDestroyNotify -= OnTaskPaneDestroyNotify;
                (TaskPaneView as TaskpaneView).TaskPaneToolbarButtonClicked -= OnTaskPaneToolbarButtonClicked;

                if (Control is IDisposable)
                {
                    (Control as IDisposable).Dispose();
                }

                if (!TaskPaneView.DeleteView())
                {
                    throw new InvalidOperationException("Failed to remove TaskPane");
                }
            }
        }
    }
}
