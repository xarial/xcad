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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry.Exceptions;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Exceptions;

namespace Xarial.XCad.SolidWorks.Geometry
{
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

        public bool IgnoreUserAssignedValues
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

            if (massPrps == null)
            {
                throw new MassPropertyNotAvailableException();
            }

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

        protected void SetScope(IMassProperty massPrps)
        {
            var scope = Scope;

            if (scope == null)
            {
                if (VisibleOnly) 
                {
                    scope = GetAllVisibleBodies();
                }
            }

            if (!massPrps.AddBodies(scope?.Select(x => ((ISwBody)x).Body).ToArray()))
            {
                throw new Exception("Failed to add bodies to the scope");
            }

            //IMPORTANT: if this property is not called then the values are not calculated correctly
            var testRefreshCall = massPrps.OverrideCenterOfMass;
        }

        protected virtual IXBody[] GetAllVisibleBodies()
        {
            if (m_Doc is IXPart)
            {
                return ((IXPart)m_Doc).Bodies.OfType<IXSolidBody>().Where(b => b.Visible).ToArray();
            }
            else 
            {
                throw new NotSupportedException();
            }
        }

        public void Commit(CancellationToken cancellationToken)
            => m_Creator.Create(cancellationToken);

        public void Dispose()
        {
        }
    }

    internal class SwAssemblyLegacyMassProperty : SwLegacyMassProperty, ISwAssemblyMassProperty
    {
        private readonly ISwAssembly m_Assm;

        internal SwAssemblyLegacyMassProperty(ISwAssembly assm, IMathUtility mathUtils) : base(assm, mathUtils)
        {
            m_Assm = assm;
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
                    return comps.SelectMany(c => c.IterateBodies(!VisibleOnly)).OfType<ISwSolidBody>().ToArray();
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

        protected override IXBody[] GetAllVisibleBodies()
            => m_Assm.Configurations.Active.Components
            .IterateBodies(!VisibleOnly).OfType<IXSolidBody>().ToArray();
    }
}
