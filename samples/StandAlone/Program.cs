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
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.Toolkit.Utils;

namespace StandAlone
{
    class Program
    {
        static void Main(string[] args)
        {
            //var app = SwApplicationFactory.Start(Xarial.XCad.SolidWorks.Enums.SwVersion_e.Sw2020);
            var app = SwApplicationFactory.FromProcess(Process.GetProcessesByName("SLDWORKS").First());

            //SketchSegmentColors(app);

            //CreateDrawingView(app);

            //var sw = Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application")) as ISldWorks;
            //sw.Visible = true;

            //var app = SwApplication.FromPointer(sw);

            //CreateSketchEntities(app);

            //TraverseSelectedFaces(app);

            CreateSweepFromSelection(app);
            //CreateTempGeometry(app);

            //CreateSweepFromSelection(app);
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
            var view = partDoc.ModelViews[StandardViewType_e.Right];
            var drw = app.Documents.NewDrawing();
            var drwView = drw.Sheets.Active.DrawingViews.CreateModelViewBased(view);
        }

        private static void CreateSketchEntities(IXApplication app)
        {
            var sketch3D = app.Documents.Active.Features.PreCreate3DSketch();
            var line = (IXSketchLine)sketch3D.Entities.PreCreateLine();
            line.Color = System.Drawing.Color.Green;
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

        private static void CreateSweepFromSelection(ISwApplication app) 
        {
            var doc = app.Documents.Active;
            var feat = doc.Selections.First() as ISwFeature;
            var sketch = SwObjectFactory.FromDispatch<ISwSketch2D>(feat.Feature.GetSpecificFeature2() as ISketch, doc);
            var reg = sketch.Regions.First();

            var pathSeg = app.Documents.Active.Selections.Last() as IXSketchSegment;

            var pathCurve = pathSeg.Definition;

            var sweep = app.MemoryGeometryBuilder.SolidBuilder.PreCreateSweep();
            sweep.Profiles = new IXRegion[] { reg };
            sweep.Path = pathCurve;
            sweep.Commit();

            var body = (sweep.Bodies.First() as ISwBody).Body;

            (app.Documents.Active as ISwPart).Part.CreateFeatureFromBody3(body, false, 0);
        }

        private static void CreateTempGeometry(IXApplication app) 
        {
            var sweepArc = app.MemoryGeometryBuilder.WireBuilder.PreCreateArc();
            sweepArc.Center = new Point(0, 0, 0);
            sweepArc.Axis = new Vector(0, 0, 1);
            sweepArc.Diameter = 0.01;
            sweepArc.Commit();

            var sweepLine = app.MemoryGeometryBuilder.WireBuilder.PreCreateLine();
            sweepLine.StartCoordinate = new Point(0, 0, 0);
            sweepLine.EndCoordinate = new Point(1, 1, 1);
            sweepLine.Commit();

            var sweep = app.MemoryGeometryBuilder.SolidBuilder.PreCreateSweep();
            sweep.Profiles = new IXRegion[] { app.MemoryGeometryBuilder.CreatePlanarSurface(sweepArc).Bodies.OfType<IXPlanarSheetBody>().First() };
            sweep.Path = sweepLine;
            sweep.Commit();

            var body = (sweep.Bodies.First() as ISwBody).Body;

            (app.Documents.Active as ISwPart).Part.CreateFeatureFromBody3(body, false, 0);

            var cone = app.MemoryGeometryBuilder.CreateSolidCone(
                new Point(0, 0, 0), 
                new Vector(1, 1, 1), 
                0.1, 0.05, 0.2);
            
            body = (cone.Bodies.First() as ISwBody).Body;

            (app.Documents.Active as ISwPart).Part.CreateFeatureFromBody3(body, false, 0);

            var arc = app.MemoryGeometryBuilder.WireBuilder.PreCreateArc();
            arc.Center = new Point(-0.1, 0, 0);
            arc.Axis = new Vector(0, 0, 1);
            arc.Diameter = 0.01;
            arc.Commit();

            var axis = app.MemoryGeometryBuilder.WireBuilder.PreCreateLine();
            axis.StartCoordinate = new Point(0, 0, 0);
            axis.EndCoordinate = new Point(0, 1, 0);
            axis.Commit();

            var rev = app.MemoryGeometryBuilder.SolidBuilder.PreCreateRevolve();
            rev.Angle = Math.PI * 2;
            rev.Axis = axis;
            rev.Profiles = new IXRegion[] { app.MemoryGeometryBuilder.CreatePlanarSurface(arc).Bodies.OfType<IXPlanarSheetBody>().First() };
            rev.Commit();

            body = (rev.Bodies.First() as ISwBody).Body;

            (app.Documents.Active as ISwPart).Part.CreateFeatureFromBody3(body, false, 0);

            var box = app.MemoryGeometryBuilder.CreateSolidBox(
                new Point(0, 0, 0), 
                new Vector(1, 1, 1),
                new Vector(1, 1, 1).CreateAnyPerpendicular(),
                0.1, 0.2, 0.3);

            body = (box.Bodies.First() as ISwBody).Body;

            (app.Documents.Active as ISwPart).Part.CreateFeatureFromBody3(body, false, 0);

            var polyline = app.MemoryGeometryBuilder.WireBuilder.PreCreatePolyline();
            polyline.Points = new Point[] 
            {
                new Point(0, 0, 0),
                new Point(0.1, 0.1, 0),
                new Point(0.2, 0, 0),
                new Point(0, 0, 0)
            };
            polyline.Commit();

            var extr = app.MemoryGeometryBuilder.SolidBuilder.PreCreateExtrusion();
            extr.Depth = 0.5;
            extr.Direction = new Vector(1, 1, 1);
            extr.Profiles = new IXRegion[] { app.MemoryGeometryBuilder.CreatePlanarSurface(polyline).Bodies.OfType<IXPlanarSheetBody>().First() };
            extr.Commit();

            body = (extr.Bodies.First() as ISwBody).Body;

            (app.Documents.Active as ISwPart).Part.CreateFeatureFromBody3(body, false, 0);

            var cyl = app.MemoryGeometryBuilder.CreateSolidCylinder(
                new Point(0, 0, 0), new Vector(1, 0, 0), 0.1, 0.2);

            body = (cyl.Bodies.First() as ISwBody).Body;

            (app.Documents.Active as ISwPart).Part.CreateFeatureFromBody3(body, false, 0);
        }
    }
}
