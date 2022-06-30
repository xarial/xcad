using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Exceptions;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.Toolkit.Exceptions;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public interface ISwRayIntersection : IXRayIntersection
    {
    }

    public interface ISwAssemblyRayIntersection : ISwRayIntersection, IXAssemblyRayIntersection
    {
    }

    public interface ISwRay : IXRay
    {
    }

    internal class SwRay : ISwRay
    {
        public Axis Axis { get; }

        public bool IsCommitted { get; private set; }

        public RayHitResult[] Hits
        {
            get
            {
                if (IsCommitted)
                {
                    return m_Hits.ToArray();
                }
                else
                {
                    throw new NonCommittedElementAccessException();
                }
            }
        }

        private readonly List<RayHitResult> m_Hits;

        public SwRay(Axis axis) 
        {
            Axis = axis;
            IsCommitted = false;
            m_Hits = new List<RayHitResult>();
        }

        public void Commit(CancellationToken cancellationToken)
        {
            IsCommitted = true;
        }

        internal void RegisterHit(Point point, Vector normal, IXBody body, IXFace face, RayIntersectionType_e type) 
        {
            m_Hits.Add(new RayHitResult(point, normal, body, face, type));
            IsCommitted = true;
        }
    }

    internal abstract class SwRayIntersection : ISwRayIntersection
    {
        //TODO: implement relative to matrix
        public TransformMatrix RelativeTo { get => TransformMatrix.Identity; set => throw new NotImplementedException(); }
        
        //TODO: implement handling of user units
        public bool UserUnits { get => false; set => throw new NotImplementedException(); }

        public bool VisibleOnly
        {
            get => m_Creator.CachedProperties.Get<bool>();
            set
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else
                {
                    throw new CommittedElementPropertyChangeNotSupported();
                }
            }
        }

        public virtual IXBody[] Scope
        {
            get => m_Creator.CachedProperties.Get<IXBody[]>();
            set
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else 
                {
                    throw new CommittedElementPropertyChangeNotSupported();
                }
            }
        }

        public bool IsCommitted => m_Creator.IsCreated;

        public IXRay[] Rays => m_RaysList.ToArray();

        public bool Precise
        {
            get => m_Creator.CachedProperties.Get<bool>();
            set
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else
                {
                    throw new CommittedElementPropertyChangeNotSupported();
                }
            }
        }

        private List<SwRay> m_RaysList;

        public void Commit(CancellationToken cancellationToken)
            => m_Creator.Create(cancellationToken);

        private IReadOnlyList<IXRay> CreateRayIntersections(CancellationToken cancellationToken) 
        {
            var bodies = Scope;

            if(bodies?.Any() != true) 
            {
                bodies = GetAllBodies();
            }

            //assembly bodies must be transformed to the assembly space
            bodies = bodies.Select(b => 
            {
                var comp = b.Component;

                if (comp != null)
                {
                    var copy = b.Copy();
                    copy.Transform(comp.Transformation);
                    return copy;
                }
                else 
                {
                    return b;
                }
            }).ToArray();
            
            var intersectCount = m_Doc.Model.Extension.RayIntersections(bodies.Cast<ISwBody>().Select(b => b.Body).ToArray(),
                m_RaysList.SelectMany(r => r.Axis.Point.ToArray()).ToArray(),
                m_RaysList.SelectMany(r => r.Axis.Direction.ToArray()).ToArray(),
                (int)(swRayPtsOpts_e.swRayPtsOptsTOPOLS | swRayPtsOpts_e.swRayPtsOptsNORMALS | swRayPtsOpts_e.swRayPtsOptsENTRY_EXIT),
                0.00001, 0, Precise);

            var topo = m_Doc.Model.GetRayIntersectionsTopology() as object[];
            var topoData = m_Doc.Model.GetRayIntersectionsPoints() as double[];

            var entsHits = new Dictionary<int, IFace2>();
            var ptsHits = new Dictionary<int, Point>();

            var hitRayIndices = new List<int>();

            for (int i = 0; i < intersectCount; i++)
            {
                var hitType = (swRayPtsResults_e)topoData[i * 9 + 2];

                if (hitType.HasFlag(swRayPtsResults_e.swRayPtsResultsFACE))
                {
                    RayIntersectionType_e rayType;

                    if (hitType.HasFlag(swRayPtsResults_e.swRayPtsResultsENTER))
                    {
                        rayType = RayIntersectionType_e.Enter;
                    }
                    else if (hitType.HasFlag(swRayPtsResults_e.swRayPtsResultsEXIT))
                    {
                        rayType = RayIntersectionType_e.Exit;
                    }
                    else 
                    {
                        continue;
                    }

                    if (topo[i] is IFace2)
                    {
                        var bodyIndex = (int)topoData[i * 9];

                        var rayIndex = (int)topoData[i * 9 + 1];

                        hitRayIndices.Add(rayIndex);

                        var hitPt = new Point(topoData[i * 9 + 3], topoData[i * 9 + 4], topoData[i * 9 + 5]);

                        var hitNorm = new Vector(topoData[i * 9 + 6], topoData[i * 9 + 7], topoData[i * 9 + 8]);

                        var face = topo[i] as IFace2;

                        m_RaysList[rayIndex].RegisterHit(hitPt, hitNorm, bodies[bodyIndex], m_Doc.CreateObjectFromDispatch<ISwFace>(face), rayType);
                    }
                }
            }

            for (int i = 0; i < m_RaysList.Count; i++) 
            {
                if (!hitRayIndices.Contains(i)) 
                {
                    m_RaysList[i].Commit();
                }
            }

            return m_RaysList;
        }

        protected readonly IElementCreator<IReadOnlyList<IXRay>> m_Creator;

        private readonly ISwDocument3D m_Doc;

        internal SwRayIntersection(ISwDocument3D doc)
        {
            m_Doc = doc;
            
            m_Creator = new ElementCreator<IReadOnlyList<IXRay>>(CreateRayIntersections, null, false);

            m_RaysList = new List<SwRay>();
        }

        public IXRay AddRay(Axis rayAxis)
        {
            if (!IsCommitted)
            {
                var ray = new SwRay(rayAxis);
                m_RaysList.Add(ray);
                return ray;
            }
            else 
            {
                throw new CommittedElementPropertyChangeNotSupported();
            }
        }

        protected abstract IXBody[] GetAllBodies();

        public void Dispose()
        {
        }
    }

    internal class SwPartRayIntersection : SwRayIntersection
    {
        private readonly ISwPart m_Part;

        internal SwPartRayIntersection(ISwPart part) : base(part) 
        {
            m_Part = part;
        }

        protected override IXBody[] GetAllBodies()
            => m_Part.Bodies.OfType<IXSolidBody>()
            .Where(b => !VisibleOnly || b.Visible).ToArray();
    }

    internal class SwAssemblyRayIntersection : SwRayIntersection, ISwAssemblyRayIntersection
    {
        private readonly ISwAssembly m_Assm;

        internal SwAssemblyRayIntersection(ISwAssembly assm) : base(assm)
        {
            m_Assm = assm;
        }

        public override IXBody[] Scope
        {
            get
            {
                var comps = (this as IXAssemblyRayIntersection).Scope;

                if (comps == null)
                {
                    return base.Scope;
                }
                else
                {
                    return comps.SelectMany(c => c.IterateBodies(!VisibleOnly)).ToArray();
                }
            }
            set => base.Scope = value;
        }

        IXComponent[] IAssemblyEvaluation.Scope
        {
            get => m_Creator.CachedProperties.Get<IXComponent[]>(nameof(Scope) + "_Components");
            set
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value, nameof(Scope) + "_Components");
                }
                else 
                {
                    throw new CommittedElementPropertyChangeNotSupported();
                }
            }
        }

        protected override IXBody[] GetAllBodies()
            => m_Assm.Configurations.Active.Components.SelectMany(c => c.IterateBodies(!VisibleOnly)).ToArray();
    }
}
