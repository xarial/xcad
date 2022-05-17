//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Threading;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Services;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Sketch
{
    public interface ISwSketchPoint : IXSketchPoint, ISwSketchEntity
    {
        ISketchPoint Point { get; }
    }

    internal class SwSketchPoint : SwSketchEntity, ISwSketchPoint
    {
        protected readonly IElementCreator<ISketchPoint> m_Creator;

        protected readonly ISketchManager m_SketchMgr;
        
        public override bool IsCommitted => m_Creator.IsCreated;

        public ISketchPoint Point => m_Creator.Element;

        public override object Dispatch => Point;

        public override IXSketchBase OwnerSketch => OwnerDocument.CreateObjectFromDispatch<ISwSketchBase>(Point.GetSketch());

        internal SwSketchPoint(ISketchPoint pt, ISwDocument doc, ISwApplication app, bool created) : base(pt, doc, app)
        {
            m_SketchMgr = doc.Model.SketchManager;
            m_Creator = new ElementCreator<ISketchPoint>(CreatePoint, pt, created);
        }

        internal SwSketchPoint(SwSketchBase ownerSketch, ISwDocument doc, ISwApplication app) : base(ownerSketch, doc, app)
        {
        }

        public override void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);

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
            var pt = m_SketchMgr.CreatePoint(Coordinate.X, Coordinate.Y, Coordinate.Z);

            SetColor(pt, m_Creator.CachedProperties.Get<System.Drawing.Color?>(nameof(Color)));

            return pt;
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
    }
}