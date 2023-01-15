using __TemplateNamePlaceholder__SwMacroFeatureAddIn.Properties;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Xarial.XCad;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Geometry;
using Xarial.XCad.SolidWorks.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.UI.PropertyPage.Services;
using Xarial.XCad.UI.PropertyPage.Structures;

namespace __TemplateNamePlaceholder__.Sw.AddIn
{
    [ComVisible(true)]
    [Guid("6397DDCC-CFF8-448C-B955-4147C628BE0A")]
    [Icon(typeof(Resources), nameof(Resources.cylinder_icon))]
    [Title("Create Cylinder")]
    public class CylinderPropertyPage : SwPropertyManagerPageHandler
    {
        private class PlanarRegionSelectionFilter : ISelectionCustomFilter
        {
            public void Filter(IControl selBox, IXSelObject selection, SelectionCustomFilterArguments args)
            {
                //only allow planar region (e.g. planar faces or planes)
                args.Filter = selection is IXPlanarRegion;

                if (!args.Filter) 
                {
                    //show the custom warning to the user of why this entity cannot be selected
                    args.Reason = "Select planar face or plane";
                }
            }
        }

        public class LocationGroup
        {
            //any selectable entity will be rendered as the selection box
            //default filter will only allow selection of faces and planes and custom filter will additionlly
            //excluded non planar faces
            [SelectionBoxOptions(Filters = new SelectType_e[] { SelectType_e.Faces, SelectType_e.Planes },
                CustomFilter = typeof(PlanarRegionSelectionFilter))]
            [StandardControlIcon(BitmapLabelType_e.SelectFace)]
            [Description("Face or plane to place box on")]
            public IXEntity PlaneOrFace { get; set; }

            [ControlOptions(align: ControlLeftAlign_e.Indent)]
            [Description("Reverses the result of the cylinder")]
            public bool Reverse { get; set; }
#if _SupportsEditBodies_

            [Description("Options for the bodies result")]
            [ControlOptions(align: ControlLeftAlign_e.Indent)]
            public BooleanOptions_e BooleanOptions { get; set; }
#endif
        }

        public class ParametersGroup
        {
            //public property of type double will be rendered as the number box
            [NumberBoxOptions(NumberBoxUnitType_e.Length, 0, 1000, 0.01, false, 0.02, 0.001)]
            [Description("Radius of the cylinder")]
            [StandardControlIcon(BitmapLabelType_e.Radius)]
            public double Radius { get; set; }

            [NumberBoxOptions(NumberBoxUnitType_e.Length, 0, 1000, 0.01, false, 0.02, 0.001)]
            [Description("Height of the cylinder")]
            [Icon(typeof(Resources), nameof(Resources.height_icon))]
            public double Height { get; set; }
        }

        //classes will be rendered as property manager page groups
        public LocationGroup Location { get; }
        public ParametersGroup Parameters { get; }

        public CylinderPropertyPage() 
        {
            Location = new LocationGroup();
            Parameters = new ParametersGroup();
        }
    }
}
