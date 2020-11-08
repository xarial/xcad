//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

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
using Xarial.XCad.Features.CustomFeature.Attributes;
using System.Linq;
using Xarial.XCad.SolidWorks.Geometry.Primitives;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.Base;

namespace SwAddInExample
{
    [ComVisible(true)]
    [MissingDefinitionErrorMessage("xCAD. Download the add-in")]
    public class SimpleMacroFeature : SwMacroFeatureDefinition 
    {
        public override CustomFeatureRebuildResult OnRebuild(ISwApplication app, ISwDocument model, ISwMacroFeature feature)
        {
            return base.OnRebuild(app, model, feature);
        }
    }

    [ComVisible(true)]
    [Icon(typeof(Resources), nameof(Resources.xarial))]
    [MissingDefinitionErrorMessage("xCAD. Download the add-in")]
    public class SampleMacroFeature : SwMacroFeatureDefinition<PmpMacroFeatData>
    {
        public override CustomFeatureRebuildResult OnRebuild(ISwApplication app, ISwDocument model, ISwMacroFeature feature, 
            PmpMacroFeatData parameters, out AlignDimensionDelegate<PmpMacroFeatData> alignDim)
        {
            alignDim = (n, d)=> 
            {
                switch (n) 
                {
                    case nameof(PmpMacroFeatData.Number):
                        this.AlignLinearDimension(d, new Point(0, 0, 0), new Vector(0, 1, 0));
                        break;

                    case nameof(PmpMacroFeatData.Angle):
                        this.AlignAngularDimension(d, new Point(0, 0, 0), new Point(-0.1, 0, 0), new Vector(0, 1, 0));
                        break;
                }
            };

            var sweepArc = app.MemoryGeometryBuilder.WireBuilder.PreCreateArc();
            sweepArc.Center = new Point(0, 0, 0);
            sweepArc.Axis = new Vector(0, 0, 1);
            sweepArc.Diameter = 0.01;
            sweepArc.Commit();

            var sweepLine = app.MemoryGeometryBuilder.WireBuilder.PreCreateLine();
            sweepLine.StartCoordinate = new Point(0, 0, 0);
            sweepLine.EndCoordinate = new Point(1, 1, 1);
            sweepLine.Commit();

            var sweep = (ISwTempSweep)app.MemoryGeometryBuilder.SolidBuilder.PreCreateSweep();
            sweep.Profiles = new ISwTempRegion[] { app.MemoryGeometryBuilder.CreatePlanarSheet(sweepArc).Bodies.OfType<ISwTempPlanarSheetBody>().First() };
            sweep.Path = sweepLine;
            sweep.Commit();

            parameters.Number = parameters.Number + 1;
            return new CustomFeatureBodyRebuildResult() { Bodies = sweep.Bodies };
        }
    }
}
