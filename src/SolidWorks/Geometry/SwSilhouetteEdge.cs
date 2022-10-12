//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System.Collections.Generic;
using System.Linq;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Curves;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public interface ISwSilhouetteEdge : ISwEntity, IXSilhouetteEdge
    {
        ISilhouetteEdge SilhouetteEdge { get; }
    }

    internal class SwSilhouetteEdge : SwEntity, ISwSilhouetteEdge
    {
        public ISilhouetteEdge SilhouetteEdge { get; }

        internal SwSilhouetteEdge(ISilhouetteEdge silhouetteEdge, SwDocument doc, SwApplication app) : base((IEntity)silhouetteEdge, doc, app)
        {
            SilhouetteEdge = silhouetteEdge;
        }

        public IXFace Face => OwnerDocument.CreateObjectFromDispatch<ISwFace>(SilhouetteEdge.GetFace());

        public IXCurve Definition => OwnerApplication.CreateObjectFromDispatch<SwCurve>(SilhouetteEdge.GetCurve(), OwnerDocument);

        public IXPoint StartPoint => new SwMathPoint(SilhouetteEdge.GetStartPoint(), OwnerDocument, OwnerApplication);

        public IXPoint EndPoint => new SwMathPoint(SilhouetteEdge.GetStartPoint(), OwnerDocument, OwnerApplication);

        public double Length => Definition.Length;

        public override ISwBody Body => (ISwBody)Face.Body;

        public override ISwEntityRepository AdjacentEntities => new EmptySwEntityRepository();

        public override Point FindClosestPoint(Point point)
            => new Point(((double[])SilhouetteEdge.GetCurve().GetClosestPointOn(point.X, point.Y, point.Z)).Take(3).ToArray());
    }

    internal class EmptySwEntityRepository : SwEntityRepository
    {
        protected override IEnumerable<ISwEntity> IterateEntities(bool faces, bool edges, bool vertices, bool silhouetteEdges)
        {
            yield break;
        }
    }
}