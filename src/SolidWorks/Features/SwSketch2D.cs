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
using Xarial.XCad.Features;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Sketch;

namespace Xarial.XCad.SolidWorks.Features
{
    public interface ISwSketch2D : ISwSketchBase, IXSketch2D
    {
        new IEnumerable<ISwSketchRegion> Regions { get; }
    }

    internal class SwSketch2D : SwSketchBase, ISwSketch2D
    {
        IEnumerable<IXSketchRegion> IXSketch2D.Regions => Regions;

        internal SwSketch2D(IFeature feat, ISwDocument doc, ISwApplication app, bool created) 
            : base(feat, doc, app, created)
        {
            if (doc == null) 
            {
                throw new ArgumentNullException(nameof(doc));
            }
        }

        public IEnumerable<ISwSketchRegion> Regions 
        {
            get
            {
                var regs = Sketch.GetSketchRegions() as object[];
                
                if (regs?.Any() == true)
                {
                    foreach (ISketchRegion reg in regs) 
                    {
                        yield return OwnerDocument.CreateObjectFromDispatch<ISwSketchRegion>(reg);
                    }
                }
            }
        }
        
        public Plane Plane 
        {
            get
            {
                var mathUtils = OwnerApplication.Sw.IGetMathUtility();

                var transform = Sketch.ModelToSketchTransform.IInverse();

                var x = (IMathVector)mathUtils.CreateVector(new double[] { 1, 0, 0 });
                var z = (IMathVector)mathUtils.CreateVector(new double[] { 0, 0, 1 });
                var origin = (IMathPoint)mathUtils.CreatePoint(new double[] { 0, 0, 0 });

                x = (IMathVector)x.MultiplyTransform(transform);
                z = (IMathVector)z.MultiplyTransform(transform);
                origin = (IMathPoint)origin.MultiplyTransform(transform);

                return new Plane(new Point((double[])origin.ArrayData),
                    new Vector((double[])z.ArrayData),
                    new Vector((double[])x.ArrayData));
            }
        }

        protected override ISketch CreateSketch()
        {
            //TODO: select the plane or face
            OwnerModelDoc.InsertSketch2(true);
            return OwnerModelDoc.SketchManager.ActiveSketch;
        }

        protected override void ToggleEditSketch()
            => OwnerModelDoc.InsertSketch2(true);
    }
}