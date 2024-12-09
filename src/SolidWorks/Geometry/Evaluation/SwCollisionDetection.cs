//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Evaluation;
using Xarial.XCad.Geometry.Structures;
using static Xarial.XCad.SolidWorks.Geometry.Evaluation.SwBoundingBox;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.Services;
using Xarial.XCad.Toolkit.Exceptions;
using Xarial.XCad.Documents;
using Xarial.XCad.Base;
using Xarial.XCad.Geometry.Exceptions;
using System.Configuration;
using Xarial.XCad.Documents.Enums;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.sldworks;
using Xarial.XCad.SolidWorks.Utils;
using System.Runtime.InteropServices;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.Toolkit;
using Microsoft.VisualBasic;

namespace Xarial.XCad.SolidWorks.Geometry.Evaluation
{
    /// <summary>
    /// Represents SOLIDWORKS specific collision detection
    /// </summary>
    public interface ISwCollisionDetection : IXCollisionDetection 
    {
    }

    internal class SwCollisionDetection : ISwCollisionDetection
    {
        #region NotSupported
        public TransformMatrix RelativeTo { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public bool UserUnits { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        #endregion

        protected readonly IElementCreator<IXCollisionResult[]> m_Creator;

        private readonly SwDocument3D m_Doc;

        private readonly SwApplication m_App;

        private readonly IModeler m_Modeler;

        internal SwCollisionDetection(SwDocument3D doc, SwApplication app)
        {
            m_Doc = doc;

            m_App = app;

            m_Modeler = app.Sw.IGetModeler();

            m_Creator = new ElementCreator<IXCollisionResult[]>(CalculateCollision, null, false);

            m_Creator.CachedProperties.Set(true, nameof(IncludeCoincidentContact));
        }

        public IXCollisionResult[] Results => m_Creator.Element;

        public bool Precise
        {
            get => true;
            set
            {
            }
        }

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

        public bool IncludeCoincidentContact
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

        public void Commit(CancellationToken cancellationToken)
            => m_Creator.Create(cancellationToken);

        protected virtual IXCollisionResult[] CalculateCollision(CancellationToken cancellationToken) 
        {
            var bodies = Scope;

            if (bodies?.Any() != true)
            {
                bodies = GetAllBodies();
            }

            var results = new List<SwCollisionResult>();

            for (int i = 0; i < bodies.Length; i++) 
            {
                for (var j = i + 1; j < bodies.Length; j++) 
                {
                    var firstBody = CreateCollisionBody(bodies[i]);
                    var secondBody = CreateCollisionBody(bodies[j]);

                    if (IncludeCoincidentContact)
                    {
                        object firstBodyFaces = null;
                        object secondBodyFaces = null;
                        object intersectBodies = null;//actual bodies colliding, not the intersecting volume

                        if (m_Modeler.CheckInterference3(new IBody2[] { firstBody.Body }, new IBody2[] { secondBody.Body },
                            (int)swCheckInterferenceOption_e.swBodyInterference_IncludeCoincidentFaces, ref firstBodyFaces, ref secondBodyFaces, ref intersectBodies))
                        {
                            results.Add(new SwCollisionResult(new IXBody[] { bodies[i], bodies[j] }, new Lazy<IXMemoryBody[]>(() =>
                            {
                                try
                                {
                                    var intersection = firstBody.Common(secondBody);

                                    return intersection;
                                }
                                catch
                                {
                                    //IBody2::Operation2 does not return the common for the coincidence intersection
                                    return Array.Empty<IXMemoryBody>();
                                }
                            })));
                        }
                    }
                    else 
                    {
                        //NOTE: body boolean operations does not work for the contact surface, using this option if contact is not required to improve performance
                        try
                        {
                            var intersection = firstBody.Common(secondBody);

                            if (intersection?.Any() == true)
                            {
                                results.Add(new SwCollisionResult(new IXBody[] { bodies[i], bodies[j] }, intersection));
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }

            return results.ToArray();
        }

        private IXBody[] GetAllBodies()
        {
            switch (m_Doc) 
            {
                case IXPart part:
                    return part.Bodies.OfType<IXSolidBody>()
                        .Where(b => !VisibleOnly || b.Visible).ToArray();

                case IXAssembly assm:
                    return assm.Configurations.Active.Components
                        .SelectMany(c => c.IterateBodies(!VisibleOnly)).ToArray();

                default:
                    throw new NotSupportedException();
            }
        }

        protected virtual ISwTempBody CreateCollisionBody(IXBody body) => (ISwTempBody)body.Copy();

        public void Dispose()
        {
        }
    }

    internal class SwCollisionResult : IXCollisionResult
    {
        public virtual IXBody[] CollidedBodies { get; }
        public IXMemoryBody[] CollisionVolume 
        {
            get 
            {
                if (m_CollisionVolumeLazy != null)
                {
                    return m_CollisionVolumeLazy.Value;
                }
                else 
                {
                    return m_CollisionVolume;
                }
            }
        }

        private readonly IXMemoryBody[] m_CollisionVolume;
        private readonly Lazy<IXMemoryBody[]> m_CollisionVolumeLazy;
        
        internal SwCollisionResult(IXBody[] collidedBodies, IXMemoryBody[] collisionVolume)
        {
            CollidedBodies = collidedBodies;
            m_CollisionVolume = collisionVolume;
        }

        internal SwCollisionResult(IXBody[] collidedBodies, Lazy<IXMemoryBody[]> collisionVolumeLazy)
        {
            CollidedBodies = collidedBodies;
            m_CollisionVolumeLazy = collisionVolumeLazy;
        }
    }

    internal class SwAssemblyCollisionResult : SwCollisionResult, IXAssemblyCollisionResult
    {
        public IXComponent[] CollidedComponents { get; }

        public override IXBody[] CollidedBodies => m_CollidedBodiesLazy.Value;

        public IInterference Interference { get; }

        private readonly Lazy<IXBody[]> m_CollidedBodiesLazy;

        internal SwAssemblyCollisionResult(IXComponent[] collidedComps,
            IXBody[] collidedBodies, IXMemoryBody[] collisionVolume)
            : this(null, collidedComps, new Lazy<IXBody[]>(() => collidedBodies), collisionVolume)
        {
        }

        internal SwAssemblyCollisionResult(IInterference interference,
            IXComponent[] collidedComps,
            IXMemoryBody[] collisionVolume)
            : this(interference, collidedComps,
                  new Lazy<IXBody[]>(() => GetCollidedBodiesByVolume(collidedComps, collisionVolume)), collisionVolume)
        {
        }

        private SwAssemblyCollisionResult(IInterference interference, IXComponent[] collidedComps,
            Lazy<IXBody[]> collidedBodiesLazy, IXMemoryBody[] collisionVolume)
            : base(null, collisionVolume)
        {
            Interference = interference;
            
            m_CollidedBodiesLazy = collidedBodiesLazy;
            CollidedComponents = collidedComps;
        }

        private static IXBody[] GetCollidedBodiesByVolume(IXComponent[] collidedComps,
            IXMemoryBody[] collisionVolume)
        {
            var collidedBodies = new List<IXBody>();

            foreach (var collidedComp in collidedComps) 
            {
                var collidedCompBodies = collidedComp.Bodies.ToArray();

                if (collidedCompBodies.Length == 1)
                {
                    collidedBodies.Add(collidedCompBodies.First());
                }
                else 
                {
                    var compTransform = collidedComp.Transformation;

                    foreach (var collidedCompBody in collidedCompBodies) 
                    {
                        if (collisionVolume.Any(v => IsAbsorbed(collidedCompBody, compTransform, v))) 
                        {
                            collidedBodies.Add(collidedCompBody);
                        }
                    }
                }
            }

            return collidedBodies.ToArray();
        }

        private static bool IsAbsorbed(IXBody mainBody, TransformMatrix mainBodyTransform, IXBody toolBody)
        {
            IXMemoryBody PrepareBody(IXBody body, TransformMatrix transform) 
            {
                var copyBody = body.Copy();

                if (transform != null)
                {
                    copyBody.Transform(transform);
                }

                return copyBody;
            }

            try
            {
                var subsBodies = PrepareBody(toolBody, null).Subtract(PrepareBody(mainBody, mainBodyTransform));

                return subsBodies?.Any() != true;
            }
            catch 
            {
            }

            return false;
        }
    }

    /// <summary>
    /// <see cref="ISwAssembly"/> specific implementation of <see cref="ISwCollisionDetection"/>
    /// </summary>
    public interface ISwAssemblyCollisionDetection : IXAssemblyCollisionDetection 
    {
    }

    internal class SwAssemblyCollisionDetection : SwCollisionDetection, ISwAssemblyCollisionDetection
    {
        IXComponent[] IAssemblyEvaluation.Scope
        {
            get => m_Creator.CachedProperties.Get<IXComponent[]>(nameof(Scope) + "%Components");
            set
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value, nameof(Scope) + "%Components");
                }
                else
                {
                    throw new CommittedElementPropertyChangeNotSupported();
                }
            }
        }

        private readonly SwAssembly m_Assm;
        private readonly IInterferencesProvider m_InterferencesProvider;

        public SwAssemblyCollisionDetection(SwAssembly assm, SwApplication app) : base(assm, app)
        {
            m_Assm = assm;
            m_InterferencesProvider = app.Services.GetService<IInterferencesProvider>();
        }

        IXAssemblyCollisionResult[] IXAssemblyCollisionDetection.Results => m_Results;

        public override IXBody[] Scope
        {
            get
            {
                var comps = (this as IXAssemblyCollisionDetection).Scope;

                if (comps?.Any() != true)
                {
                    return base.Scope;
                }
                else
                {
                    var bodies = comps.SelectMany(c => c.IterateBodies(!VisibleOnly)).ToArray();

                    if (bodies?.Any() != true)
                    {
                        throw new EvaluationFailedException("No bodies found in the component");
                    }

                    return bodies;
                }
            }
            set => base.Scope = value;
        }

        private IXAssemblyCollisionResult[] m_Results;

        protected override IXCollisionResult[] CalculateCollision(CancellationToken arg)
        {
            if (base.Scope != null)
            {
                //if bodies are specified through the parent interface, calculate using bodies
                m_Results = base.CalculateCollision(arg)?.Select(r => new SwAssemblyCollisionResult(
                    r.CollidedBodies?.Select(b => b.Component).Distinct(new XObjectEqualityComparer<IXComponent>()).ToArray(),
                    r.CollidedBodies, r.CollisionVolume)).ToArray();

                return m_Results;
            }
            else
            {
                var collisions = new List<IXAssemblyCollisionResult>();

                using (var interferences = m_InterferencesProvider.GetInterferences(m_Assm, (this as IXAssemblyCollisionDetection).Scope, VisibleOnly, IncludeCoincidentContact))
                {
                    foreach (var interference in interferences) 
                    {
                        var collidedComps = ((object[])interference.Components ?? Array.Empty<object>())
                                .Select(m_Assm.CreateObjectFromDispatch<ISwComponent>).ToArray();

                        IXMemoryBody[] collisionVolume;

                        var interVolume = interference.GetInterferenceBody();
                        if (interVolume != null)
                        {
                            collisionVolume = new IXMemoryBody[] { m_Assm.CreateObjectFromDispatch<ISwTempBody>(interVolume).Copy() };
                        }
                        else
                        {
                            collisionVolume = Array.Empty<IXMemoryBody>();
                        }

                        collisions.Add(new SwAssemblyCollisionResult(interference, collidedComps, collisionVolume));
                    }
                }

                m_Results = collisions.ToArray();
                
                return m_Results;
            }
        }

        protected override ISwTempBody CreateCollisionBody(IXBody body)
        {
            if (!(body is IXMemoryBody))
            {
                var comp = body.Component;

                if (comp != null)
                {
                    var copy = body.Copy();

                    copy.Transform(comp.Transformation);

                    return (ISwTempBody)copy;
                }
                else 
                {
                    throw new Exception("Body does not have parent component");
                }
            }
            else 
            {
                return (ISwTempBody)body.Copy();
            }
        }
    }
}
