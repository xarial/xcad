//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Extensions;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.Enums;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Curves;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.SwDocumentManager;
using Xarial.XCad.Toolkit;
using Xarial.XCad.Toolkit.Utils;

namespace StandAlone
{
    public class MyLogger : IXLogger
    {
        public void Log(string msg, LoggerMessageSeverity_e severity = LoggerMessageSeverity_e.Information)
        {
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var app = SwApplicationFactory.Create(null, ApplicationState_e.Default);
                app.ShowMessageBox("Hello World!");

                //var app = SwApplicationFactory.FromProcess(Process.GetProcessesByName("SLDWORKS").First());

                //var dmApp = SwDmApplicationFactory.Create(
                //    System.Environment.GetEnvironmentVariable("SW_DM_KEY", EnvironmentVariableTarget.Machine));

                //RenameFiles(app);

                //ParseViewPolylines(app);

                //StructuralMembersTest(app);

                //AnnotationColor(app);

                //CreateLoftFromSelection(app);

                //CustomServices();

                //Progress(app);

                //SketchSegmentColors(app);

                //CreateDrawingView(app);

                //var sw = Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application")) as ISldWorks;
                //sw.Visible = true;

                //var app = SwApplication.FromPointer(sw);

                //CreateSketchEntities(app);

                //TraverseSelectedFaces(app);

                //CreateSweepFromSelection(app);
                CreateTempGeometry(app);

                //CreateSweepFromSelection(app);
            }
            catch 
            {
            }

            Console.ReadLine();
        }

        private static void RenameFiles(IXApplication app)
        {
            void CommitAllDependencies(IXDocument doc) 
            {
                if (!doc.IsCommitted) 
                {
                    doc.Commit();
                }

                foreach (var dep in doc.Dependencies) 
                {
                    CommitAllDependencies(dep);
                }
            }

            var assm = app.Documents.PreCreateAssembly();
            assm.Path = @"D:\Assem1\Assem1.SLDASM";

            if (app is ISwDmApplication)
            {
                CommitAllDependencies(assm);
            }

            assm.Dependencies.ReplaceAll(x => 
            {
                var newPath = Path.Combine(Path.GetDirectoryName(x), "_" + Path.GetFileName(x));

                if (!File.Exists(newPath)) 
                {
                    File.Copy(x, newPath);
                }

                return newPath;
            });

            if (app is ISwDmApplication)
            {
                foreach (var dep in assm.Dependencies.TryIterateAll()) 
                {
                    dep.Save();
                    dep.Close();
                }

                assm.Save();
                assm.Close();
            }
        }

        private static void ParseViewPolylines(ISwApplication app) 
        {
            var drw = (ISwDrawing)app.Documents.Active;

            var view = drw.Selections.OfType<ISwDrawingView>().First();

            //var view = (ISwDrawingView)drw.Sheets.Active.DrawingViews.First();

            var polylinesData = view.Polylines;

            //select
            foreach (var polylineData in polylinesData)
            {
                view.DrawingView.SelectEntity(((ISwEntity)polylineData.Entity).Entity, true);
            }
            //

            //create in 3D view
            var sketch = view.ReferencedDocument.Features.PreCreate3DSketch();

            var ents = new List<IXWireEntity>();

            foreach (var polylineData in polylinesData)
            {
                for (int i = 0; i < polylineData.Points.Length - 1; i++)
                {
                    var line = sketch.Entities.PreCreateLine();
                    line.Geometry = new Line(polylineData.Points[i], polylineData.Points[i + 1]);
                    ents.Add(line);
                }
            }

            sketch.Entities.AddRange(ents);

            sketch.Commit();
            //

            //create in sheet
            var sheetSketch = drw.Sheets.Active.Sketch;

            using (var editor = sheetSketch.Edit())
            {
                var sheetEnts = new List<IXWireEntity>();

                foreach (var polylineData in polylinesData)
                {
                    for (int i = 0; i < polylineData.Points.Length - 1; i++)
                    {
                        var line = sheetSketch.Entities.PreCreateLine();
                        line.Geometry = new Line(polylineData.Points[i], polylineData.Points[i + 1]);
                        sheetEnts.Add(line);
                    }
                }

                sheetSketch.Entities.AddRange(ents);
            }
            //
        }
        
        private static void StructuralMembersTest(ISwApplication app)
        {
            var doc = app.Documents.Active;

            var feat = doc.Selections.OfType<ISwStructuralMember>().First();

            foreach (var grp in feat.Groups)
            {
                foreach (var piece in grp.Pieces)
                {
                    DrawPlaneOrientation(doc, piece.ProfilePlane);
                }
            }
        }

