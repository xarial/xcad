//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System.Collections.Generic;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public class SwEdge : SwEntity, IXEdge
    {
        IXSegment IXEdge.Definition => Definition;

        public IEdge Edge { get; }

        public override SwBody Body => FromDispatch<SwBody>(Edge.GetBody());

        public override IEnumerable<SwEntity> AdjacentEntities 
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

        public SwCurve Definition => FromDispatch<SwCurve>(Edge.IGetCurve());

        internal SwEdge(IEdge edge) : base(edge as IEntity)
        {
            Edge = edge;
        }
    }

    public class SwCircularEdge : SwEdge, IXCircularEdge
    {
        internal SwCircularEdge(IEdge edge) : base(edge)
        {
        }

        public Point Center 
        {
            get 
            {
                var circParams = CircleParams;

                return new Point(circParams[0], circParams[1], circParams[2]);
            }
        }

        public Vector Axis 
        {
            get
            {
                var circParams = CircleParams;

                return new Vector(circParams[3], circParams[4], circParams[5]);
            }
        }

        public double Radius => CircleParams[6];

        private double[] CircleParams
        {
            get
            {
                return Edge.IGetCurve().CircleParams as double[];
            }
        }
    }

    public class SwLinearEdge : SwEdge, IXLinearEdge
    {
        internal SwLinearEdge(IEdge edge) : base(edge)
        {
        }

        public Point RootPoint
        {
            get
            {
                var lineParams = LineParams;

                return new Point(lineParams[0], lineParams[1], lineParams[2]);
            }
        }

        public Vector Direction
        {
            get
            {
                var lineParams = LineParams;

                return new Vector(lineParams[3], lineParams[4], lineParams[5]);
            }
        }

        private double[] LineParams
        {
            get
            {
                //TODO: use curve
                return (double[])Edge.IGetCurve().LineParams;
            }
        }
    }
}