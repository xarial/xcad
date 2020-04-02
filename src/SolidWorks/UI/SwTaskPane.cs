using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Xarial.XCad.SolidWorks.UI.Commands.Toolkit.Structures;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI;
using Xarial.XCad.UI.TaskPane;
using Xarial.XCad.UI.TaskPane.Delegates;

namespace Xarial.XCad.SolidWorks.UI
{
    public class SwTaskPane<TControl> : IXTaskPane<TControl>, IDisposable
    {
        const int S_OK = 0;

        public event TaskPaneButtonClickDelegate ButtonClick;

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

        internal SwTaskPane(ISldWorks app, ITaskpaneView taskPaneView, TControl ctrl, TaskPaneSpec spec)
        {
            Control = ctrl;
            TaskPaneView = taskPaneView;
            m_Spec = spec;

            (TaskPaneView as TaskpaneView).TaskPaneDestroyNotify += OnTaskPaneDestroyNotify;

            LoadButtons(app);

            m_IsDisposed = false;
        }

        private void LoadButtons(ISldWorks app) 
        {
            if (m_Spec.Buttons?.Any() == true) 
            {
                using (var iconsConv = new IconsConverter())
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
            if (!m_IsDisposed)
            {
                m_IsDisposed = true;

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