        private static void DrawPlaneOrientation(IXDocument doc, Plane plane) 
        {
            var sketch = doc.Features.PreCreate3DSketch();
            
            var pt = sketch.Entities.PreCreatePoint();
            pt.Coordinate = plane.Point;

            var xLine = (IXSketchLine)sketch.Entities.PreCreateLine();
            xLine.Geometry = new Line(plane.Point, plane.Point.Move(plane.Direction, 0.1));
            xLine.Color = System.Drawing.Color.Red;

            var yLine = (IXSketchLine)sketch.Entities.PreCreateLine();
            yLine.Geometry = new Line(plane.Point, plane.Point.Move(plane.Reference, 0.2));
            yLine.Color = System.Drawing.Color.Green;

            var zLine = (IXSketchLine)sketch.Entities.PreCreateLine();
            zLine.Geometry = new Line(plane.Point, plane.Point.Move(plane.Normal, 0.3));
            zLine.Color = System.Drawing.Color.Blue;

            sketch.Entities.AddRange(new IXWireEntity[] { pt, xLine, yLine, zLine });

            sketch.Commit();
        }

        private static void AnnotationColor(ISwApplication app)
        {
            var note = app.Documents.Active.Selections.OfType<IXNote>().First();
            var c = note.Color;
            note.Color = System.Drawing.Color.Green;
            note.Color = null;
        }

        private static void CustomServices() 
        {
            var app = SwApplicationFactory.PreCreate();
            var svcColl = new ServiceCollection();
            svcColl.Add<IXLogger, MyLogger>();
            app.CustomServices = svcColl;
            app.Commit();
        }

        private static void Progress(IXApplication app) 
        {
            using (var prg = app.CreateProgress())
            {
                for (int i = 0; i < 100; i++) 
                {
                    prg.Report((double)i / 100);
                    prg.SetStatus(i.ToString());
                    System.Threading.Thread.Sleep(100);
                }
            }
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
            line.Geometry = new Line(new Point(0.1, 0.1, 0.1), new Point(0.2, 0.2, 0.2));
            sketch3D.Entities.AddRange(new IXSketchEntity[] { line });

            app.Documents.Active.Features.Add(sketch3D);

            var c = line.EndPoint.Coordinate;

            using (var editor = sketch3D.Edit()) 
            {
                line.EndPoint.Coordinate = new Point(0.3, 0.3, 0.3);
            }

            using (var editor = sketch3D.Edit())
            {
                var line2 = (IXSketchLine)sketch3D.Entities.PreCreateLine();
                line2.Geometry = new Line(new Point(0, 0, 0), new Point(0.1, 0.2, 0.3));
                line2.Commit();
            }
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
            
            var polyline = app.MemoryGeometryBuilder.WireBuilder.PreCreatePolyline();
            polyline.Points = new Point[]
            {
                new Point(0, 0, 0),
                new Point(0.01, 0.01, 0),
                new Point(0.02, 0, 0),
                new Point(0, 0, 0)
            };
            polyline.Commit();

            var reg = app.MemoryGeometryBuilder.CreatePlanarSheet(
                app.MemoryGeometryBuilder.CreateRegionFromSegments(polyline)).Bodies.First();

            var pathSeg = app.Documents.Active.Selections.Last() as IXSketchSegment;

            var pathCurve = pathSeg.Definition;

            var sweep = app.MemoryGeometryBuilder.SolidBuilder.PreCreateSweep();
            sweep.Profiles = new IXPlanarRegion[] { reg };
            sweep.Path = pathCurve;
            sweep.Commit();

            var body = (sweep.Bodies.First() as ISwBody).Body;

            (app.Documents.Active as ISwPart).Part.CreateFeatureFromBody3(body, false, 0);
        }

