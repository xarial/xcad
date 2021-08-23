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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry.Exceptions;
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
        public Point CenterOfGravity => new Point((double[])MassProperty.CenterOfMass);
        public double SurfaceArea => MassProperty.SurfaceArea;
        public double Volume => MassProperty.Volume;
        public double Mass => MassProperty.Mass;

        public double Density => MassProperty.Density;

        public PrincipalAxesOfInertia PrincipalAxesOfInertia
        {
            get
            {
                const int AXIS_X = 0;
                const int AXIS_Y = 1;
                const int AXIS_Z = 2;

                if (m_Doc is ISwPart)
                {
                    return new PrincipalAxesOfInertia(
                        new Vector((double[])MassPropertyLegacy.PrincipleAxesOfInertia[AXIS_X]),
                        new Vector((double[])MassPropertyLegacy.PrincipleAxesOfInertia[AXIS_Y]),
                        new Vector((double[])MassPropertyLegacy.PrincipleAxesOfInertia[AXIS_Z]));
                }
                else 
                {
                    var overrides = (IMassPropertyOverrideOptions)MassProperty.GetOverrideOptions();

                    double[] x;
                    double[] y;
                    double[] z;

                    if (overrides.OverrideMomentsOfInertia)//invalid values returned for the Axis if overriden
                    {
                        x = (double[])overrides.GetOverridePrincipalAxesOrientation(AXIS_X);
                        y = (double[])overrides.GetOverridePrincipalAxesOrientation(AXIS_Y);
                        z = (double[])overrides.GetOverridePrincipalAxesOrientation(AXIS_Z);
                    }
                    else
                    {
                        x = (double[])MassProperty.PrincipalAxesOfInertia[AXIS_X];
                        y = (double[])MassProperty.PrincipalAxesOfInertia[AXIS_Y];
                        z = (double[])MassProperty.PrincipalAxesOfInertia[AXIS_Z];
                    }

                    return new PrincipalAxesOfInertia(new Vector(x), new Vector(y), new Vector(z));
                }
            }
        }

        public PrincipalMomentOfInertia PrincipalMomentOfInertia
        {
            get 
            {
                if (m_Doc is ISwPart)
                {
                    return new PrincipalMomentOfInertia((double[])MassPropertyLegacy.PrincipleMomentsOfInertia);
                }
                else 
                {
                    return new PrincipalMomentOfInertia((double[])MassProperty.PrincipalMomentsOfInertia);
                }
            }
        }

        public MomentOfInertia MomentOfInertia
        {
            get
            {
                var moi = (double[])MassProperty.GetMomentOfInertia(RelativeTo != null
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
                    SetScope(m_Creator.Element.Item1);

                    if (m_Doc is ISwPart)
                    {
                        SetPartScope((ISwPart)m_Doc, m_Creator.Element.Item2);
                    }
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

        protected readonly ISwDocument3D m_Doc;
        private readonly IMathUtility m_MathUtils;

        protected readonly ElementCreator<Tuple<IMassProperty2, IMassProperty>> m_Creator;

        protected IMassProperty2 MassProperty => m_Creator.Element.Item1;
        protected IMassProperty MassPropertyLegacy => m_Creator.Element.Item2;

        internal SwMassProperty(ISwDocument3D doc, IMathUtility mathUtils) 
        {
            m_Doc = doc;
            m_MathUtils = mathUtils;

            m_Creator = new ElementCreator<Tuple<IMassProperty2, IMassProperty>>(CreateMassProperty, null, false);

            UserUnits = false;
        }

        protected Tuple<IMassProperty2, IMassProperty> CreateMassProperty(CancellationToken cancellationToken)
        {
            var massPrps = (IMassProperty2)m_Doc.Model.Extension.CreateMassProperty2();

            if (massPrps == null) 
            {
                throw new MassPropertyNotAvailableException();
            }

            IMassProperty partMassPrps = null;

            if (m_Doc is ISwPart)
            {
                partMassPrps = m_Doc.Model.Extension.CreateMassProperty();
                partMassPrps.UseSystemUnits = !UserUnits;
            }

            massPrps.UseSystemUnits = !UserUnits;

            m_IncludeHiddenBodiesDefault = massPrps.IncludeHiddenBodiesOrComponents;

            if (RelativeTo != null)
            {
                if (!massPrps.SetCoordinateSystem(m_MathUtils.ToMathTransform(RelativeTo)))
                {
                    throw new Exception("Failed to set coordinate system");
                }

                if (m_Doc is ISwPart)
                {
                    if (!partMassPrps.SetCoordinateSystem((MathTransform)m_MathUtils.ToMathTransform(RelativeTo)))
                    {
                        throw new Exception("Failed to set coordinate system");
                    }
                }
            }

            massPrps.IncludeHiddenBodiesOrComponents = !VisibleOnly;

            SetScope(massPrps);

            if (m_Doc is ISwPart)
            {
                SetPartScope((ISwPart)m_Doc, partMassPrps);
            }

            return new Tuple<IMassProperty2, IMassProperty>(massPrps, partMassPrps);
        }

        protected void SetScope(IMassProperty2 massPrps)
        {
            var scope = GetSpecificSelectionScope();

            if (scope != null)
            {
                massPrps.SelectedItems = scope;
            }
            else 
            {
                if (massPrps.SelectedItems != null)
                {
                    m_Doc.Selections.Clear();
                }
            }

            if (!massPrps.Recalculate())
            {
                throw new Exception($"Failed to recalculate mass properties");
            }
        }

        //IMPORTANT: IMassProperty2 returns invalid results for PrincipalAxesOfInertia and PrincipalMomentOfInertia for parts
        private void SetPartScope(ISwPart part, IMassProperty massPrps)
        {
            var scope = Scope;

            if (scope == null)
            {
                var bodies = part.Bodies.OfType<ISwSolidBody>();

                if (VisibleOnly)
                {
                    bodies = bodies.Where(b => b.Visible);
                }

                scope = bodies.ToArray();
            }

            if (!massPrps.AddBodies(scope.Cast<ISwSolidBody>().Select(b => b.Body).ToArray())) 
            {
                throw new Exception("Failed to add bodies to mass property scope");
            }

            //IMPORTANT: if this property is not called then the principal moment of intertia and principal axes will not be calculated correctly
            var testRefreshCall = massPrps.OverrideCenterOfMass;
        }

        protected virtual DispatchWrapper[] GetSpecificSelectionScope()
        {
            var scope = Scope;

            if (scope != null)
            {
                var bodies = scope.OfType<IXSolidBody>().Select(x => new DispatchWrapper(((ISwBody)x).Body)).ToArray();

                if (!bodies.Any()) 
                {
                    throw new MassPropertyNotAvailableException();
                }

                return bodies;
            }
            else
            {
                return null;
            }
        }

        public void Commit(CancellationToken cancellationToken)
            => m_Creator.Create(cancellationToken);

        public void Dispose()
        {
            if (m_Creator.IsCreated) 
            {
                m_Creator.Element.Item1.IncludeHiddenBodiesOrComponents = m_IncludeHiddenBodiesDefault;
            }
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
                    SetScope(MassProperty);
                }
            }
        }

        protected override DispatchWrapper[] GetSpecificSelectionScope()
        {
            var scope = (this as IXAssemblyMassProperty).Scope;

            if (scope != null)
            {
                var hasSolidBodies = false;

                var comps = scope.Select(x =>
                {
                    if (!hasSolidBodies) 
                    {
                        hasSolidBodies = x.IterateBodies(!VisibleOnly).OfType<IXSolidBody>().Any();
                    }
                    return new DispatchWrapper(((ISwComponent)x).Component);
                }).ToArray();

                if (!hasSolidBodies)
                {
                    throw new MassPropertyNotAvailableException();
                }

                return comps;
            }
            else 
            {
                return null;
            }
        }
    }
}
