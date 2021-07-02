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
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Exceptions;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public interface ISwMassProperty : IXMassProperty
    {
    }

    public interface ISwAssemblyMassProperty : ISwMassProperty, IXAssemblyMassProperty 
    {
    }

    internal class SwMassProperty : ISwMassProperty
    {
        public Point CenterOfGravity => new Point((double[])m_Creator.Element.CenterOfMass);
        public double SurfaceArea => m_Creator.Element.SurfaceArea;
        public double Volume => m_Creator.Element.Volume;
        public double Mass => m_Creator.Element.Mass;
        public double Density => m_Creator.Element.Density;

        public PrincipalAxesOfInertia PrincipalAxesOfInertia
            => new PrincipalAxesOfInertia(
                new Vector((double[])m_Creator.Element.PrincipalAxesOfInertia[0]),
                new Vector((double[])m_Creator.Element.PrincipalAxesOfInertia[1]),
                new Vector((double[])m_Creator.Element.PrincipalAxesOfInertia[2]));

        public PrincipalMomentOfInertia PrincipalMomentOfInertia
            => new PrincipalMomentOfInertia((double[])m_Creator.Element.PrincipalMomentsOfInertia);

        public MomentOfInertia MomentOfInertia
        {
            get
            {
                var moi = (double[])m_Creator.Element.GetMomentOfInertia(RelativeTo != null
                    ? (int)swMassPropertyMoment_e.swMassPropertyMomentAboutCoordSys
                    : (int)swMassPropertyMoment_e.swMassPropertyMomentAboutCenterOfMass);

                return new MomentOfInertia(
                    new Vector(moi[0], moi[1], moi[2]),
                    new Vector(moi[3], moi[4], moi[5]),
                    new Vector(moi[6], moi[7], moi[8]));
            }
        }

        public TransformMatrix RelativeTo
        {
            get => m_Creator.CachedProperties.Get<TransformMatrix>();
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

        public IXBody[] Scope
        {
            get => m_Creator.CachedProperties.Get<IXBody[]>();
            set
            {
                m_Creator.CachedProperties.Set(value);

                if (IsCommitted)
                {
                    SetScope(m_Creator.Element);
                }
            }
        }

        public bool UserUnits
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

        public bool IsCommitted => m_Creator.IsCreated;

        private bool m_IncludeHiddenBodiesDefault;

        private readonly ISwDocument3D m_Doc;
        private readonly IMathUtility m_MathUtils;

        protected readonly ElementCreator<IMassProperty2> m_Creator;

        internal SwMassProperty(ISwDocument3D doc, IMathUtility mathUtils) 
        {
            m_Doc = doc;
            m_MathUtils = mathUtils;

            m_Creator = new ElementCreator<IMassProperty2>(CreateMassProperty, null, false);

            UserUnits = false;
        }

        private IMassProperty2 CreateMassProperty(CancellationToken cancellationToken)
        {
            var massPrps = (IMassProperty2)m_Doc.Model.Extension.CreateMassProperty2();

            massPrps.UseSystemUnits = !UserUnits;

            m_IncludeHiddenBodiesDefault = massPrps.IncludeHiddenBodiesOrComponents;

            if (RelativeTo != null)
            {
                if (!massPrps.SetCoordinateSystem(m_MathUtils.ToMathTransform(RelativeTo)))
                {
                    throw new Exception("Failed to set coordinate system");
                }
            }

            SetScope(massPrps);

            massPrps.IncludeHiddenBodiesOrComponents = !VisibleOnly;

            if (!massPrps.Recalculate())
            {
                throw new Exception($"Failed to recalculate mass properties");
            }

            return massPrps;
        }

        protected virtual void SetScope(IMassProperty2 massPrps)
        {
            var scope = Scope;

            if (scope != null)
            {
                massPrps.SelectedItems = scope.Select(x => ((ISwBody)x).Body).ToArray();
            }
            else 
            {
                massPrps.SelectedItems = null;
            }
        }

        public void Commit(CancellationToken cancellationToken)
            => m_Creator.Create(cancellationToken);

        public void Dispose()
        {
            if (m_Creator.IsCreated) 
            {
                m_Creator.Element.IncludeHiddenBodiesOrComponents = m_IncludeHiddenBodiesDefault;
            }
        }
    }

    internal class SwLegacyMassProperty : ISwMassProperty
    {
        public Point CenterOfGravity => new Point((double[])m_Creator.Element.CenterOfMass);
        public double SurfaceArea => m_Creator.Element.SurfaceArea;
        public double Volume => m_Creator.Element.Volume;
        public double Mass => m_Creator.Element.Mass;
        public double Density => m_Creator.Element.Density;

        public PrincipalAxesOfInertia PrincipalAxesOfInertia
            => new PrincipalAxesOfInertia(
                new Vector((double[])m_Creator.Element.PrincipleAxesOfInertia[0]),
                new Vector((double[])m_Creator.Element.PrincipleAxesOfInertia[1]),
                new Vector((double[])m_Creator.Element.PrincipleAxesOfInertia[2]));

        public PrincipalMomentOfInertia PrincipalMomentOfInertia
            => new PrincipalMomentOfInertia((double[])m_Creator.Element.PrincipleMomentsOfInertia);

        public MomentOfInertia MomentOfInertia
        {
            get
            {
                var moi = (double[])m_Creator.Element.GetMomentOfInertia(RelativeTo != null
                    ? (int)swMassPropertyMoment_e.swMassPropertyMomentAboutCoordSys
                    : (int)swMassPropertyMoment_e.swMassPropertyMomentAboutCenterOfMass);

                return new MomentOfInertia(
                    new Vector(moi[0], moi[1], moi[2]),
                    new Vector(moi[3], moi[4], moi[5]),
                    new Vector(moi[6], moi[7], moi[8]));
            }
        }

        public TransformMatrix RelativeTo
        {
            get => m_Creator.CachedProperties.Get<TransformMatrix>();
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
                m_Creator.CachedProperties.Set(value);

                if (IsCommitted) 
                {
                    SetScope(m_Creator.Element);
                }
            }
        }

        public bool UserUnits
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

        public bool IsCommitted => m_Creator.IsCreated;

        protected readonly ISwDocument3D m_Doc;
        private readonly IMathUtility m_MathUtils;

        protected readonly ElementCreator<IMassProperty> m_Creator;

        internal SwLegacyMassProperty(ISwDocument3D doc, IMathUtility mathUtils)
        {
            m_Doc = doc;
            m_MathUtils = mathUtils;

            m_Creator = new ElementCreator<IMassProperty>(CreateMassProperty, null, false);

            UserUnits = false;
        }

        private IMassProperty CreateMassProperty(CancellationToken cancellationToken)
        {
            var massPrps = (IMassProperty)m_Doc.Model.Extension.CreateMassProperty();

            massPrps.UseSystemUnits = !UserUnits;

            if (RelativeTo != null)
            {
                if (!massPrps.SetCoordinateSystem((MathTransform)m_MathUtils.ToMathTransform(RelativeTo)))
                {
                    throw new Exception("Failed to set coordinate system");
                }
            }

            SetScope(massPrps);

            return massPrps;
        }

        protected virtual void SetScope(IMassProperty massPrps)
        {
            var scope = Scope;

            if (scope != null)
            {
                if (!massPrps.AddBodies(scope.Select(x => ((ISwBody)x).Body).ToArray()))
                {
                    throw new Exception("Failed to add bodies to the scope");
                }
            }
        }

        public void Commit(CancellationToken cancellationToken)
            => m_Creator.Create(cancellationToken);

        public void Dispose()
        {
        }
    }

    internal class SwAssemblyMassProperty : SwMassProperty, ISwAssemblyMassProperty
    {
        internal SwAssemblyMassProperty(ISwAssembly assm, IMathUtility mathUtils) : base(assm, mathUtils)
        {
            VisibleOnly = true;
        }

        IXComponent[] IAssemblyEvaluation.Scope
        {
            get => m_Creator.CachedProperties.Get<IXComponent[]>();
            set
            {
                m_Creator.CachedProperties.Set(value);

                if (IsCommitted)
                {
                    SetScope(m_Creator.Element);
                }
            }
        }

        protected override void SetScope(IMassProperty2 massPrps)
        {
            var scope = (this as IXAssemblyMassProperty).Scope;

            if (scope != null)
            {
                massPrps.SelectedItems = scope.Select(x => ((ISwComponent)x).Component).ToArray();
            }

            massPrps.IncludeHiddenBodiesOrComponents = !VisibleOnly;
        }
    }

    internal class SwAssemblyLegacyMassProperty : SwLegacyMassProperty, ISwAssemblyMassProperty
    {
        internal SwAssemblyLegacyMassProperty(ISwAssembly assm, IMathUtility mathUtils) : base(assm, mathUtils)
        {
        }

        public override IXBody[] Scope
        {
            get
            {
                var comps = (this as IXAssemblyMassProperty).Scope;

                if (comps == null)
                {
                    return base.Scope;
                }
                else
                {
                    return comps.SelectMany(c => c.IterateBodies(!VisibleOnly).Select(b =>
                    {
                        var swBody = (b as ISwBody).Body;
                        var ent = swBody.GetFirstFace() as IEntity;
                        var comp = (IComponent2)ent.GetComponent();
                        
                        if (comp != null) 
                        {
                            swBody = swBody.ICopy();
                            swBody.ApplyTransform(comp.Transform2);
                        }
                        return SwObjectFactory.FromDispatch<ISwBody>(swBody, m_Doc);
                    })).ToArray();
                }
            }
            set => base.Scope = value;
        }

        IXComponent[] IAssemblyEvaluation.Scope
        {
            get => m_Creator.CachedProperties.Get<IXComponent[]>(nameof(Scope) + "_Components");
            set
            {
                m_Creator.CachedProperties.Set(value, nameof(Scope) + "_Components");

                if (IsCommitted)
                {
                    SetScope(m_Creator.Element);
                }
            }
        }
    }
}
