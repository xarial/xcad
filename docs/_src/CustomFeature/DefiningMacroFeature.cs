using System;
using System.Runtime.InteropServices;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Documentation.Properties;
using Xarial.XCad.Features.CustomFeature.Attributes;
using Xarial.XCad.Features.CustomFeature.Delegates;
using Xarial.XCad.Features.CustomFeature.Enums;
using Xarial.XCad.Features.CustomFeature.Structures;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features.CustomFeature;

namespace Xarial.XCad.Documentation
{
    public class MySimpleMacroFeatureParameters
    {
        public string Parameter1 { get; set; }
    }

    [ComVisible(true)]
    [Guid("47827004-8897-49F5-9C65-5B845DC7F5AC")]
    [ProgId("CodeStack.MyMacroFeature")]
    [Title("MyMacroFeature")]
    [CustomFeatureOptions(CustomFeatureOptions_e.AlwaysAtEnd)]
    [Icon(typeof(Resources), nameof(Resources.macro_feature_icon))]
    public class MySimpleMacroFeature : SwMacroFeatureDefinition<MySimpleMacroFeatureParameters>
    {
        public override CustomFeatureRebuildResult OnRebuild(ISwApplication app, ISwDocument model, 
            ISwMacroFeature feature, MySimpleMacroFeatureParameters parameters,
            out AlignDimensionDelegate<MySimpleMacroFeatureParameters> alignDim)
        {
            alignDim = null;
            return new CustomFeatureRebuildResult() { Result = true };
        }
    }
}
