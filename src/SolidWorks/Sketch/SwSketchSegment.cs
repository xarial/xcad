//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry.Curves;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.Services;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Sketch
{
    /// <summary>
    /// SOLIDWORKS specific sketch segment
    /// </summary>
    public interface ISwSketchSegment : IXSketchSegment, ISwSelObject, ISwSketchEntity
    {
        /// <summary>
        /// Pointer to sketch segment
        /// </summary>
        ISketchSegment Segment { get; }

        /// <summary>
        /// Pointer to the underlying curve
        /// </summary>
        new ISwCurve Definition { get; }
    }

    internal abstract class SwSketchSegment : SwSketchEntity, ISwSketchSegment
    {
        IXCurve IXSketchSegment.Definition => Definition;
        IXPoint IXSegment.StartPoint => StartPoint;
        IXPoint IXSegment.EndPoint => EndPoint;

        protected readonly IElementCreator<ISketchSegment> m_Creator;

        protected readonly ISketchManager m_SketchMgr;

        public ISketchSegment Segment => m_Creator.Element;

        public override IXIdentifier Id => new XIdentifier((int[])Segment.GetID());

        public override bool IsCommitted => m_Creator.IsCreated;

        public override object Dispatch => Segment;

        public override bool IsAlive => this.CheckIsAlive(() => Segment.GetID());

        private readonly IMathUtility m_MathUtils;

        private SwSketchBase m_OwnerSketch;

        protected SwSketchSegment(ISketchSegment seg, SwDocument doc, SwApplication app, bool created) : base(seg, doc, app)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            m_SketchMgr = doc.Model.SketchManager;
            m_Creator = new ElementCreator<ISketchSegment>(CreateEntity, seg, created);

            m_MathUtils = app.Sw.IGetMathUtility();

            if (seg != null)
            {
                SetOwnerSketch(seg);
            }
        }

        protected SwSketchSegment(SwSketchBase ownerSketch, SwDocument doc, SwApplication app) : this(null, doc, app, false)
        {
            m_OwnerSketch = ownerSketch;
        }

        public override void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);

        public override IXLayer Layer
        {
            get => SwLayerHelper.GetLayer(this, x => x.Segment.Layer);
            set => SwLayerHelper.SetLayer(this, value, (x, y) => x.Segment.Layer = y);
        }
        
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

                var blockTransform = OwnerBlock?.GetTotalTransform();

                if (blockTransform != null)
                {
                    startPt = startPt.Transform(blockTransform);
                    endPt = endPt.Transform(blockTransform);
                }
                
                if (AssignedOwnerBlock != null) 
                {
                    //NOTE: if block is assigned and this sketch entity is extracted form the definition block, it is required transform the curve to the sketch block instance space
                    curve.ApplyTransform((MathTransform)m_MathUtils.ToMathTransform(blockTransform));
                }

                curve = curve.CreateTrimmedCurve2(startPt.X, startPt.Y, startPt.Z, 
                    endPt.X, endPt.Y, endPt.Z);

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

        public override IXSketchBase OwnerSketch => m_OwnerSketch;

        internal override void Select(bool append, ISelectData selData)
        {
            if (!Segment.Select4(append, (SelectData)selData)) 
            {
                throw new Exception("Failed to select sketch segment");
            }
        }

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
            using (var editor = !m_OwnerSketch.IsEditing ? m_OwnerSketch?.Edit() : null)
            {
                //NOTE: this entity can be created even if the IsCommited set to false as these are the cached entities created
                var seg = CreateSketchEntity();

                if (seg != null)
                {
                    SetColor(seg, m_Creator.CachedProperties.Get<Color?>(nameof(Color)));

                    SetOwnerSketch(seg);

                    return seg;
                }
                else 
                {
                    throw new Exception("Failed to create sketch segment");
                }
            }
        }

        protected virtual ISketchSegment CreateSketchEntity() 
            => throw new NotSupportedException();

        protected override string GetFullName() => this.Segment.GetName();

        private void SetOwnerSketch(ISketchSegment seg) 
        {
            m_OwnerSketch = OwnerDocument.CreateObjectFromDispatch<SwSketchBase>(seg.GetSketch());
        }
    }
}
