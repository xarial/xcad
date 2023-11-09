using __TemplateNamePlaceholder__SwAddin.Properties;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Xarial.XCad;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Features;
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
    [Guid("1825D395-A8A2-484F-B9D6-C2D04B2AB9B3")]
    [Icon(typeof(Resources), nameof(Resources.box_icon))]
    [Title("Create Box")]
    public class BoxPropertyPage : SwPropertyManagerPageHandler
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
            [StandardControlIcon(BitmapLabelType_e.SelectFace)]
            [Description("Face or plane to place box on")]
            [SelectionBoxOptions(Filters = new Type[] { typeof(IXFace), typeof(IXPlane) },
                CustomFilter = typeof(PlanarRegionSelectionFilter))]
            //default filter will only allow selection of faces and planes and custom filter will additionlly
            //excluded non planar faces
            public IXEntity PlaneOrFace { get; set; }
        }

        public class ParametersGroup
        {
            //public property of type double will be rendered as the number box
            [NumberBoxOptions(NumberBoxUnitType_e.Length, 0, 1000, 0.01, false, 0.02, 0.001)]
            [Description("Width of the box")]
            [Icon(typeof(Resources), nameof(Resources.width_icon))]
            public double Width { get; set; } = 0.1;

            [NumberBoxOptions(NumberBoxUnitType_e.Length, 0, 1000, 0.01, false, 0.02, 0.001)]
            [Description("Height of the box")]
            [Icon(typeof(Resources), nameof(Resources.height_icon))]
            public double Height { get; set; } = 0.1;

            [NumberBoxOptions(NumberBoxUnitType_e.Length, 0, 1000, 0.01, false, 0.02, 0.001)]
            [Description("Length of the box")]
            [Icon(typeof(Resources), nameof(Resources.length_icon))]
            public double Length { get; set; } = 0.1;
        }

        //classes will be rendered as property manager page groups
        public LocationGroup Location { get; }
        public ParametersGroup Parameters { get; }

        public BoxPropertyPage() 
        {
            Location = new LocationGroup();
            Parameters = new ParametersGroup();
        }
    }
}
