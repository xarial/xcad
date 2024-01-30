using __TemplateNamePlaceholderSwMacroFeature__.Sw.Properties;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.UI.Commands;
using Xarial.XCad.UI.Commands.Attributes;
using Xarial.XCad.UI.Commands.Enums;

namespace __TemplateNamePlaceholderSwMacroFeature__.Sw
{
    [ComVisible(true)]
    [Guid("04304922-1760-469A-8A2D-9A7F1055EFBC")]
    [Title("__TemplateNamePlaceholderSwMacroFeature__ SOLIDWORKS Macro Feature Add-In")]
    [Description("SOLIDWORKS macro feature add-in created with xCAD.NET")]
    public class MainSwAddIn : SwAddInEx
    {
        //command groups can be created by defining the enumeration and all its fields will be rendered as buttons
        [Title("__TemplateNamePlaceholderSwMacroFeature__")]
        [Description("Commands of __TemplateNamePlaceholderSwMacroFeature__")]
        private enum Commands_e
        {
            [Icon(typeof(Resources), nameof(Resources.cylinder_icon))]
            [Title("Create Cylinder")]
            [Description("Creates parametric cylinder")]
#if _SupportsInContext_
            [CommandItemInfo(true, true, WorkspaceTypes_e.Part | WorkspaceTypes_e.InContextPart, true)]
            CreateCylinder
#else
            [CommandItemInfo(true, true, WorkspaceTypes_e.Part, true)]
            CreateCylinder
#endif
        }

        //function is called when add-in is loading
        public override void OnConnect()
        {
            //creating command manager based on enum
            CommandManager.AddCommandGroup<Commands_e>().CommandClick += OnCommandClick;
        }

        //button click handler will pass the enum of the button being clicked
        private void OnCommandClick(Commands_e spec)
        {
            switch (spec)
            {
                case Commands_e.CreateCylinder:
#if _AddEditor_
                    Application.Documents.Active.Features.CreateCustomFeature<CylinderMacroFeatureDefinition, CylinderMacroFeatureData, CylinderPropertyPage>();
#else
                    //use the first selected plane or face as the reference
                    var refPlane = Application.Documents.Active.Selections.OfType<IXEntity>().FirstOrDefault(e => e is IXPlanarRegion);
                    if (refPlane != null)
                    {
                        //if not selected use front plane
                        refPlane = Application.Documents.Active.Features.OfType<IXPlane>().First();
                    }

                    Application.Documents.Active.Features.CreateCustomFeature<CylinderMacroFeatureDefinition, CylinderMacroFeatureData>(new CylinderMacroFeatureData()
                    {
                        Height = 0.1,
                        Radius = 0.01,
                        PlaneOrFace = refPlane,
                    });
#endif
                    break;
            }
        }
    }
}
