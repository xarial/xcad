using __TemplateNamePlaceholder__SwAddin.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.UI.PropertyPage.Services;
using Xarial.XCad.UI.PropertyPage.Structures;

namespace __TemplateNamePlaceholder__SwAddin
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
                args.Filter = selection is IXPlanarRegion;

                if (!args.Filter) 
                {
                    args.Reason = "Select planar face or plane";
                }
            }
        }

        public class LocationGroup 
        {
            [StandardControlIcon(BitmapLabelType_e.SelectFace)]
            [Description("Face or plane to place box on")]
            [SelectionBoxOptions(CustomFilter = typeof(PlanarRegionSelectionFilter))]
            public IXEntity PlaneOrFace { get; set; }
        }

        public class ParametersGroup
        {
            [NumberBoxOptions(NumberBoxUnitType_e.Length, 0, 1000, 0.01, false, 0.02, 0.001)]
            [Description("Width of the box")]
            [StandardControlIcon(BitmapLabelType_e.Width)]
            public double Width { get; set; } = 0.1;

            [NumberBoxOptions(NumberBoxUnitType_e.Length, 0, 1000, 0.01, false, 0.02, 0.001)]
            [Description("Height of the box")]
            [StandardControlIcon(BitmapLabelType_e.Thickness1)]
            public double Height { get; set; } = 0.1;

            [NumberBoxOptions(NumberBoxUnitType_e.Length, 0, 1000, 0.01, false, 0.02, 0.001)]
            [Description("Length of the box")]
            [StandardControlIcon(BitmapLabelType_e.Depth)]
            public double Length { get; set; } = 0.1;
        }

        public LocationGroup Location { get; }
        public ParametersGroup Parameters { get; }

        public BoxPropertyPage() 
        {
            Location = new LocationGroup();
            Parameters = new ParametersGroup();
        }
    }
}
