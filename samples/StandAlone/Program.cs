using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks;

namespace StandAlone
{
    class Program
    {
        static void Main(string[] args)
        {
            var sw = Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application")) as ISldWorks;
            sw.Visible = true;

            var app = SwApplication.FromPointer(sw);

            //CreateSketchEntities(app);

            TraverseSelectedFaces(app);
        }

        private static void CreateSketchEntities(IXApplication app)
        {
            var sketch3D = app.Documents.Active.Features.PreCreate3DSketch();
            var line = sketch3D.Entities.PreCreateLine();
            line.StartPoint.Coordinate = new Point(0.1, 0.1, 0.1);
            line.EndPoint.Coordinate = new Point(0.2, 0.2, 0.2);
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
