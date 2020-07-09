using System;
using System.Runtime.InteropServices;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.UI.PropertyPage;
using Xarial.XCad.UI.Commands;

namespace Xarial.XCad.Documentation
{
    [ComVisible(true), Guid("D803CCDA-2254-41A8-8C74-F8240E95BEF9")]
    public class ControlsAddIn : SwAddInEx
    {
        [ComVisible(true)]
        public class MyPMPageHandler : SwPropertyManagerPageHandler 
        {
        }

        private enum Pages_e
        {
            DataModelCommonOpts,
            ComboBoxDataModel,
            GroupDataModel,
            NumberBoxDataModel,
            DataModelPageOpts,
            DataModelPageAtts,
            DataModelHelpLinks,
            TextBoxDataModel,
            OptionBoxDataModel,
            SelectionBoxDataModel,
            SelectionBoxListDataModel,
            SelectionBoxCustomSelectionFilterDataModel,
            ButtonDataModel,
            TabDataModel,
            BitmapDataModel,
            DynamicValuesDataModel
        }

        private SwPropertyManagerPage<DataModelCommonOpts> m_DataModelCommonOpts;
        private SwPropertyManagerPage<ComboBoxDataModel> m_ComboBoxDataModel;
        private SwPropertyManagerPage<GroupDataModel> m_GroupDataModel;
        private SwPropertyManagerPage<NumberBoxDataModel> m_NumberBoxDataModel;
        private SwPropertyManagerPage<DataModelPageOpts> m_DataModelPageOpts;
        private SwPropertyManagerPage<DataModelPageAtts> m_DataModelPageAtts;
        private SwPropertyManagerPage<DataModelHelpLinks> m_DataModelHelpLinks;
        private SwPropertyManagerPage<TextBoxDataModel> m_TextBoxDataModel;
        private SwPropertyManagerPage<OptionBoxDataModel> m_OptionBoxDataModel;
        private SwPropertyManagerPage<SelectionBoxDataModel> m_SelectionBoxDataModel;
        private SwPropertyManagerPage<SelectionBoxListDataModel> m_SelectionBoxListDataModel;
        private SwPropertyManagerPage<SelectionBoxCustomSelectionFilterDataModel> m_SelectionBoxCustomSelectionFilterDataModel;
        private SwPropertyManagerPage<ButtonDataModel> m_ButtonDataModel;
        private SwPropertyManagerPage<TabDataModel> m_TabDataModel;
        private SwPropertyManagerPage<BitmapDataModel> m_BitmapDataModel;
        private SwPropertyManagerPage<DynamicValuesDataModel> m_DynamicValuesDataModel;

        public override void OnConnect()
        {
            m_DataModelCommonOpts = CreatePage<DataModelCommonOpts, MyPMPageHandler>();
            m_ComboBoxDataModel = CreatePage<ComboBoxDataModel, MyPMPageHandler>();
            m_GroupDataModel = CreatePage<GroupDataModel, MyPMPageHandler>();
            m_NumberBoxDataModel = CreatePage<NumberBoxDataModel, MyPMPageHandler>();
            m_DataModelPageOpts = CreatePage<DataModelPageOpts, MyPMPageHandler>();
            m_DataModelPageAtts = CreatePage<DataModelPageAtts, MyPMPageHandler>();
            m_DataModelHelpLinks = CreatePage<DataModelHelpLinks, MyPMPageHandler>();
            m_TextBoxDataModel = CreatePage<TextBoxDataModel, MyPMPageHandler>();
            m_OptionBoxDataModel = CreatePage<OptionBoxDataModel, MyPMPageHandler>();
            m_SelectionBoxDataModel = CreatePage<SelectionBoxDataModel, MyPMPageHandler>();
            m_SelectionBoxListDataModel = CreatePage<SelectionBoxListDataModel, MyPMPageHandler>();
            m_SelectionBoxCustomSelectionFilterDataModel = CreatePage<SelectionBoxCustomSelectionFilterDataModel, MyPMPageHandler>();
            m_ButtonDataModel = CreatePage<ButtonDataModel, MyPMPageHandler>();
            m_TabDataModel = CreatePage<TabDataModel, MyPMPageHandler>();
            m_BitmapDataModel = CreatePage<BitmapDataModel, MyPMPageHandler>();
            m_DynamicValuesDataModel = CreatePage<DynamicValuesDataModel, MyPMPageHandler>();

            this.CommandManager.AddCommandGroup<Pages_e>().CommandClick += OnButtonClick;
        }

        private void OnButtonClick(Pages_e cmd)
        {
            switch (cmd)
            {
                case Pages_e.DataModelCommonOpts:
                    m_DataModelCommonOpts.Show(new DataModelCommonOpts());
                    break;
                case Pages_e.ComboBoxDataModel:
                    m_ComboBoxDataModel.Show(new ComboBoxDataModel());
                    break;
                case Pages_e.GroupDataModel:
                    m_GroupDataModel.Show(new GroupDataModel());
                    break;
                case Pages_e.NumberBoxDataModel:
                    m_NumberBoxDataModel.Show(new NumberBoxDataModel());
                    break;
                case Pages_e.DataModelPageOpts:
                    m_DataModelPageOpts.Show(new DataModelPageOpts());
                    break;
                case Pages_e.DataModelPageAtts:
                    m_DataModelPageAtts.Show(new DataModelPageAtts());
                    break;
                case Pages_e.DataModelHelpLinks:
                    m_DataModelHelpLinks.Show(new DataModelHelpLinks());
                    break;
                case Pages_e.TextBoxDataModel:
                    m_TextBoxDataModel.Show(new TextBoxDataModel());
                    break;
                case Pages_e.OptionBoxDataModel:
                    m_OptionBoxDataModel.Show(new OptionBoxDataModel());
                    break;
                case Pages_e.SelectionBoxDataModel:
                    m_SelectionBoxDataModel.Show(new SelectionBoxDataModel());
                    break;
                case Pages_e.SelectionBoxListDataModel:
                    m_SelectionBoxListDataModel.Show(new SelectionBoxListDataModel());
                    break;
                case Pages_e.SelectionBoxCustomSelectionFilterDataModel:
                    m_SelectionBoxCustomSelectionFilterDataModel.Show(new SelectionBoxCustomSelectionFilterDataModel());
                    break;
                case Pages_e.ButtonDataModel:
                    m_ButtonDataModel.Show(new ButtonDataModel());
                    break;
                case Pages_e.TabDataModel:
                    m_TabDataModel.Show(new TabDataModel());
                    break;
                case Pages_e.BitmapDataModel:
                    m_BitmapDataModel.Show(new BitmapDataModel());
                    break;
                case Pages_e.DynamicValuesDataModel:
                    m_DynamicValuesDataModel.Show(new DynamicValuesDataModel());
                    break;
            }
        }
    }
}
