using System.Collections.Generic;
using System.Runtime.InteropServices;
using Xarial.XCad.Features.CustomFeature.Structures;
using Xarial.XCad.Geometry;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features.CustomFeature;

namespace Xarial.XCad.Documentation.CustomFeature
{
    public class MacroFeatureEditBodiesParams
    {
        //--- single
        public IXBody InputBody { get; set; }
        //---

        //--- multiple
        public IXBody EditBody1 { get; set; }
        public IXBody EditBody2 { get; set; }
        //---

        //--- list
        public List<IXBody> EditBodies { get; set; }
        //---
    }

    [ComVisible(true)]
    public class EditBodiesMacroFeature : SwMacroFeatureDefinition<MacroFeatureEditBodiesParams>
    {
        public override CustomFeatureRebuildResult OnRebuild(ISwApplication app, ISwDocument model,
            ISwMacroFeature<MacroFeatureEditBodiesParams> feature) => new CustomFeatureRebuildResult();
    }
}