        private static void CreateTempGeometry(IXApplication app) 
        {
            var sweepArc = app.MemoryGeometryBuilder.WireBuilder.PreCreateCircle();
            sweepArc.Geometry = new Circle(new Axis(new Point(0, 0, 0), new Vector(0, 0, 1)), 0.01);
            sweepArc.Commit();

            var sweepLine = app.MemoryGeometryBuilder.WireBuilder.PreCreateLine();
            sweepLine.Geometry = new Line(new Point(0, 0, 0), new Point(1, 1, 1));
            sweepLine.Commit();

            var sweep = app.MemoryGeometryBuilder.SolidBuilder.PreCreateSweep();
            sweep.Profiles = new IXPlanarRegion[] { app.MemoryGeometryBuilder.CreatePlanarSheet(
                app.MemoryGeometryBuilder.CreateRegionFromSegments(sweepArc)).Bodies.First() };
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

            var arc = app.MemoryGeometryBuilder.WireBuilder.PreCreateCircle();
            arc.Geometry = new Circle(new Axis(new Point(-0.1, 0, 0), new Vector(0, 0, 1)), 0.01);
            arc.Commit();

            var axis = app.MemoryGeometryBuilder.WireBuilder.PreCreateLine();
            axis.Geometry = new Line(new Point(0, 0, 0), new Point(0, 1, 0));
            axis.Commit();

            var rev = app.MemoryGeometryBuilder.SolidBuilder.PreCreateRevolve();
            rev.Angle = Math.PI * 2;
            rev.Axis = axis;
            rev.Profiles = new IXPlanarRegion[] { app.MemoryGeometryBuilder.CreatePlanarSheet(
                app.MemoryGeometryBuilder.CreateRegionFromSegments(arc)).Bodies.First() };
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
            extr.Profiles = new IXPlanarRegion[] { app.MemoryGeometryBuilder.CreatePlanarSheet(
                app.MemoryGeometryBuilder.CreateRegionFromSegments(polyline)).Bodies.First() };
            extr.Commit();

            body = (extr.Bodies.First() as ISwBody).Body;

            (app.Documents.Active as ISwPart).Part.CreateFeatureFromBody3(body, false, 0);

            var cyl = app.MemoryGeometryBuilder.CreateSolidCylinder(
                new Point(0, 0, 0), new Vector(1, 0, 0), 0.1, 0.2);

            cyl = app.MemoryGeometryBuilder.CreateSurfaceCylinder(
                new Point(0, 0, 0), new Vector(1, 0, 0), 0.1, 0.2);

            body = (cyl.Bodies.First() as ISwBody).Body;

            (app.Documents.Active as ISwPart).Part.CreateFeatureFromBody3(body, false, 0);
        }

        private static void CreateLoftFromSelection(ISwApplication app)
        {
            var part = (ISwPart)app.Documents.Active;

            var faces = part.Selections.OfType<IXPlanarFace>().ToArray();

            var modeler = app.Sw.IGetModeler();

            var curves = new List<ICurve>();

            var firstProfile = faces[0].AdjacentEntities.OfType<ISwEdge>().Select(e => e.Definition.Curves.First().ICopy());
            var secondProfile = faces[1].AdjacentEntities.OfType<ISwEdge>().Select(e => e.Definition.Curves.First().ICopy());

            var c1 = modeler.MergeCurves(firstProfile.Select(c => c.ICopy()).ToArray());
            var c2 = modeler.MergeCurves(secondProfile.Select(c => c.ICopy()).ToArray());

            c1.GetEndParams(out var start, out var end, out var isClosed, out var isPer);
            c2.GetEndParams(out var start1, out var end1, out var isClosed1, out var isPer1);

            var startPt = (double[])c1.Evaluate2(start, 0);
            var endPt = (double[])c1.Evaluate2(end, 0);

            c1 = c1.CreateTrimmedCurve2(startPt[0], startPt[1], startPt[2], endPt[0], endPt[1], endPt[2]);

            startPt = (double[])c2.Evaluate2(start, 0);
            endPt = (double[])c2.Evaluate2(end, 0);

            c2 = c2.CreateTrimmedCurve2(startPt[0], startPt[1], startPt[2], endPt[0], endPt[1], endPt[2]);

            curves.Add(c1.MakeBsplineCurve2());
            curves.Add(c2.MakeBsplineCurve2());

            //curves.AddRange(firstProfile.Select(c => c.MakeBsplineCurve2()));
            //curves.AddRange(secondProfile.Select(c => c.MakeBsplineCurve2()));

            //var vec = (IMathVector)app.Sw.IGetMathUtility().CreateVector(new double[] { 0, 0, 1 });

            var surf = (ISurface)modeler.CreateLoftSurface(curves.ToArray(), false, true, new ICurve[0], 0, 0, null, null, new IFace2[0], new IFace2[0], false, false, null, null, -1, -1, -1, -1);

            curves.Clear();

            //curves.AddRange(firstProfile.Select(c => c.ICopy()));
            curves.Add(c1.ICopy());
            curves.Add(null);
            curves.Add(c2.ICopy());
            //curves.AddRange(secondProfile.Select(c => c.ICopy()));

            var body = (IBody2)surf.CreateTrimmedSheet4(curves.ToArray(), true);

            part.Part.CreateFeatureFromBody3(body, false, (int)swCreateFeatureBodyOpts_e.swCreateFeatureBodySimplify);
        }
    }
}
