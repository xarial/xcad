//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Surfaces;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.SolidWorks.Geometry.Surfaces;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public interface ISwFace : ISwEntity, IXFace 
    {
        IFace2 Face { get; }
        new ISwSurface Definition { get; }
        new ISwEdge[] Edges { get; }
    }

    internal class SwFace : SwEntity, ISwFace
    {
        IXSurface IXFace.Definition => Definition;
        IXEdge[] IXFace.Edges => Edges;

        public IFace2 Face { get; }
        private readonly IMathUtility m_MathUtils;

        internal SwFace(IFace2 face, ISwDocument doc, ISwApplication app) : base((IEntity)face, doc, app)
        {
            Face = face;
            m_MathUtils = app.Sw.IGetMathUtility();
        }

        public override ISwBody Body => Application.CreateObjectFromDispatch<ISwBody>(Face.GetBody(), Document);

        public override IEnumerable<ISwEntity> AdjacentEntities 
        {
            get 
            {
                foreach (IEdge edge in (Face.GetEdges() as object[]).ValueOrEmpty())
                {
                    yield return Application.CreateObjectFromDispatch<ISwEdge>(edge, Document);
                }
            }
        }

        public double Area => Face.GetArea();

        private IComponent2 Component => (Face as IEntity).GetComponent() as IComponent2;

        public System.Drawing.Color? Color 
        {
            get => SwColorHelper.GetColor(Face, Component, 
                (o, c) => Face.GetMaterialPropertyValues2((int)o, c) as double[]);
            set => SwColorHelper.SetColor(Face, value, Component,
                (m, o, c) => Face.SetMaterialPropertyValues2(m, (int)o, c),
                (o, c) => Face.RemoveMaterialProperty2((int)o, c));
        }

        public ISwSurface Definition => Application.CreateObjectFromDispatch<SwSurface>(Face.IGetSurface(), Document);

        public IXFeature Feature 
        {
            get 
            {
                var feat = Face.IGetFeature();

                if (feat != null)
                {
                    return Application.CreateObjectFromDispatch<ISwFeature>(feat, Document);
                }
                else 
                {
                    return null;
                }
            }
        }

        public ISwEdge[] Edges => (Face.GetEdges() as object[])
            .Select(f => Application.CreateObjectFromDispatch<ISwEdge>(f, Document)).ToArray();

        public override Point FindClosestPoint(Point point)
            => new Point(((double[])Face.GetClosestPointOn(point.X, point.Y, point.Z)).Take(3).ToArray());

        public bool TryProjectPoint(Point point, Vector direction, out Point projectedPoint)
        {
            var dirVec = (MathVector)m_MathUtils.CreateVector(direction.ToArray());
            var startPt = (MathPoint)m_MathUtils.CreatePoint(point.ToArray());

            var resPt = Face.GetProjectedPointOn(startPt, dirVec);

            if (resPt != null)
            {
                projectedPoint = new Point((double[])resPt.ArrayData);
                return true;
            }
            else
            {
                projectedPoint = null;
                return false;
            }
        }
    }

    public interface ISwPlanarFace : ISwFace, IXPlanarFace, ISwRegion
    {
        new ISwPlanarSurface Definition { get; }
    }

    internal class SwPlanarFace : SwFace, ISwPlanarFace
    {
        IXSegment[] IXRegion.Boundary => Boundary;
        IXPlanarSurface IXPlanarFace.Definition => Definition;

        public SwPlanarFace(IFace2 face, ISwDocument doc, ISwApplication app) : base(face, doc, app)
        {
        }

        public new ISwPlanarSurface Definition => Application.CreateObjectFromDispatch<SwPlanarSurface>(Face.IGetSurface(), Document);

        public Plane Plane => Definition.Plane;

        public ISwCurve[] Boundary => Edges.Select(e => e.Definition).ToArray();
    }

    public interface ISwCylindricalFace : ISwFace, IXCylindricalFace
    {
        new ISwCylindricalSurface Definition { get; }
    }

    internal class SwCylindricalFace : SwFace, ISwCylindricalFace
    {
        IXCylindricalSurface IXCylindricalFace.Definition => Definition;

        public SwCylindricalFace(IFace2 face, ISwDocument doc, ISwApplication app) : base(face, doc, app)
        {
        }

        public new ISwCylindricalSurface Definition => Application.CreateObjectFromDispatch<SwCylindricalSurface>(Face.IGetSurface(), Document);
    }
}
