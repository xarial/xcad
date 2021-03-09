//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.Commands.Exceptions;
using Xarial.XCad.SolidWorks.UI.Commands.Toolkit.Structures;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI;
using Xarial.XCad.UI.TaskPane;
using Xarial.XCad.Toolkit;

namespace Xarial.XCad.SolidWorks.UI.Toolkit
{
    internal class TaskPaneTabCreator<TControl> : CustomControlCreator<ITaskpaneView, TControl>
    {
        private readonly ISwApplication m_App;
        private readonly IServiceProvider m_SvcProvider;
        internal TaskPaneSpec Spec { get; }

        internal TaskPaneTabCreator(ISwApplication app, IServiceProvider svcProvider, TaskPaneSpec spec) 
        {
            m_App = app;
            m_SvcProvider = svcProvider;
            Spec = spec ?? new TaskPaneSpec();
        }

        protected override ITaskpaneView HostComControl(string progId, string title, 
            IXImage image, out TControl specCtrl)
        {
            using (var iconsConv = m_SvcProvider.GetService<IIconsCreator>())
            {
                var taskPaneView = CreateTaskPaneView(iconsConv, image, title);
                specCtrl = (TControl)taskPaneView.AddControl(progId, "");

                if (specCtrl == null)
                {
                    throw new ComControlHostException(progId);
                }

                return taskPaneView;
            }
        }

        protected override ITaskpaneView HostNetControl(Control winCtrlHost, TControl ctrl, string title, IXImage image)
        {
            using (var iconsConv = m_SvcProvider.GetService<IIconsCreator>())
            {
                var taskPaneView = CreateTaskPaneView(iconsConv, image, title);

                if (!taskPaneView.DisplayWindowFromHandle(winCtrlHost.Handle.ToInt32()))
                {
                    throw new NetControlHostException(winCtrlHost.Handle);
                }

                return taskPaneView;
            }
        }

        private ITaskpaneView CreateTaskPaneView(IIconsCreator iconConv, IXImage icon,
            string title)
        {
            if (icon == null)
            {
                if (Spec.Icon != null)
                {
                    icon = Spec.Icon;
                }
            }

            if (string.IsNullOrEmpty(title))
            {
                title = Spec.Title;
            }

            ITaskpaneView taskPaneView;

            if (m_App.Sw.SupportsHighResIcons(CompatibilityUtils.HighResIconsScope_e.TaskPane))
            {
                string[] taskPaneIconImages = null;

                if (icon != null)
                {
                    taskPaneIconImages = iconConv.ConvertIcon(new TaskPaneHighResIcon(icon));
                }

                taskPaneView = m_App.Sw.CreateTaskpaneView3(taskPaneIconImages, title);
            }
            else
            {
                var taskPaneIconImage = "";

                if (icon != null)
                {
                    taskPaneIconImage = iconConv.ConvertIcon(new TaskPaneIcon(icon)).First();
                }

                taskPaneView = m_App.Sw.CreateTaskpaneView2(taskPaneIconImage, title);
            }

            LoadButtons(taskPaneView, m_App.Sw);

            return taskPaneView;
        }

        private void LoadButtons(ITaskpaneView taskPaneView, ISldWorks app)
        {
            if (Spec.Buttons?.Any() == true)
            {
                using (var iconsConv = m_SvcProvider.GetService<IIconsCreator>())
                {
                    foreach (var btn in Spec.Buttons)
                    {
                        var tooltip = btn.Tooltip;

                        if (string.IsNullOrEmpty(tooltip))
                        {
                            tooltip = btn.Title;
                        }

                        if (btn.StandardIcon.HasValue)
                        {
                            if (!taskPaneView.AddStandardButton((int)btn.StandardIcon, tooltip))
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
                                if (!taskPaneView.AddCustomButton2(imageList, tooltip))
                                {
                                    throw new InvalidOperationException($"Failed to create task pane button for '{tooltip}' with highres icon");
                                }
                            }
                            else
                            {
                                var imagePath = iconsConv.ConvertIcon(new CommandGroupIcon(icon)).First();
                                if (!taskPaneView.AddCustomButton(imagePath, tooltip))
                                {
                                    throw new InvalidOperationException($"Failed to create task pane button for {tooltip}");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
