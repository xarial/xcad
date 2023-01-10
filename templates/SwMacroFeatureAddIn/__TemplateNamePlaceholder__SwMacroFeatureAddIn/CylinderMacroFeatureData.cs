using Xarial.XCad.Features.CustomFeature.Attributes;
using Xarial.XCad.Features.CustomFeature.Enums;
using Xarial.XCad.Geometry;

namespace __TemplateNamePlaceholder__.Sw.AddIn
{
    //this corresponds to the data required to generate this macro feature
    public class CylinderMacroFeatureData
    {
        //face or plane. This entity will be associated as the parent relation and macro feature will update
        //when this entity changes
        public IXEntity PlaneOrFace { get; set; }
        
        //marking this parameter to be served as the radial dimension
        [ParameterDimension(CustomFeatureDimensionType_e.Radial)]
        public double Radius { get; set; } = 0.1;

        //marking this parameter to be served as the linear dimension
        [ParameterDimension(CustomFeatureDimensionType_e.Linear)]
        public double Height { get; set; } = 0.1;
    }
}
