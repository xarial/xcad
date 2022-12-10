//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Sketch
{
    public interface ISwSketchRegion : IXSketchRegion, ISwPlanarRegion, ISwSelObject
    {
        ISketchRegion Region { get; }
    }

    internal class SwSketchRegion : SwSelObject, ISwSketchRegion
    {
        IXLoop IXRegion.OuterLoop { get => OuterLoop; set => OuterLoop = (ISwLoop)value; }
        IXLoop[] IXRegion.InnerLoops { get => InnerLoops; set => InnerLoops = value.Cast<ISwLoop>().ToArray(); }

        internal SwSketchRegion(ISketchRegion region, SwDocument doc, SwApplication app) : base(region, doc, app)
        {
            Region = region;
        }

        public ISketchRegion Region { get; }

        public Plane Plane
        {
            get
            {
                var transform = Region.Sketch.ModelToSketchTransform.IInverse().ToTransformMatrix();

                var x = new Vector(1, 0, 0).Transform(transform);
                var z = new Vector(0, 0, 1).Transform(transform);
                var origin = new Point(0, 0, 0).Transform(transform);

                return new Plane(origin, z, x);
            }
        }

        public ISwTempPlanarSheetBody PlanarSheetBody 
        {
            get 
            {
                var face = Region.GetFirstLoop().IGetFace();
                var sheetBody = face.CreateSheetBody();
                return OwnerApplication.CreateObjectFromDispatch<SwTempPlanarSheetBody>(sheetBody, OwnerDocument);
            }
        }

        public ISwLoop OuterLoop 
        {
            get => IterateLoops().First(l => l.Loop.IsOuter());
            set => throw new NotSupportedException();
        }

        public ISwLoop[] InnerLoops 
        {
            get => IterateLoops().Where(l => !l.Loop.IsOuter()).ToArray();
            set => throw new NotSupportedException();
        }

        private IEnumerable<ISwLoop> IterateLoops() 
        {
            var loop = Region.GetFirstLoop();

            while (loop != null)
            {
                yield return OwnerApplication.CreateObjectFromDispatch<ISwLoop>(loop, OwnerDocument);

                loop = loop.IGetNext();
            }
        }
    }
}
