using System.Runtime.InteropServices;
using Xarial.XCad.Features.CustomFeature.Attributes;
using Xarial.XCad.Features.CustomFeature.Delegates;
using Xarial.XCad.Features.CustomFeature.Enums;
using Xarial.XCad.Features.CustomFeature.Structures;
using Xarial.XCad.Geometry;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features.CustomFeature;

namespace Xarial.XCad.Documentation
{   
    public class MacroFeatureParams
    {
        // text metadata
        public string TextParameter { get; set; }

        // boolean metadata
        public bool ToggleParameter { get; set; }

        // any dependency selection
        public IXSelObject FaceSelectionParameter { get; set; }

        // edit body - base body which macro feature is modifying
        [ParameterEditBody]
        public IXBody InputBody { get; set; }

        // macro feature dimension. Value of the dimension will be sync with the proeprty
        [ParameterDimension(CustomFeatureDimensionType_e.Linear)]
        public double LinearDimension { get; set; }
    }

    [ComVisible(true)]
    public class MyParamsMacroFeature : SwMacroFeatureDefinition<MacroFeatureParams>
    {
        public override CustomFeatureRebuildResult OnRebuild(SwApplication app, SwDocument model, SwMacroFeature feature, MacroFeatureParams parameters, out AlignDimensionDelegate<MacroFeatureParams> alignDim)
        {
            var txt = parameters.TextParameter;
            var inputBody = parameters.InputBody;

            alignDim = null;
            return new CustomFeatureRebuildResult();
        }
    }
}
