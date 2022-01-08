using System;
using System.Linq;
using System.Runtime.InteropServices;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.UI.PropertyPage;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.Commands;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Services;

namespace Xarial.XCad.Documentation
{
    //--- CascadingVisibility
    public enum Groups_e
    {
        GroupA,
        GroupB,
        GroupC
    }

    public enum GroupA_e
    {
        GroupA_OptionA,
        GroupA_OptionB,
        GroupA_OptionC
    }

    public enum GroupB_e
    {
        GroupB_OptionA,
        GroupB_OptionB,
    }

    public enum GroupC_e
    {
        GroupC_OptionA,
        GroupC_OptionB,
        GroupC_OptionC,
        GroupC_OptionD
    }

    public enum Tags_e
    {
        Groups
    }

    [ComVisible(true)]
    public class DataModelCascading : SwPropertyManagerPageHandler
    {
        [ControlTag(Tags_e.Groups)]
        public Groups_e Groups { get; set; }

        [DependentOn(typeof(GroupOptionsVisibilityDepHandler), Tags_e.Groups)]
        [ControlTag(Groups_e.GroupA)]
        [OptionBox]
        public GroupA_e GroupA { get; set; }

        [DependentOn(typeof(GroupOptionsVisibilityDepHandler), Tags_e.Groups)]
        [ControlTag(Groups_e.GroupB)]
        [OptionBox]
        public GroupB_e GroupB { get; set; }

        [DependentOn(typeof(GroupOptionsVisibilityDepHandler), Tags_e.Groups)]
        [ControlTag(Groups_e.GroupC)]
        [OptionBox]
        public GroupC_e GroupC { get; set; }
    }

    public class GroupOptionsVisibilityDepHandler : IDependencyHandler
    {
        public void UpdateState(IXApplication app, IControl source, IControl[] dependencies)
        {
            var curGrp = (Groups_e)(dependencies.First()).GetValue();

            source.Visible = (Groups_e)source.Tag == curGrp;
        }
    }
    //---

    //--- Enable
    [ComVisible(true)]
    public class DataModelEnable : SwPropertyManagerPageHandler
    {
        [ControlTag(nameof(Enable))]
        public bool Enable { get; set; }

        [DependentOn(typeof(EnableDepHandler), nameof(Enable))]
        public ISwFace Selection { get; set; }
    }

    public class EnableDepHandler : IDependencyHandler
    {
        public void UpdateState(IXApplication app, IControl source, IControl[] dependencies)
        {
            source.Enabled = (bool)dependencies.First().GetValue();
        }
    }
    //---
    
    [ComVisible(true), Guid("D745F780-C031-4A2B-A627-AC8C7C61F421")]
    public class TagsAndDependencies : SwAddInEx
    {
        [Title("PmpDependency")]
        public enum Commands_e
        {
            ShowPmpCascading,
            ShowPmpEnable
        }

        private ISwPropertyManagerPage<DataModelCascading> m_PmpPageCascading;
        private DataModelCascading m_DataModelCascading;

        private ISwPropertyManagerPage<DataModelEnable> m_PmpPageEnable;
        private DataModelEnable m_DataModelEnable;

        public override void OnConnect()
        {
            this.CommandManager.AddCommandGroup<Commands_e>().CommandClick += OnButtonClick;

            m_PmpPageCascading = CreatePage<DataModelCascading>();
            m_PmpPageCascading.Closed += OnClosed;
            m_DataModelCascading = new DataModelCascading();

            m_PmpPageEnable = CreatePage<DataModelEnable>();
            m_PmpPageEnable.Closed += OnClosed;
            m_DataModelEnable = new DataModelEnable();
        }

        private void OnClosed(PageCloseReasons_e reason)
        {
            var selGrp = m_DataModelCascading.Groups;

            var selOpt = default(Enum);

            switch (selGrp)
            {
                case Groups_e.GroupA:
                    selOpt = m_DataModelCascading.GroupA;
                    break;

                case Groups_e.GroupB:
                    selOpt = m_DataModelCascading.GroupB;
                    break;

                case Groups_e.GroupC:
                    selOpt = m_DataModelCascading.GroupC;
                    break;
            }

            Application.ShowMessageBox($"Selected group: {selGrp}, selected option {selOpt}");
        }

        private void OnButtonClick(Commands_e cmd)
        {
            switch (cmd)
            {
                case Commands_e.ShowPmpCascading:
                    m_PmpPageCascading.Show(m_DataModelCascading);
                    break;

                case Commands_e.ShowPmpEnable:
                    m_PmpPageEnable.Show(m_DataModelEnable);
                    break;
            }
        }
    }
}

