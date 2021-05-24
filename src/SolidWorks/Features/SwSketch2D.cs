//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using Xarial.XCad.Features;
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

        internal SwSketch2D(ISwDocument doc, IFeature feat, bool created) : base(doc, feat, created)
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
                        yield return SwObject.FromDispatch<ISwSketchRegion>(reg);
                    }
                }
            }
        }

        protected override ISketch CreateSketch()
        {
            //TODO: select the plane or face
            ModelDoc.InsertSketch2(true);
            return ModelDoc.SketchManager.ActiveSketch;
        }

        protected override void ToggleEditSketch()
        {
            ModelDoc.InsertSketch2(true);
        }
    }
}