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

        internal SwCollisionDetection(SwDocument3D doc, SwApplication app)
        {
            m_Doc = doc;

            m_App = app;

            m_Creator = new ElementCreator<IXCollisionResult[]>(CalculateCollision, null, false);
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

            var results = new List<SwCollisionResult>();

            for (int i = 0; i < bodies.Length; i++) 
            {
                for (var j = i + 1; j < bodies.Length; j++) 
                {
                    var firstBody = CreateCollisionBody(bodies[i]);
                    var secondBody = CreateCollisionBody(bodies[j]);

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

            return results.ToArray();
        }

        protected virtual IXMemoryBody CreateCollisionBody(IXBody body) => body.Copy();

        public void Dispose()
        {
        }
    }

    internal class SwCollisionResult : IXCollisionResult
    {
        public IXBody[] CollidedBodies { get; }
        public IXMemoryBody[] CollisionVolume { get; }

        internal SwCollisionResult(IXBody[] collidedBodies, IXMemoryBody[] collisionVolume)
        {
            CollidedBodies = collidedBodies;
            CollisionVolume = collisionVolume;
        }
    }

    internal class SwAssemblyCollisionResult : SwCollisionResult, IXAssemblyCollisionResult
    {
        public IXComponent[] CollidedComponents { get; }
        public IInterference Interference { get; }

        internal SwAssemblyCollisionResult(IInterference interference, IXComponent[] collidedComps,
            IXBody[] collidedBodies, IXMemoryBody[] collisionVolume) 
            : base(collidedBodies, collisionVolume)
        {
            Interference = interference;
            CollidedComponents = collidedComps;
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

        private readonly SwAssembly m_Assm;

        public SwAssemblyCollisionDetection(SwAssembly assm, SwApplication app) : base(assm, app)
        {
            m_Assm = assm;
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
                m_Results = base.CalculateCollision(arg)?.Select(r => new SwAssemblyCollisionResult(null,
                    r.CollidedBodies?.Select(b => b.Component).Distinct(new XObjectEqualityComparer<IXComponent>()).ToArray(),
                    r.CollidedBodies, r.CollisionVolume)).ToArray();

                return m_Results;
            }
            else
            {
                var comps = (this as IXAssemblyCollisionDetection).Scope;

                var interfDetectMgr = m_Assm.Assembly.InterferenceDetectionManager;
                interfDetectMgr.TreatCoincidenceAsInterference = true;
                interfDetectMgr.UseTransform = true;
                interfDetectMgr.MakeInterferingPartsTransparent = false;
                interfDetectMgr.NonInterferingComponentDisplay = (int)swNonInterferingComponentDisplay_e.swNonInterferingComponentDisplay_Current;
                interfDetectMgr.ShowIgnoredInterferences = false;
                interfDetectMgr.TreatSubAssembliesAsComponents = false;
                interfDetectMgr.IgnoreHiddenBodies = VisibleOnly;

                if (comps?.Any() == true)
                {
                    var swComps = comps.Cast<ISwComponent>().Select(c => c.Component).ToArray();

                    var transforms = swComps.Select(c => c.Transform2).ToArray();

                    var res = (swSetComponentsAndTransformsStatus_e)interfDetectMgr.SetComponentsAndTransforms(
                        swComps, transforms);

                    if (res != swSetComponentsAndTransformsStatus_e.swSetComponentsAndTransforms_Succeeded)
                    {
                        throw new Exception($"Failed to set interference detection components: {res}");
                    }
                }

                var collisions = new List<IXAssemblyCollisionResult>();

                foreach (var interf in ((object[])interfDetectMgr.GetInterferences() ?? Array.Empty<object>()).Cast<IInterference>().ToArray())
                {
                    var collidedComps = ((object[])interf.Components ?? Array.Empty<object>())
                            .Select(c => m_Assm.CreateObjectFromDispatch<ISwComponent>(c)).ToArray();

                    IXMemoryBody[] collisionVolume;

                    var interVolume = interf.GetInterferenceBody();
                    if (interVolume != null)
                    {
                        collisionVolume = new IXMemoryBody[] { m_Assm.CreateObjectFromDispatch<ISwTempBody>(interVolume).Copy() };
                    }
                    else
                    {
                        collisionVolume = Array.Empty<IXMemoryBody>();
                    }

                    collisions.Add(new SwAssemblyCollisionResult(interf, collidedComps, null, collisionVolume));
                }

                interfDetectMgr.Done();

                m_Results = collisions.ToArray();

                return m_Results;
            }
        }

        protected override IXMemoryBody CreateCollisionBody(IXBody body)
        {
            if (!(body is IXMemoryBody))
            {
                var comp = body.Component;

                if (comp != null)
                {
                    var copy = body.Copy();

                    copy.Transform(comp.Transformation);

                    return copy;
                }
                else 
                {
                    throw new Exception("Body does not have parent component");
                }
            }
            else 
            {
                return body.Copy();
            }
        }
    }
}
