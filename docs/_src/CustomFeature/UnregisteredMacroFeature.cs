using System.Runtime.InteropServices;
using Xarial.XCad.Features.CustomFeature.Attributes;
using Xarial.XCad.SolidWorks.Features.CustomFeature;

namespace Xarial.XCad.Documentation
{
    [ComVisible(true)]
    [MissingDefinitionErrorMessage("xCAD. Download the add-in")]
    public class UnregisteredMacroFeature : SwMacroFeatureDefinition
    {
    }
}
