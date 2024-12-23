//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Threading;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry.Structures;
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
    /// SOLIDWORKS specific sketch point
    /// </summary>
    public interface ISwSketchPoint : IXSketchPoint, ISwSketchEntity, ISwPoint
    {
        /// <summary>
        /// Pointer to sketch point
        /// </summary>
        ISketchPoint Point { get; }
    }

    internal class SwSketchPoint : SwSketchEntity, ISwSketchPoint
    {
        protected readonly IElementCreator<ISketchPoint> m_Creator;

        protected readonly ISketchManager m_SketchMgr;
        
        public override bool IsCommitted => m_Creator.IsCreated;

        public ISketchPoint Point => m_Creator.Element;

        public override IXIdentifier Id => new XIdentifier((int[])Point.GetID());

        public override bool IsAlive => this.CheckIsAlive(() => Point.GetID());

        public override object Dispatch => Point;

        public override IXSketchBase OwnerSketch => m_OwnerSketch;

        private SwSketchBase m_OwnerSketch;

        internal SwSketchPoint(ISketchPoint pt, SwDocument doc, SwApplication app, bool created) : base(pt, doc, app)
        {
            m_SketchMgr = doc.Model.SketchManager;

            m_Creator = new ElementCreator<ISketchPoint>(CreatePoint, pt, created);

            if (pt != null) 
            {
                SetOwnerSketch(pt);
            }
        }

        internal SwSketchPoint(SwSketchBase ownerSketch, SwDocument doc, SwApplication app) : this(null, doc, app, false)
        {
            m_OwnerSketch = ownerSketch;
        }

        public override void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);

        public override IXLayer Layer
        {
            get => SwLayerHelper.GetLayer(this, x => x.Point.Layer);
            set => SwLayerHelper.SetLayer(this, value, (x, y) => x.Point.Layer = y);
        }

        public override System.Drawing.Color? Color
        {
            get
            {
                if (IsCommitted)
                {
                    return GetColor();
                }
                else
                {
                    return m_Creator.CachedProperties.Get<System.Drawing.Color?>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    SetColor(Point, value);
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public Point Coordinate
        {
            get
            {
                if (m_Creator.IsCreated)
                {
                    return new Point(Point.X, Point.Y, Point.Z);
                }
                else
                {
                    return m_Creator.CachedProperties.Get<Point>();
                }
            }
            set
            {
                if (m_Creator.IsCreated)
                {
                    if (m_SketchMgr.ActiveSketch != Point.GetSketch())
                    {
                        throw new Exception("You must set the sketch into editing mode in order to modify the cooridinate");
                    }

                    Point.SetCoords(value.X, value.Y, value.Z);
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        internal override void Select(bool append, ISelectData selData)
        {
            if (!Point.Select4(append, (SelectData)selData))
            {
                throw new Exception("Failed to select sketch point");
            }
        }

        private void SetColor(ISketchPoint pt, System.Drawing.Color? color)
        {
            int colorRef = 0;

            if (color.HasValue)
            {
                colorRef = ColorUtils.ToColorRef(color.Value);
            }

            pt.Color = colorRef;
        }

        private System.Drawing.Color? GetColor() => ColorUtils.FromColorRef(Point.Color);

        private ISketchPoint CreatePoint(CancellationToken cancellationToken)
        {
            using (var editor = !m_OwnerSketch.IsEditing ? m_OwnerSketch.Edit() : null)
            {
                var pt = m_SketchMgr.CreatePoint(Coordinate.X, Coordinate.Y, Coordinate.Z);

                SetColor(pt, m_Creator.CachedProperties.Get<System.Drawing.Color?>(nameof(Color)));

                SetOwnerSketch(pt);

                return pt;
            }
        }

        protected override string GetFullName() 
        {
            if (OwnerApplication.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2015)) 
            {
                if (OwnerModelDoc.ISelectionManager.GetSelectByIdSpecification(Point, out string name, out _, out _))
                {
                    return name;
                }
                else 
                {
                    throw new Exception("Failed to get the selection specification of the point");
                }
            }
            else 
            {
                throw new NotSupportedException("Point name extraction is supported in SOLIDWORKS 2015 or newer");
            }
        }

        private void SetOwnerSketch(ISketchPoint pt)
        {
            m_OwnerSketch = OwnerDocument.CreateObjectFromDispatch<SwSketchBase>(pt.GetSketch());
        }
    }
}