using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public class SwFace : SwEntity, IXFace
    {
        public IFace2 Face { get; }

        public SwFace(IFace2 face) : base(face as IEntity)
        {
            Face = face;
        }

        public override SwBody Body => (SwBody)FromDispatch(Face.GetBody());

        public override IEnumerable<SwEntity> AdjacentEntities 
        {
            get 
            {
                foreach (IEdge edge in (Face.GetEdges() as object[]).ValueOrEmpty())
                {
                    yield return (SwEdge)FromDispatch(edge);
                }
            }
        }

        public double Area => Face.GetArea();
    }

    public class SwPlanarFace : SwFace, IXPlanarFace
    {
        public SwPlanarFace(IFace2 face) : base(face)
        {
        }

        public Vector Normal => new Vector(Face.Normal as double[]);
    }

    public class SwCylindricalFace : SwFace, IXCylindricalFace
    {
        public SwCylindricalFace(IFace2 face) : base(face)
        {
        }

        public Point Origin
        {
            get
            {
                var cylParams = CylinderParams;

                return new Point(cylParams[0], cylParams[1], cylParams[2]);
            }
        }

        public Vector Axis 
        {
            get 
            {
                var cylParams = CylinderParams;

                return new Vector(cylParams[3], cylParams[4], cylParams[5]);
            }
        }

        public double Radius => CylinderParams[6];

        private double[] CylinderParams
        {
            get
            {
                return Face.IGetSurface().CylinderParams as double[];
            }
        }
    }
}
