using System.Runtime.InteropServices;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Documentation.Properties;
using Xarial.XCad.Features.CustomFeature.Structures;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features.CustomFeature;
using Xarial.XCad.SolidWorks.UI.PropertyPage;
using Xarial.XCad.UI.Commands;
using Xarial.XCad.Features;

namespace Xarial.XCad.Documentation
{
    #region PMPage
    [Title(typeof(Resources), nameof(Resources.LocalizedPmPageTitle))]
    public class LocalizedPmPage
    {
        [Title(typeof(Resources), nameof(Resources.TextFieldTitle))]
        [Summary(typeof(Resources), nameof(Resources.TextFieldDescription))]
        public string TextField { get; set; }

        [Title(typeof(Resources), nameof(Resources.NumericFieldTitle))]
        [Summary(typeof(Resources), nameof(Resources.NumericFieldDescription))]
        public double NumericField { get; set; }
    }
    #endregion PMPage

    #region MacroFeature
    [Title(typeof(Resources), nameof(Resources.MacroFeatureBaseName))]
    [ComVisible(true)]
    public class LocalizedMacroFeature : SwMacroFeatureDefinition
    {
        public override CustomFeatureRebuildResult OnRebuild(ISwApplication app, ISwDocument model, ISwMacroFeature feature)
        {
            if (!string.IsNullOrEmpty(model.Title))
            {
                return new CustomFeatureRebuildResult()
                {
                    Result = true
                };
            }
            else
            {
                return new CustomFeatureRebuildResult()
                {
                    Result = true,
                    ErrorMessage = Resources.MacroFeatureError
                };
            }
        }
    }
    #endregion MacroFeature

    [ComVisible(true), Guid("CD96ACAE-57E6-400F-927A-27D912407663")]
    public class LocalizationAddIn : SwAddInEx
    {
        #region Commands
        [Title(typeof(Resources), nameof(Resources.ToolbarTitle))]
        [Summary(typeof(Resources), nameof(Resources.ToolbarHint))]
        public enum Commands_e
        {
            [Title(typeof(Resources), nameof(Resources.ShowPmpCommandTitle))]
            [Summary(typeof(Resources), nameof(Resources.ShowPmpCommandHint))]
            ShowPmp,

            [Title(typeof(Resources), nameof(Resources.CreateMacroFeatureCommandTitle))]
            [Summary(typeof(Resources), nameof(Resources.CreateMacroFeatureCommandHint))]
            CreateMacroFeature
        }
        #endregion Commands

        [ComVisible(true)]
        public class PMPageHandler : SwPropertyManagerPageHandler
        {
        }

        private ISwPropertyManagerPage<LocalizedPmPage> m_Page;

        public override void OnConnect()
        {
            this.CommandManager.AddCommandGroup`< Commands_e >`().CommandClick += OnButtonClick;

            m_Page = CreatePage<LocalizedPmPage, PMPageHandler>();
        }

        private void OnButtonClick(Commands_e cmd)
        {
            switch (cmd)
            {
                case Commands_e.ShowPmp:
                    m_Page.Show(new LocalizedPmPage());
                    break;

                case Commands_e.CreateMacroFeature:
                    Application.Documents.Active.Features.CreateCustomFeature<LocalizedMacroFeature>();
                    break;
            }
        }
    }
}
