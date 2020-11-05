using System.Runtime.InteropServices;
using Xarial.XCad.Features.CustomFeature.Enums;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features.CustomFeature;

namespace Xarial.XCad.Documentation
{
    [ComVisible(true)]
    public class UpdateStateMacroFeature : SwMacroFeatureDefinition
    {
        public override CustomFeatureState_e OnUpdateState(ISwApplication app, ISwDocument model, ISwMacroFeature feature)
        {
            //disallow editing or suppressing of the feature
            return CustomFeatureState_e.CannotBeDeleted 
                | CustomFeatureState_e.CannotBeSuppressed;
        }
    }
}
