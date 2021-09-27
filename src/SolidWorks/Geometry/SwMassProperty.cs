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
using Xarial.XCad.Geometry.Exceptions;
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

    internal enum PrincipalAxesOfInertia_e 
    {
        X = 0,
        Y = 1,
        Z = 2
    }

    internal class SwMassProperty : ISwMassProperty
    {
        public Point CenterOfGravity
        {
            get
            {
                ThrowIfScopeException();
                return new Point((double[])MassProperty.CenterOfMass);
            }
        }

        public double SurfaceArea
        {
            get
            {
                ThrowIfScopeException();
                return MassProperty.SurfaceArea;
            }
        }

        public double Volume
        {
            get
            {
                ThrowIfScopeException();
                return MassProperty.Volume;
            }
        }

        public double Mass
        {
            get
            {
                ThrowIfScopeException();
                return MassProperty.Mass;
            }
        }

        public double Density
        {
            get
            {
                ThrowIfScopeException();
                return MassProperty.Density;
            }
        }

        public PrincipalAxesOfInertia PrincipalAxesOfInertia
        {
            get
            {
                ThrowIfScopeException();

                if (m_Doc is ISwPart)
                {
                    return new PrincipalAxesOfInertia(
                        new Vector((double[])MassPropertyLegacy.PrincipleAxesOfInertia[(int)PrincipalAxesOfInertia_e.X]),
                        new Vector((double[])MassPropertyLegacy.PrincipleAxesOfInertia[(int)PrincipalAxesOfInertia_e.Y]),
                        new Vector((double[])MassPropertyLegacy.PrincipleAxesOfInertia[(int)PrincipalAxesOfInertia_e.Z]));
                }
                else 
                {
                    var overrides = (IMassPropertyOverrideOptions)MassProperty.GetOverrideOptions();

                    double[] ix;
                    double[] iy;
                    double[] iz;

                    if (overrides.OverrideMomentsOfInertia)//invalid values returned for the Axis if overriden
                    {
                        ix = (double[])overrides.GetOverridePrincipalAxesOrientation((int)PrincipalAxesOfInertia_e.X);
                        iy = (double[])overrides.GetOverridePrincipalAxesOrientation((int)PrincipalAxesOfInertia_e.Y);
                        iz = (double[])overrides.GetOverridePrincipalAxesOrientation((int)PrincipalAxesOfInertia_e.Z);
                    }
                    else
                    {
                        ix = (double[])MassProperty.PrincipalAxesOfInertia[(int)PrincipalAxesOfInertia_e.X];
                        iy = (double[])MassProperty.PrincipalAxesOfInertia[(int)PrincipalAxesOfInertia_e.Y];
                        iz = (double[])MassProperty.PrincipalAxesOfInertia[(int)PrincipalAxesOfInertia_e.Z];
                    }

                    return new PrincipalAxesOfInertia(new Vector(ix), new Vector(iy), new Vector(iz));
                }
            }
        }

        public PrincipalMomentOfInertia PrincipalMomentOfInertia
        {
            get 
            {
                ThrowIfScopeException();

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
                ThrowIfScopeException();

                //NOTE: if this is not called the incorrect values will be returned for sub-assemblies with override options when include hidden is false
                //keeping this for all cases as in case there are otehr unknow conditions
                var testCall = (IMassPropertyOverrideOptions)MassProperty.GetOverrideOptions();
                
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
                    UpdateScope(m_Creator.Element.Item1, m_Creator.Element.Item2);
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

        private Exception m_CurrentScopeException;

        internal SwMassProperty(ISwDocument3D doc, IMathUtility mathUtils) 
        {
            m_Doc = doc;
            m_MathUtils = mathUtils;

            m_Creator = new ElementCreator<Tuple<IMassProperty2, IMassProperty>>(CreateMassProperty, null, false);

            UserUnits = false;
        }

        protected void ThrowIfScopeException()
        {
            if (m_CurrentScopeException != null)
            {
                throw m_CurrentScopeException;
            }
        }

        protected Tuple<IMassProperty2, IMassProperty> CreateMassProperty(CancellationToken cancellationToken)
        {
            var massPrps = (IMassProperty2)m_Doc.Model.Extension.CreateMassProperty2();

            if (massPrps == null) 
            {
                throw new EvaluationFailedException();
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

            UpdateScope(massPrps, partMassPrps);

            return new Tuple<IMassProperty2, IMassProperty>(massPrps, partMassPrps);
        }

        protected void UpdateScope(IMassProperty2 massPrps, IMassProperty partMassPrps = null)
        {
            try
            {
                m_CurrentScopeException = null;

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

                if (m_Doc is ISwPart)
                {
                    UpdatePartScope((ISwPart)m_Doc, partMassPrps);
                }

                ValidateCalculations(massPrps);
            }
            catch (Exception ex) 
            {
                m_CurrentScopeException = ex;
                throw;
            }
        }

        /// <summary>
        /// Sometimes mass cannot be calculated and no error returned (model rebuilding is required)
        /// </summary>
        private void ValidateCalculations(IMassProperty2 massPrp)
        {
            if (massPrp.SurfaceArea == 0 && massPrp.Volume == 0)
            {
                throw new InvalidMassPropertyCalculationException();
            }
        }

        //IMPORTANT: IMassProperty2 returns invalid results for PrincipalAxesOfInertia and PrincipalMomentOfInertia for parts
        private void UpdatePartScope(ISwPart part, IMassProperty massPrps)
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
                    throw new EvaluationFailedException();
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
                    UpdateScope(MassProperty);
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
                    throw new EvaluationFailedException();
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
