using SolidWorks.Interop.sldworks;
using System.Linq;
using System.Runtime.InteropServices;
using Xarial.XCad.Features.CustomFeature.Structures;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features.CustomFeature;

namespace Xarial.XCad.Documentation
{
    //returns successful regeneration without bodies
    [ComVisible(true)]
    public class RegenerationNoResultsMacroFeature : SwMacroFeatureDefinition
    {
        public override CustomFeatureRebuildResult OnRebuild(ISwApplication app, ISwDocument model, ISwMacroFeature feature)
        {
            return new CustomFeatureRebuildResult()
            {
                Result = true
            };
        }
    }

    // returns regeneration error
    [ComVisible(true)]
    public class RegenerationRebuildErrorMacroFeature : SwMacroFeatureDefinition
    {
        public override CustomFeatureRebuildResult OnRebuild(ISwApplication app, ISwDocument model, ISwMacroFeature feature)
        {
            return new CustomFeatureRebuildResult()
            {
                Result = false,
                ErrorMessage = "Failed to regenerate this feature"
            };
        }
    }

    //return body
    [ComVisible(true)]
    public class RegenerationBodyMacroFeature : SwMacroFeatureDefinition
    {
        public override CustomFeatureRebuildResult OnRebuild(ISwApplication app, ISwDocument model, ISwMacroFeature feature)
        {
            var body = app.MemoryGeometryBuilder.CreateSolidBox(new Point(0, 0, 0),
                new Vector(1, 0, 0), new Vector(0, 1, 0),
                0.1, 0.1, 0.1);

            return new CustomFeatureBodyRebuildResult()
            {
                Bodies = body.Bodies.ToArray()
            };
        }
    }
}
