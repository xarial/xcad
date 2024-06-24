using System.Xml.Linq;
using Xarial.XCad.Annotations;
using Xarial.XCad.Features.CustomFeature;
using Xarial.XCad.Features.CustomFeature.Structures;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features.CustomFeature;

namespace Xarial.XCad.Documentation
{
    public class MyDimRegenDataMacroFeature : SwMacroFeatureDefinition<DimensionMacroFeatureParams>
    {        
        private IXBody[] GetBodies()
        {
            //create bodies for geometry
            return null;
        }

        public override CustomFeatureRebuildResult OnRebuild(ISwApplication app, ISwDocument model, 
            ISwMacroFeature<DimensionMacroFeatureParams> feature)
        {
            var parameters = feature.Parameters;

            var resBodies = GetBodies(); //generating bodies

            return new CustomFeatureBodyRebuildResult() { Bodies = resBodies }; //returning custom rebuild result
        }

        public override void OnAlignDimension(IXCustomFeature<DimensionMacroFeatureParams> feat, string paramName, IXDimension dim)
        {
            switch (paramName)
            {
                case nameof(DimensionMacroFeatureParams.FirstDimension):
                    this.AlignLinearDimension(dim, new Point(0, 0, 0), new Vector(1, 0, 0));
                    break;

                case nameof(DimensionMacroFeatureParams.SecondDimension):
                    this.AlignRadialDimension(dim, new Point(0, 0, 0), new Vector(0, 0, 1));
                    break;
            }
        }
    }
}
