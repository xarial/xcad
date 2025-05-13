﻿using System.Collections.Generic;
using System.Runtime.InteropServices;
using Xarial.XCad.Features.CustomFeature.Delegates;
using Xarial.XCad.Features.CustomFeature.Structures;
using Xarial.XCad.Geometry;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features.CustomFeature;

namespace Xarial.XCad.Documentation.CustomFeature
{
    public class MacroFeatureEditBodiesParams
    {
        #region single
        public IXBody InputBody { get; set; }
        #endregion single

        #region multiple
        public IXBody EditBody1 { get; set; }
        public IXBody EditBody2 { get; set; }
        #endregion multiple

        #region list
        public List<IXBody> EditBodies { get; set; }
        #endregion list
    }

    [ComVisible(true)]
    public class EditBodiesMacroFeature : SwMacroFeatureDefinition<MacroFeatureEditBodiesParams>
    {
        public override CustomFeatureRebuildResult OnRebuild(ISwApplication app, ISwDocument model,
            ISwMacroFeature<MacroFeatureEditBodiesParams> feature, out AlignDimensionDelegate<MacroFeatureEditBodiesParams> alignDim)
        {
            alignDim = null;
            return new CustomFeatureRebuildResult();
        }
    }
}
