using Xarial.XCad.Features.CustomFeature.Attributes;
using Xarial.XCad.Features.CustomFeature.Enums;

namespace Xarial.XCad.Documentation
{
    public class DimensionMacroFeatureParams
    {
        [ParameterDimension(CustomFeatureDimensionType_e.Linear)]
        public double FirstDimension { get; set; } = 0.01;

        [ParameterDimension(CustomFeatureDimensionType_e.Radial)]
        public double SecondDimension { get; set; }
    }    
}
