using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Features.CustomFeature.Attributes;
using Xarial.XCad.Features.CustomFeature.Enums;
using Xarial.XCad.Geometry;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Enums;

namespace __TemplateNamePlaceholderSwMacroFeature__.Sw
{
#if _SupportsEditBodies_
    public enum BooleanOptions_e
    {
        Extrude,

        [Title("Merge Extrude Results")]
        MergeExtrudeResults,

        Cut
    }

#endif
    //this corresponds to the data required to generate this macro feature
    public class CylinderMacroFeatureData
    {
        //face or plane. This entity will be associated as the parent relation and macro feature will update
        //when this entity changes
        public IXEntity PlaneOrFace { get; set; }

#if _AddDimensions_
        //marking this parameter to be served as the radial dimension
        [ParameterDimension(CustomFeatureDimensionType_e.Radial)]
#endif
        public double Radius { get; set; } = 0.1;

#if _AddDimensions_
        //marking this parameter to be served as the linear dimension
        [ParameterDimension(CustomFeatureDimensionType_e.Linear)]
#endif
        public double Height { get; set; } = 0.1;

        [ControlOptions(align: ControlLeftAlign_e.Indent)]
        public bool Reverse { get; set; } = false;
#if _SupportsEditBodies_
        
        [ParameterEditBody]
        public IXBody EditBody { get; set; }

        public BooleanOptions_e BooleanOptions { get; set; } = BooleanOptions_e.Extrude;
#endif
    }
}
