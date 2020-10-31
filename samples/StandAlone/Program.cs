//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.Toolkit.Utils;

namespace StandAlone
{
    class Program
    {
        static void Main(string[] args)
        {
            //var app = SwApplication.Start(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2020);
            var app = SwApplication.FromProcess(Process.GetProcessesByName("SLDWORKS").First());
                       
            //SketchSegmentColors(app);

            //CreateDrawingView(app);

            //var sw = Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application")) as ISldWorks;
            //sw.Visible = true;

            //var app = SwApplication.FromPointer(sw);

            CreateSketchEntities(app);

            //TraverseSelectedFaces(app);
        }
        
        private static void SketchSegmentColors(IXApplication app) 
        {
            var seg = app.Documents.Active.Selections.First() as IXSketchSegment;
            var color = seg.Color;
            seg.Color = System.Drawing.Color.Purple;
        }

        private static void CreateDrawingView(IXApplication app) 
        {
            var partDoc = app.Documents.Active as IXDocument3D;
            var view = partDoc.Views[StandardViewType_e.Right];
            var drw = app.Documents.NewDrawing();
            var drwView = drw.Sheets.Active.DrawingViews.CreateModelViewBased(view);
        }

        private static void CreateSketchEntities(IXApplication app)
        {
            var sketch3D = app.Documents.Active.Features.PreCreate3DSketch();
            var line = (IXSketchLine)sketch3D.Entities.PreCreateLine();
            line.StartCoordinate = new Point(0.1, 0.1, 0.1);
            line.EndCoordinate = new Point(0.2, 0.2, 0.2);
            sketch3D.Entities.AddRange(new IXSketchEntity[] { line });

            app.Documents.Active.Features.Add(sketch3D);

            var c = line.EndPoint.Coordinate;
            sketch3D.IsEditing = true;
            line.EndPoint.Coordinate = new Point(0.3, 0.3, 0.3);
            sketch3D.IsEditing = false;
        }

        private static void TraverseSelectedFaces(IXApplication app) 
        {
            foreach (var face in app.Documents.Active.Selections.OfType<IXFace>()) 
            {
                Console.WriteLine(face.Area);
            }
        }
    }
}
