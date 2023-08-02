//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
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
    }

    internal class SwFaceAdjacentEntitiesRepository : SwEntityRepository
    {
        private readonly SwFace m_Face;

        internal SwFaceAdjacentEntitiesRepository(SwFace face) 
        {
            m_Face = face;
        }

        protected override IEnumerable<ISwEntity> IterateEntities(bool faces, bool edges, bool vertices, bool silhouetteEdges)
        {
            IEnumerable<IVertex> EnumerateVertices(IEdge edge)
            {
                var startVertex = edge.IGetStartVertex();

                if (startVertex != null)
                {
                    yield return startVertex;
                }

                var endVertex = edge.IGetEndVertex();

                if (endVertex != null)//vertex is null for the closed curves
                {
                    yield return endVertex;
                }
            }

            if (edges)
            {
                foreach (IEdge edge in (m_Face.Face.GetEdges() as object[]).ValueOrEmpty())
                {
                    yield return m_Face.OwnerApplication.CreateObjectFromDispatch<ISwEdge>(edge, m_Face.OwnerDocument);
                }
            }

            if (vertices)
            {
                foreach (var vertex in (m_Face.Face.GetEdges() as object[]).ValueOrEmpty().Cast<IEdge>().SelectMany(EnumerateVertices).Distinct())
                {
                    yield return m_Face.OwnerApplication.CreateObjectFromDispatch<ISwVertex>(vertex, m_Face.OwnerDocument);
                }
            }
        }
    }

    internal abstract class SwFace : SwEntity, ISwFace
    {
        IXSurface IXFace.Definition => Definition;

        public IFace2 Face { get; }
        private readonly IMathUtility m_MathUtils;

        protected SwFace(IFace2 face, SwDocument doc, SwApplication app) : base((IEntity)face, doc, app)
        {
            Face = face;
            m_MathUtils = app.Sw.IGetMathUtility();
            AdjacentEntities = new SwFaceAdjacentEntitiesRepository(this);
        }

        public override ISwBody Body => OwnerApplication.CreateObjectFromDispatch<ISwBody>(Face.GetBody(), OwnerDocument);

        public override ISwEntityRepository AdjacentEntities { get; }

        public double Area => Face.GetArea();

        private IComponent2 GetSwComponent() => (Face as IEntity).GetComponent() as IComponent2;

        public System.Drawing.Color? Color 
        {
            get => SwColorHelper.GetColor(GetSwComponent(), 
                (o, c) => Face.GetMaterialPropertyValues2((int)o, c) as double[]);
            set => SwColorHelper.SetColor(value, GetSwComponent(),
                (m, o, c) => Face.SetMaterialPropertyValues2(m, (int)o, c),
                (o, c) => Face.RemoveMaterialProperty2((int)o, c));
        }

        public ISwSurface Definition => OwnerApplication.CreateObjectFromDispatch<SwSurface>(Face.IGetSurface(), OwnerDocument);

        private IEnumerable<ISwLoop> IterateLoops() 
        {
            var loops = (object[])Face.GetLoops();

            for (int i = 0; i < loops.Length; i++)
            {
                yield return OwnerApplication.CreateObjectFromDispatch<ISwLoop>((ILoop2)loops[i], OwnerDocument);
            }
        }

        public IXFeature Feature 
        {
            get 
            {
                var feat = Face.IGetFeature();

                if (feat != null)
                {
                    return OwnerDocument.CreateObjectFromDispatch<ISwFeature>(feat);
                }
                else 
                {
                    return null;
                }
            }
        }
        
        public bool Sense => Face.FaceInSurfaceSense();

        public IXLoop OuterLoop
        {
            get => IterateLoops().First(l => l.Loop.IsOuter());
            set => throw new NotSupportedException();
        }

        public IXLoop[] InnerLoops
        {
            get => IterateLoops().Where(l => !l.Loop.IsOuter()).ToArray();
            set => throw new NotSupportedException();
        }

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

        public void GetUVBoundary(out double uMin, out double uMax, out double vMin, out double vMax)
        {
            var uvBounds = (double[])Face.GetUVBounds();

            uMin = uvBounds[0];
            uMax = uvBounds[1];
            vMin = uvBounds[2];
            vMax = uvBounds[3];
        }

        public void CalculateUVParameter(Point point, out double uParam, out double vParam)
        {
            var uvParam = (double[])Face.ReverseEvaluate(point.X, point.Y, point.Z);

            if (uvParam != null)
            {
                uParam = uvParam[0];
                vParam = uvParam[1];
            }
            else
            {
                throw new NullReferenceException("Failed to extract UV parameters of the face. This may indicate that input point does not lie on the face");
            }
        }
    }

    public interface ISwPlanarFace : ISwFace, IXPlanarFace, ISwPlanarRegion
    {
        new ISwPlanarSurface Definition { get; }
    }

    internal class SwPlanarFace : SwFace, ISwPlanarFace
    {
        IXPlanarSurface IXPlanarFace.Definition => Definition;
        ISwLoop ISwRegion.OuterLoop { get => (ISwLoop)OuterLoop; set => OuterLoop = value; }
        ISwLoop[] ISwRegion.InnerLoops { get => InnerLoops.Cast<ISwLoop>().ToArray(); set => InnerLoops = value; }

        public SwPlanarFace(IFace2 face, SwDocument doc, SwApplication app) : base(face, doc, app)
        {
        }

        public new ISwPlanarSurface Definition => OwnerApplication.CreateObjectFromDispatch<SwPlanarSurface>(Face.IGetSurface(), OwnerDocument);

        public Plane Plane => Definition.Plane;

        public ISwTempPlanarSheetBody PlanarSheetBody 
        {
            get 
            {
                var sheetBody = Face.CreateSheetBody();
                return OwnerApplication.CreateObjectFromDispatch<SwTempPlanarSheetBody>(sheetBody, OwnerDocument);
            }
        }
    }

    public interface ISwCylindricalFace : ISwFace, IXCylindricalFace
    {
        new ISwCylindricalSurface Definition { get; }
    }

    internal class SwCylindricalFace : SwFace, ISwCylindricalFace
    {
        IXCylindricalSurface IXCylindricalFace.Definition => Definition;

        public SwCylindricalFace(IFace2 face, SwDocument doc, SwApplication app) : base(face, doc, app)
        {
        }

        public new ISwCylindricalSurface Definition => OwnerApplication.CreateObjectFromDispatch<SwCylindricalSurface>(
            Face.IGetSurface(), OwnerDocument);
    }

    public interface ISwBlendXFace : ISwFace, IXBlendXFace
    {
    }

    internal class SwBlendFace : SwFace, ISwBlendXFace
    {
        public SwBlendFace(IFace2 face, SwDocument doc, SwApplication app) : base(face, doc, app)
        {
        }

        IXBlendSurface IXBlendXFace.Definition => (IXBlendSurface)base.Definition;
    }

    public interface ISwBFace : ISwFace, IXBFace
    {
    }

    internal class SwBFace : SwFace, ISwBFace
    {
        public SwBFace(IFace2 face, SwDocument doc, SwApplication app) : base(face, doc, app)
        {
        }

        IXBSurface IXBFace.Definition => (IXBSurface)base.Definition;
    }

    public interface ISwConicalFace : ISwFace, IXConicalFace
    {
    }

    internal class SwConicalFace : SwFace, ISwConicalFace
    {
        public SwConicalFace(IFace2 face, SwDocument doc, SwApplication app) : base(face, doc, app)
        {
        }

        IXConicalSurface IXConicalFace.Definition => (IXConicalSurface)base.Definition;
    }

    public interface ISwExtrudedFace : ISwFace, IXExtrudedFace
    {
    }

    internal class SwExtrudedFace : SwFace, ISwExtrudedFace
    {
        public SwExtrudedFace(IFace2 face, SwDocument doc, SwApplication app) : base(face, doc, app)
        {
        }

        IXExtrudedSurface IXExtrudedFace.Definition => (IXExtrudedSurface)base.Definition;
    }

    public interface ISwOffsetFace : ISwFace, IXOffsetFace
    {
    }

    internal class SwOffsetFace : SwFace, ISwOffsetFace
    {
        public SwOffsetFace(IFace2 face, SwDocument doc, SwApplication app) : base(face, doc, app)
        {
        }

        IXOffsetSurface IXOffsetFace.Definition => (IXOffsetSurface)base.Definition;
    }

    public interface ISwRevolvedFace : ISwFace, IXRevolvedFace
    {
    }

    internal class SwRevolvedFace : SwFace, ISwRevolvedFace
    {
        public SwRevolvedFace(IFace2 face, SwDocument doc, SwApplication app) : base(face, doc, app)
        {
        }

        IXRevolvedSurface IXRevolvedFace.Definition => (IXRevolvedSurface)base.Definition;
    }

    public interface ISwSphericalFace : ISwFace, IXSphericalFace
    {
    }

    internal class SwSphericalFace : SwFace, ISwSphericalFace
    {
        public SwSphericalFace(IFace2 face, SwDocument doc, SwApplication app) : base(face, doc, app)
        {
        }

        IXSphericalSurface IXSphericalFace.Definition => (IXSphericalSurface)base.Definition;
    }

    public interface ISwToroidalFace : ISwFace, IXToroidalFace
    {
    }

    internal class SwToroidalFace : SwFace, ISwToroidalFace
    {
        public SwToroidalFace(IFace2 face, SwDocument doc, SwApplication app) : base(face, doc, app)
        {
        }

        IXToroidalSurface IXToroidalFace.Definition => (IXToroidalSurface)base.Definition;
    }
}
