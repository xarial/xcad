﻿//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.Sketch;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Features
{
    public interface ISwSketch2D : ISwSketchBase, IXSketch2D
    {
        new IEnumerable<ISwSketchRegion> Regions { get; }
    }

    internal class SwSketch2DEditor : SwSketchEditorBase<SwSketch2D>
    {
        public SwSketch2DEditor(SwSketch2D sketch, ISketch swSketch) : base(sketch, swSketch)
        {
        }

        protected override void StartEdit() => Target.OwnerDocument.Model.SketchManager.InsertSketch(true);
        protected override void EndEdit(bool cancel) => Target.OwnerDocument.Model.SketchManager.InsertSketch(!cancel);
    }

    internal class SwSketch2D : SwSketchBase, ISwSketch2D
    {
        internal const string TypeName = "ProfileFeature";

        IEnumerable<IXSketchRegion> IXSketch2D.Regions => Regions;

        internal SwSketch2D(IFeature feat, SwDocument doc, SwApplication app, bool created) 
            : base(feat, doc, app, created)
        {
        }

        internal SwSketch2D(ISketch sketch, SwDocument doc, SwApplication app, bool created)
            : base(sketch, doc, app, created)
        {
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
                var transform = Sketch.ModelToSketchTransform.IInverse().ToTransformMatrix();

                var x = new Vector(1, 0, 0).Transform(transform);
                var z = new Vector(0, 0, 1).Transform(transform);
                var origin = new Point(0, 0, 0).Transform(transform);
                
                return new Plane(origin, z, x);
            }
        }

        public IXPlanarRegion ReferenceEntity 
        {
            get 
            {
                if (IsCommitted)
                {
                    int entType = -1;
                    return (IXPlanarRegion)OwnerDocument.CreateObjectFromDispatch<ISwEntity>(Sketch.GetReferenceEntity(ref entType));
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<IXPlanarRegion>();
                }
            }
            set 
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else 
                {
                    throw new CommitedElementReadOnlyParameterException();
                }
            }
        }

        protected override ISketch CreateSketch()
        {
            var ent = (ISwEntity)ReferenceEntity;

            if (ent == null) 
            {
                throw new Exception("Reference entity is not specified");
            }

            ent.Select(false);

            OwnerModelDoc.InsertSketch2(true);
            
            return OwnerModelDoc.SketchManager.ActiveSketch;
        }

        protected override void CloseSketch()
        {
            OwnerModelDoc.ClearSelection2(true);
            OwnerModelDoc.InsertSketch2(true);
        }

        protected internal override IEditor<IXSketchBase> CreateSketchEditor(ISketch sketch) => new SwSketch2DEditor(this, sketch);
    }
}