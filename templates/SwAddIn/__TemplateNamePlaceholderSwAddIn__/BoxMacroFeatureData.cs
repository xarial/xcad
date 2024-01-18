using Xarial.XCad.Features.CustomFeature.Attributes;
using Xarial.XCad.Features.CustomFeature.Enums;
using Xarial.XCad.Geometry;

namespace __TemplateNamePlaceholderSwAddIn__.Sw
{
    //this corresponds to the data required to generate this macro feature
    public class BoxMacroFeatureData
    {
        //face or plane. This entity will be associated as the parent relation and macro feature will update
        //when this entity changes
        public IXEntity PlaneOrFace { get; set; }
        
        //marking this parameter to be served as the dimension
        [ParameterDimension(CustomFeatureDimensionType_e.Linear)]
        public double Width { get; set; } = 0.1;

        [ParameterDimension(CustomFeatureDimensionType_e.Linear)]
        public double Height { get; set; } = 0.1;

        [ParameterDimension(CustomFeatureDimensionType_e.Linear)]
        public double Length { get; set; } = 0.1;
    }
}
