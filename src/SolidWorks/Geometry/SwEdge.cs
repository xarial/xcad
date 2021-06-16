//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System.Collections.Generic;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public interface ISwEdge : ISwEntity, IXEdge 
    {
        IEdge Edge { get; }
        new ISwCurve Definition { get; }
    }

    internal class SwEdge : SwEntity, ISwEdge
    {
        IXSegment IXEdge.Definition => Definition;

        public IEdge Edge { get; }

        public override ISwBody Body => FromDispatch<SwBody>(Edge.GetBody());

        public override IEnumerable<ISwEntity> AdjacentEntities 
        {
            get 
            {
                foreach (IFace2 face in (Edge.GetTwoAdjacentFaces2() as object[]).ValueOrEmpty()) 
                {
                    yield return FromDispatch<SwFace>(face);
                }

                foreach (ICoEdge coEdge in (Edge.GetCoEdges() as ICoEdge[]).ValueOrEmpty())
                {
                    var edge = coEdge.GetEdge() as IEdge;
                    yield return FromDispatch<SwEdge>(edge);
                }

                //TODO: implement vertices
            }
        }

        public ISwCurve Definition => FromDispatch<SwCurve>(Edge.IGetCurve());

        internal SwEdge(IEdge edge, ISwDocument doc) : base((IEntity)edge, doc)
        {
            Edge = edge;
        }
    }

    public interface ISwCircularEdge : ISwEdge, IXCircularEdge
    {
        new ISwArcCurve Definition { get; }
    }

    internal class SwCircularEdge : SwEdge, ISwCircularEdge
    {
        IXArc IXCircularEdge.Definition => Definition;

        internal SwCircularEdge(IEdge edge, ISwDocument doc) : base(edge, doc)
        {
        }

        public new ISwArcCurve Definition => SwSelObject.FromDispatch<SwArcCurve>(this.Edge.IGetCurve());
    }

    public interface ISwLinearEdge : ISwEdge, IXLinearEdge
    {
        new ISwLineCurve Definition { get; }
    }

    internal class SwLinearEdge : SwEdge, ISwLinearEdge
    {
        IXLine IXLinearEdge.Definition => Definition;

        internal SwLinearEdge(IEdge edge, ISwDocument doc) : base(edge, doc)
        {
        }

        public new ISwLineCurve Definition => SwSelObject.FromDispatch<SwLineCurve>(this.Edge.IGetCurve());
    }
}