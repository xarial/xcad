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
            TextBox,
            OptionBox,
            SelectionBox,
            SelectionBoxList,
            SelectionBoxCustomSelectionFilter,
            Button,
            CheckBox,
            Tab,
            Bitmap,
            BitmapButton,
            DynamicValues,
            CustomWpfControl,
            CustomWinFormsControl
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
        private SwPropertyManagerPage<CheckBoxDataModel> m_CheckBoxDataModel;
        private SwPropertyManagerPage<TabDataModel> m_TabDataModel;
        private SwPropertyManagerPage<BitmapDataModel> m_BitmapDataModel;
        private SwPropertyManagerPage<BitmapButtonDataModel> m_BitmapButtonDataModel;
        private SwPropertyManagerPage<DynamicValuesDataModel> m_DynamicValuesDataModel;
        private SwPropertyManagerPage<CustomWpfControlPage> m_CustomWpfControlDataModel;
        private SwPropertyManagerPage<CustomWinFormsControlPage> m_CustomWinFormsControlDataModel;

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
            m_CheckBoxDataModel = CreatePage<CheckBoxDataModel, MyPMPageHandler>();
            m_TabDataModel = CreatePage<TabDataModel, MyPMPageHandler>();
            m_BitmapDataModel = CreatePage<BitmapDataModel, MyPMPageHandler>();
            m_BitmapButtonDataModel = CreatePage<BitmapButtonDataModel, MyPMPageHandler>();
            m_DynamicValuesDataModel = CreatePage<DynamicValuesDataModel, MyPMPageHandler>();
            m_CustomWpfControlDataModel = CreatePage<CustomWpfControlPage, MyPMPageHandler>();
            m_CustomWinFormsControlDataModel = CreatePage<CustomWinFormsControlPage, MyPMPageHandler>();

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
                case Pages_e.TextBox:
                    m_TextBoxDataModel.Show(new TextBoxDataModel());
                    break;
                case Pages_e.OptionBox:
                    m_OptionBoxDataModel.Show(new OptionBoxDataModel());
                    break;
                case Pages_e.SelectionBox:
                    m_SelectionBoxDataModel.Show(new SelectionBoxDataModel());
                    break;
                case Pages_e.SelectionBoxList:
                    m_SelectionBoxListDataModel.Show(new SelectionBoxListDataModel());
                    break;
                case Pages_e.SelectionBoxCustomSelectionFilter:
                    m_SelectionBoxCustomSelectionFilterDataModel.Show(new SelectionBoxCustomSelectionFilterDataModel());
                    break;
                case Pages_e.Button:
                    m_ButtonDataModel.Show(new ButtonDataModel());
                    break;
                case Pages_e.CheckBox:
                    m_CheckBoxDataModel.Show(new CheckBoxDataModel());
                    break;
                case Pages_e.Tab:
                    m_TabDataModel.Show(new TabDataModel());
                    break;
                case Pages_e.Bitmap:
                    m_BitmapDataModel.Show(new BitmapDataModel());
                    break;
                case Pages_e.BitmapButton:
                    m_BitmapButtonDataModel.Show(new BitmapButtonDataModel());
                    break;
                case Pages_e.DynamicValues:
                    m_DynamicValuesDataModel.Show(new DynamicValuesDataModel());
                    break;
                case Pages_e.CustomWpfControl:
                    m_CustomWpfControlDataModel.Show(new CustomWpfControlPage());
                    break;
                case Pages_e.CustomWinFormsControl:
                    m_CustomWinFormsControlDataModel.Show(new CustomWinFormsControlPage());
                    break;
            }
        }
    }
}
