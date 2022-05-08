//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public interface ISwVertex : ISwEntity, IXVertex
    {
        IVertex Vertex { get; }
    }

    [DebuggerDisplay("{" + nameof(Coordinate) + "}")]
    internal class SwVertex : SwEntity, ISwVertex
    {
        public IVertex Vertex { get; }

        public Point Coordinate
        {
            get => new Point((double[])Vertex.GetPoint());
            set => throw new NotSupportedException("Coordinate of the vertex cannot be changed");
        }

        public override ISwBody Body => OwnerApplication.CreateObjectFromDispatch<ISwBody>(
            ((Vertex.GetEdges() as object[]).First() as IEdge).GetBody(), OwnerDocument);

        public override IEnumerable<ISwEntity> AdjacentEntities
        {
            get
            {
                foreach (IEdge edge in (Vertex.GetEdges() as object[]).ValueOrEmpty())
                {
                    yield return OwnerApplication.CreateObjectFromDispatch<SwEdge>(edge, OwnerDocument);
                }

                foreach (IFace2 face in (Vertex.GetAdjacentFaces() as object[]).ValueOrEmpty())
                {
                    yield return OwnerApplication.CreateObjectFromDispatch<SwFace>(face, OwnerDocument);
                }
            }
        }

        public override Point FindClosestPoint(Point point)
            => new Point(((double[])Vertex.GetClosestPointOn(point.X, point.Y, point.Z)).Take(3).ToArray());

        internal SwVertex(IVertex vertex, ISwDocument doc, ISwApplication app) : base((IEntity)vertex, doc, app)
        {
            Vertex = vertex;
        }
    }
}
