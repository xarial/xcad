using System.Runtime.InteropServices;
using Xarial.XCad;
using SwAddInExample.Properties;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Features.CustomFeature.Structures;
using Xarial.XCad.Features.CustomFeature;
using Xarial.XCad.Features.CustomFeature.Delegates;
using Xarial.XCad.Geometry;
using Xarial.XCad.SolidWorks.Features.CustomFeature;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;

namespace SwAddInExample
{
    [ComVisible(true)]
    [Icon(typeof(Resources), nameof(Resources.xarial))]
    public class SampleMacroFeature : SwMacroFeatureDefinition<PmpData>
    {
        public override CustomFeatureRebuildResult OnRebuild(SwApplication app, SwDocument model, SwMacroFeature feature, 
            PmpData parameters, out AlignDimensionDelegate<PmpData> alignDim)
        {
            alignDim = (n, d)=> 
            {
                switch (n) 
                {
                    case nameof(PmpData.Number):
                        this.AlignLinearDimension(d, new Point(0, 0, 0), new Vector(0, 1, 0));
                        break;

                    case nameof(PmpData.Angle):
                        this.AlignAngularDimension(d, new Point(0, 0, 0), new Point(-0.1, 0, 0), new Vector(0, 1, 0));
                        break;
                }
            };

            var box = app.GeometryBuilder.CreateBox(new Point(0, 0, 0), new Vector(1, 0, 0), 0.1, 0.1, 0.1);
            parameters.Number = parameters.Number + 1;
            return new CustomFeatureBodyRebuildResult() { Bodies = new IXBody[] { box } };
        }
    }
}
