#if _AddCommandManager_ || _AddPropertyPage_ || _AddCustomFeature_
using __TemplateNamePlaceholder__SwAddin.Properties;
#endif
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.UI.Commands;
using Xarial.XCad.UI.Commands.Attributes;
using Xarial.XCad.UI.Commands.Enums;

namespace __TemplateNamePlaceholder__.Sw.AddIn
{
    [ComVisible(true)]
    [Guid("E82CC06F-81FC-4CE6-B88F-5B65023808F9")]
    [Title("__TemplateNamePlaceholder__ SOLIDWORKS Add-In")]
    [Description("SOLIDWORKS add-in created with xCAD.NET")]
    public class __TemplateNamePlaceholder__SwAddIn : SwAddInEx
    {
#if _AddCommandManager_ || _AddPropertyPage_ || _AddCustomFeature_
        [Title("__TemplateNamePlaceholder__")]
        [Description("Commands of __TemplateNamePlaceholder__")]
        private enum Commands_e
        {
#if _AddCommandManager_ || _AddPropertyPage_
            [Icon(typeof(Resources), nameof(Resources.box_icon))]
            [Title("Create Box")]
            [Description("Creates box using standard feature")]
            [CommandItemInfo(WorkspaceTypes_e.Part | WorkspaceTypes_e.InContextPart)]
            CreateBox,

#endif
#if _AddCustomFeature_
            [Icon(typeof(Resources), nameof(Resources.box_icon))]
            [Title("Create Parametric Box")]
            [Description("Creates parametric macro feature")]
            [CommandItemInfo(WorkspaceTypes_e.Part | WorkspaceTypes_e.InContextPart)]
            CreateParametricBox,

#endif
            [Icon(typeof(Resources), nameof(Resources.about_icon))]
            [Title("About...")]
            [Description("Shows About Box")]
            About
        }

#endif
        public override void OnConnect()
        {
#if _AddCommandManager_ || _AddPropertyPage_ || _AddCustomFeature_
            this.CommandManager.AddCommandGroup<Commands_e>().CommandClick += OnCommandClick;
#else
            Application.ShowMessageBox("Hello, __TemplateNamePlaceholder__! xCAD.NET", MessageBoxIcon_e.Info);
#endif
        }
#if _AddCommandManager_ || _AddPropertyPage_ || _AddCustomFeature_

        private void OnCommandClick(Commands_e spec)
        {
            switch (spec)
            {
#if _AddCommandManager_ || _AddPropertyPage_
                case Commands_e.CreateBox:
#if _AddPropertyPage_
                    //TODO: Show page
#else
                    CreateBox(100, 200, 300);
#endif
                    break;

#endif
#if _AddCustomFeature_
                case Commands_e.CreateParametricBox:
                    break;

#endif
                case Commands_e.About:
                    Application.ShowMessageBox("About __TemplateNamePlaceholder__", MessageBoxIcon_e.Info);
                    break;
            }
        }
#endif
#if _AddCommandManager_ || _AddPropertyPage_ 

        private void CreateBox(double width, double height, double length) 
        {
        }
#endif
    }
}
