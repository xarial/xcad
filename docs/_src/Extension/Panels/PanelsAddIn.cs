﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.UI.Commands;
using Xarial.XCad.Extensions;
using Xarial.XCad.UI;
using Xarial.XCad.UI.TaskPane.Attributes;
using Xarial.XCad.UI.TaskPane.Enums;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Documentation.Properties;

namespace Xarial.XCad.Documentation.Extension.Panels
{
    [ComVisible(true), Guid("36934A6F-5928-4D92-AC4D-5202A1F491D6")]
    public class PanelsAddIn : SwAddInEx
    {
        public enum PanelCommands_e
        {
            ShowPanels
        }

        #region TaskPaneCommands
        public enum TaskPaneCommands_e
        {
            [Title("Task Pane Command")]
            [Icon(typeof(Resources), nameof(Resources.command1))]
            Command,

            [TaskPaneStandardIcon(TaskPaneStandardIcons_e.Close)]
            Close
        }

        private IXTaskPane<WpfControl> m_TaskPane;
        #endregion TaskPaneCommands

        public override void OnConnect()
        {
            this.CommandManager.AddCommandGroup<PanelCommands_e>().CommandClick += OnCommandClick;

            #region TaskPaneSimple
            var taskPane = CreateTaskPane<WinFormControl>();
            WinFormControl control = taskPane.Control;
            #endregion TaskPaneSimple

            #region TaskPaneCommands2
            var cmdTaskPane = this.CreateTaskPane<WpfControl, TaskPaneCommands_e>();
            cmdTaskPane.ButtonClick += OnTaskPaneButtonClick;
            m_TaskPane = cmdTaskPane;
            #endregion TaskPaneCommands2
        }

        private void OnCommandClick(PanelCommands_e spec)
        {
            #region ModelViewTab
            var winFormModelViewTab = CreateDocumentTab<WinFormControl>(Application.Documents.Active);
            WinFormControl winFormModelViewCtrl = winFormModelViewTab.Control;

            var wpfModelViewTab = CreateDocumentTab<WpfControl>(Application.Documents.Active);
            WpfControl wpfModelViewTabCtrl = wpfModelViewTab.Control;
            #endregion ModelViewTab

            #region FeatMgrTab
            var winFormFeatMgrTab = CreateFeatureManagerTab<WinFormControl>(Application.Documents.Active);
            WinFormControl winFeatMgrFormCtrl = winFormFeatMgrTab.Control;

            var wpfFeatMgrTab = CreateFeatureManagerTab<WpfControl>(Application.Documents.Active);
            WpfControl wpfFeatMgrTabCtrl = wpfFeatMgrTab.Control;
            #endregion FeatMgrTab

            #region Popup
            var winFormPopupWnd = this.CreatePopupWindow<WinForm>();
            winFormPopupWnd.ShowDialog();

            var wpfPopupWnd = this.CreatePopupWindow<WpfWindow>();
            wpfPopupWnd.ShowDialog();
            #endregion Popup
        }

        #region TaskPaneCommands3
        private void OnTaskPaneButtonClick(TaskPaneCommands_e spec)
        {
            switch (spec)
            {
                case TaskPaneCommands_e.Command:
                    //handle command
                    break;

                case TaskPaneCommands_e.Close:
                    m_TaskPane.Close();
                    break;
            }
        }
        #endregion TaskPaneCommands3
    }
}
