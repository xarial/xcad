using System.Runtime.InteropServices;
using Xarial.XCad.Features.CustomFeature.Structures;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features.CustomFeature;

namespace Xarial.XCad.Documentation
{
    public class EditMacroFeatureDefinitionParameters
    {
        //add properties
    }

    [ComVisible(true)]
    public class EditMacroFeatureDefinition : SwMacroFeatureDefinition<EditMacroFeatureDefinitionParameters>
    {
        public override CustomFeatureRebuildResult OnRebuild(ISwApplication app, ISwDocument model, 
            ISwMacroFeature<EditMacroFeatureDefinitionParameters> feature)
            => new CustomFeatureRebuildResult() { Result = true };

        public override bool OnEditDefinition(ISwApplication app, ISwDocument model, ISwMacroFeature<EditMacroFeatureDefinitionParameters> feature)
        {
            using (var editor = feature.Edit())
            {
                if (ShowPage(feature.Parameters, out EditMacroFeatureDefinitionParameters newParams))
                {
                    feature.Parameters = newParams;
                    return true;
                }
                else
                {
                    editor.Cancel = true;
                    return false;
                }
            }
        }

        private bool ShowPage(EditMacroFeatureDefinitionParameters parameters, out EditMacroFeatureDefinitionParameters newParameters)
        {
            //Show property page or any other user interface

            newParameters = null;
            return true;
        }
    }
}
