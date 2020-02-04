//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using Xarial.XCad.Geometry;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public class SwEdge : SwEntity, IXEdge
    {
        public IEdge Edge { get; }

        public override IXBody Body => new SwBody(Edge.GetBody());

        internal SwEdge(IEdge edge) : base(edge as IEntity)
        {
            Edge = edge;
        }
    }
}