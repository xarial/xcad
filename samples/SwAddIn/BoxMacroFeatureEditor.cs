//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Features.CustomFeature.Delegates;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features.CustomFeature;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.UI.PropertyPage;

namespace SwAddInExample
{
    [ComVisible(true)]
    public class BoxData : SwPropertyManagerPageHandler
    {
        public double Width { get; set; } = 0.1;
        public double Height { get; set; } = 0.2;
        public double Length { get; set; } = 0.3;
    }

    [ComVisible(true)]
    public class BoxMacroFeatureEditor : SwMacroFeatureDefinition<BoxData, BoxData>
    {
        public override SwBody[] CreateGeometry(SwApplication app, SwDocument model, 
            BoxData data, bool isPreview, out AlignDimensionDelegate<BoxData> alignDim)
        {
            alignDim = null;

            var box = (SwBody)app.MemoryGeometryBuilder.CreateSolidBox(
                new Point(0, 0, 0), new Vector(0, 0, 1), new Vector(1, 0, 0).CreateAnyPerpendicular(),
                data.Width, data.Height, data.Length).Bodies.First();

            return new SwBody[] { box };
        }
    }
}
