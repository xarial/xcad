//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry.Curves;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.Services;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Sketch
{
    public interface ISwSketchSegment : IXSketchSegment, ISwSelObject
    {
        ISketchSegment Segment { get; }
        new ISwCurve Definition { get; }
    }

    internal abstract class SwSketchSegment : SwSketchEntity, ISwSketchSegment
    {
        IXCurve IXSketchSegment.Definition => Definition;
        IXPoint IXSegment.StartPoint => StartPoint;
        IXPoint IXSegment.EndPoint => EndPoint;

        protected readonly ElementCreator<ISketchSegment> m_Creator;

        protected readonly ISketchManager m_SketchMgr;

        public ISketchSegment Segment => m_Creator.Element;

        public override bool IsCommitted => m_Creator.IsCreated;

        public override object Dispatch => Segment;

        protected SwSketchSegment(ISketchSegment seg, ISwDocument doc, ISwApplication app, bool created) : base(seg, doc, app)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            m_SketchMgr = doc.Model.SketchManager;
            m_Creator = new ElementCreator<ISketchSegment>(CreateEntity, seg, created);
        }

        public override void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);
        
        public override Color? Color
        {
            get
            {
                if (IsCommitted)
                {
                    return GetColor();
                }
                else
                {
                    return m_Creator.CachedProperties.Get<Color?>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    SetColor(Segment, value);
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public ISwCurve Definition 
        {
            get 
            {
                var curve = Segment.IGetCurve();
                var startPt = StartPoint.Coordinate;
                var endPt = EndPoint.Coordinate;

                curve = curve.CreateTrimmedCurve2(startPt.X, startPt.Y, startPt.Z, 
                    endPt.X, endPt.Y, endPt.Z) as Curve;

                if (curve == null) 
                {
                    throw new NullReferenceException("Failed to trim curve");
                }

                var transform = Segment.GetSketch().ModelToSketchTransform.IInverse();

                curve.ApplyTransform(transform);

                return OwnerDocument.CreateObjectFromDispatch<SwCurve>(curve);
            }
        }

        public double Length => Definition.Length;

        public abstract IXSketchPoint StartPoint { get; }
        public abstract IXSketchPoint EndPoint { get; }

        public bool IsConstruction => Segment.ConstructionGeometry;

        public override IXSketchBase OwnerSketch => OwnerDocument.CreateObjectFromDispatch<ISwSketchBase>(Segment.GetSketch());

        private void SetColor(ISketchSegment seg, Color? color)
        {
            int colorRef = 0;

            if (color.HasValue)
            {
                colorRef = ColorUtils.ToColorRef(color.Value);
            }

            seg.Color = colorRef;
        }

        private Color? GetColor() => ColorUtils.FromColorRef(Segment.Color);

        private ISketchSegment CreateEntity(CancellationToken cancellationToken)
        {
            var ent = CreateSketchEntity();

            SetColor(ent, m_Creator.CachedProperties.Get<Color?>(nameof(Color)));

            return ent;
        }

        protected virtual ISketchSegment CreateSketchEntity() 
        {
            throw new NotSupportedException();
        }
    }
}
