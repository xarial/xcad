using System.Runtime.InteropServices;
using Xarial.XCad.Features.CustomFeature.Delegates;
using Xarial.XCad.Features.CustomFeature.Structures;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features.CustomFeature;

namespace Xarial.XCad.Documentation
{
    public class MacroFeaturePrimitiveParams
    {
        public string Parameter1 { get; set; }
        public int Parameter2 { get; set; }
    }

    //this macro feature has two parameters (Parameter1 and Parameter2)
    [ComVisible(true)]
    public class PrimitiveParametersMacroFeature : SwMacroFeatureDefinition<MacroFeaturePrimitiveParams>
    {
        public override CustomFeatureRebuildResult OnRebuild(ISwApplication app, ISwDocument model,
            ISwMacroFeature<MacroFeaturePrimitiveParams> feature, out AlignDimensionDelegate<MacroFeaturePrimitiveParams> alignDim)
        {
            alignDim = null;
            return new CustomFeatureRebuildResult();
        }
    }
}
