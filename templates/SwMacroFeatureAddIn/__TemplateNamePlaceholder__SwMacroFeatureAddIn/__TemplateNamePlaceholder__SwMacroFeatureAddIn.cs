using __TemplateNamePlaceholder__SwMacroFeatureAddIn.Properties;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.UI.Commands;
using Xarial.XCad.UI.Commands.Attributes;
using Xarial.XCad.UI.Commands.Enums;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.UI.PropertyPage.Structures;

namespace __TemplateNamePlaceholder__.Sw.AddIn
{
    [ComVisible(true)]
    [Guid("04304922-1760-469A-8A2D-9A7F1055EFBC")]
    [Title("__TemplateNamePlaceholder__ SOLIDWORKS Macro Feature Add-In")]
    [Description("SOLIDWORKS macro feature add-in created with xCAD.NET")]
    public class __TemplateNamePlaceholder__SwMacroFeatureAddIn : SwAddInEx
    {
        //command groups can be created by defining the enumeration and all its fields will be rendered as buttons
        [Title("__TemplateNamePlaceholder__")]
        [Description("Commands of __TemplateNamePlaceholder__")]
        private enum Commands_e
        {
            [Icon(typeof(Resources), nameof(Resources.cylinder_icon))]
            [Title("Create Cylinder")]
            [Description("Creates parametric cylinder")]
            [CommandItemInfo(true, true, WorkspaceTypes_e.Part | WorkspaceTypes_e.InContextPart, true)]
            CreateCylinder
        }

        //function is called when add-in is loading
        public override void OnConnect()
        {
            System.Diagnostics.Debugger.Launch();
            //creating command manager based on enum
            CommandManager.AddCommandGroup<Commands_e>().CommandClick += OnCommandClick;
        }

        //button click handler will pass the enum of the button being clicked
        private void OnCommandClick(Commands_e spec)
        {
            switch (spec)
            {
                case Commands_e.CreateCylinder:
                    Application.Documents.Active.Features.CreateCustomFeature<CylinderMacroFeatureDefinition, CylinderMacroFeatureData, CylinderPropertyPage>();
                    break;
            }
        }
    }
}
