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
using Xarial.XCad.Geometry.Exceptions;
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
        public virtual Point CenterOfGravity
        {
            get
            {
                ThrowIfScopeException();
                return GetCenterOfGravity(m_Creator.Element);
            }
        }

        public double SurfaceArea
        {
            get
            {
                ThrowIfScopeException();
                return m_Creator.Element.SurfaceArea;
            }
        }

        public double Volume
        {
            get
            {
                ThrowIfScopeException();
                return m_Creator.Element.Volume;
            }
        }

        public virtual double Mass
        {
            get
            {
                ThrowIfScopeException();
                return GetMass(m_Creator.Element);
            }
        }

        public virtual double Density
        {
            get
            {
                ThrowIfScopeException();
                return GetDensity(m_Creator.Element);
            }
        }

        public virtual PrincipalAxesOfInertia PrincipalAxesOfInertia
        {
            get
            {
                ThrowIfScopeException();
                return GetPrincipalAxesOfInertia(m_Creator.Element);
            }
        }

        public virtual PrincipalMomentOfInertia PrincipalMomentOfInertia
        {
            get 
            {
                ThrowIfScopeException();
                return GetPrincipalMomentOfInertia(m_Creator.Element);
            }
        }

        public virtual MomentOfInertia MomentOfInertia
        {
            get
            {
                ThrowIfScopeException();
                return GetMomentOfInertia(m_Creator.Element);
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
                    UpdateScope(m_Creator.Element);
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
        protected readonly IMathUtility m_MathUtils;

        protected readonly ElementCreator<IMassProperty> m_Creator;

        private Exception m_CurrentScopeException;

        internal SwLegacyMassProperty(ISwDocument3D doc, IMathUtility mathUtils)
        {
            m_Doc = doc;
            m_MathUtils = mathUtils;

            m_Creator = new ElementCreator<IMassProperty>(CreateMassProperty, null, false);

            UserUnits = false;
        }

        public void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);

        protected void ThrowIfScopeException()
        {
            if (m_CurrentScopeException != null)
            {
                throw m_CurrentScopeException;
            }
        }

        protected Point GetCenterOfGravity(IMassProperty massPrps)
        {
            //NOTE: must be called otherwise calling other propertis may clear the overriden value
            var testCall = massPrps.OverrideCenterOfMass;
            return new Point((double[])massPrps.CenterOfMass);
        }

        protected double GetMass(IMassProperty massPrps)
        {
            //NOTE: must be called otherwise calling other propertis may clear the overriden value
            var testCall = massPrps.OverrideCenterOfMass;
            return massPrps.Mass;
        }

        protected double GetDensity(IMassProperty massPrps)
        {
            return massPrps.Density;
        }

        protected PrincipalAxesOfInertia GetPrincipalAxesOfInertia(IMassProperty massPrps)
        {
            //NOTE: must be called otherwise calling other propertis may clear the overriden value
            var testCall = massPrps.OverrideMomentsOfInertia;
            return new PrincipalAxesOfInertia(
                new Vector((double[])massPrps.PrincipleAxesOfInertia[(int)PrincipalAxesOfInertia_e.X]),
                new Vector((double[])massPrps.PrincipleAxesOfInertia[(int)PrincipalAxesOfInertia_e.Y]),
                new Vector((double[])massPrps.PrincipleAxesOfInertia[(int)PrincipalAxesOfInertia_e.Z]));
        }

        protected PrincipalMomentOfInertia GetPrincipalMomentOfInertia(IMassProperty massPrps)
        {
            //NOTE: must be called otherwise calling other propertis may clear the overriden value
            var testCall = massPrps.OverrideMomentsOfInertia;
            return new PrincipalMomentOfInertia((double[])massPrps.PrincipleMomentsOfInertia);
        }

        protected MomentOfInertia GetMomentOfInertia(IMassProperty massPrps)
        {
            var momentType = RelativeTo != null
                ? swMassPropertyMoment_e.swMassPropertyMomentAboutCoordSys
                : swMassPropertyMoment_e.swMassPropertyMomentAboutCenterOfMass;

            var moi = (double[])massPrps.GetMomentOfInertia((int)momentType);

            return new MomentOfInertia(
                new Vector(moi[0], moi[1], moi[2]),
                new Vector(moi[3], moi[4], moi[5]),
                new Vector(moi[6], moi[7], moi[8]));
        }

        protected void UpdateScope(IMassProperty massPrps)
        {
            try
            {
                m_CurrentScopeException = null;

                var scope = Scope;

                if (scope == null)
                {
                    if (VisibleOnly)
                    {
                        scope = IterateRootSolidBodies(false).ToArray();
                    }
                }

                if (scope == null)
                {
                    if (!IterateRootSolidBodies(!VisibleOnly).Any())
                    {
                        throw new EvaluationFailedException();
                    }
                }
                else
                {
                    if (!scope.Any())
                    {
                        throw new EvaluationFailedException();
                    }
                }

                if (!massPrps.AddBodies(scope?.Select(x => ((ISwBody)x).Body).ToArray()))
                {
                    throw new Exception("Failed to add bodies to the scope");
                }

                ValidateCalculations(massPrps);

                //IMPORTANT: if this property is not called then the values are not calculated correctly
                var testRefreshCall = massPrps.OverrideCenterOfMass;
            }
            catch (Exception ex) 
            {
                m_CurrentScopeException = ex;
                throw;
            }
        }

        protected virtual IEnumerable<IXBody> IterateRootSolidBodies(bool includeHidden)
        {
            if (m_Doc is IXPart)
            {
                return ((IXPart)m_Doc).Bodies.OfType<IXSolidBody>().Where(b => includeHidden || b.Visible);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private IMassProperty CreateMassProperty(CancellationToken cancellationToken)
        {
            var massPrps = (IMassProperty)m_Doc.Model.Extension.CreateMassProperty();

            if (massPrps == null)
            {
                throw new EvaluationFailedException();
            }

            massPrps.UseSystemUnits = !UserUnits;

            if (RelativeTo != null)
            {
                if (!massPrps.SetCoordinateSystem((MathTransform)m_MathUtils.ToMathTransform(RelativeTo)))
                {
                    throw new Exception("Failed to set coordinate system");
                }
            }

            UpdateScope(massPrps);

            return massPrps;
        }

        /// <summary>
        /// Sometimes mass cannot be calculated and no error returned (model rebuilding is required)
        /// </summary>
        private void ValidateCalculations(IMassProperty massPrps)
        {
            if (massPrps.SurfaceArea == 0 && massPrps.Volume == 0)
            {
                throw new InvalidMassPropertyCalculationException();
            }
        }

        public void Dispose()
        {
        }
    }

    internal class SwAssemblyLegacyMassProperty : SwLegacyMassProperty, ISwAssemblyMassProperty
    {
        private class ComponentMassProperty 
        {
            internal IXDocument3D Document { get; }
            internal IXComponent Component { get; }
            internal IMassProperty MassProperty { get; }
            internal IXUnits UserUnit { get; }
            
            internal ComponentMassProperty(IXDocument3D doc, IXComponent component, IMassProperty massProperty, IXUnits userUnit)
            {
                Document = doc;
                Component = component;
                MassProperty = massProperty;
                UserUnit = userUnit;
            }
        }

        public override Point CenterOfGravity
        {
            get
            {
                ThrowIfScopeException();

                if (NeedToReadMassPropertiesFromReferencedDocument(m => m.OverrideCenterOfMass, out IMassProperty compRefDocMassPrp))
                {
                    var comp = m_ReferenceComponentMassPropertyLazy.Value.Component;
                    var units = m_ReferenceComponentMassPropertyLazy.Value.UserUnit;

                    var cog = base.GetCenterOfGravity(compRefDocMassPrp);

                    cog = cog.Transform(comp.Transformation);

                    if (RelativeTo != null)
                    {
                        cog = cog.Transform(RelativeTo.Inverse());
                    }

                    if (units != null)
                    {
                        cog.Scale(units.GetLengthConversionFactor());
                    }

                    return cog;
                }
                else
                {
                    return base.CenterOfGravity;
                }
            }
        }

        public override double Mass 
        {
            get 
            {
                ThrowIfScopeException();

                if (NeedToReadMassPropertiesFromReferencedDocument(m => m.OverrideMass, out IMassProperty compRefDocMassPrp))
                {
                    var units = m_ReferenceComponentMassPropertyLazy.Value.UserUnit;

                    var mass = base.GetMass(compRefDocMassPrp);

                    return mass * (units != null ? units.GetMassConversionFactor() : 1);
                }
                else
                {
                    return base.Mass;
                }
            }
        }

        public override double Density 
        {
            get 
            {
                ThrowIfScopeException();

                if (NeedToReadMassPropertiesFromReferencedDocument(m => m.OverrideMass, out IMassProperty compRefDocMassPrp))
                {
                    var units = m_ReferenceComponentMassPropertyLazy.Value.UserUnit;

                    //mass / cubic length 
                    var confFactor = units != null ? units.GetMassConversionFactor() / Math.Pow(units.GetLengthConversionFactor(), 3) : 1;

                    var density = base.GetDensity(compRefDocMassPrp);

                    return density * (units != null ? confFactor : 1);
                }
                else
                {
                    return base.Density;
                }
            }
        }

        public override PrincipalAxesOfInertia PrincipalAxesOfInertia
        {
            get
            {
                ThrowIfScopeException();

                bool overrideMoi = false;

                if (NeedToReadMassPropertiesFromReferencedDocument(m =>
                {
                    overrideMoi = m.OverrideMomentsOfInertia;
                    return overrideMoi;
                }, out IMassProperty compRefDocMassPrp))
                {
                    var paoi = base.GetPrincipalAxesOfInertia(compRefDocMassPrp);

                    var ix = paoi.Ix;
                    var iy = paoi.Iy;
                    var iz = paoi.Iz;

                    if (!overrideMoi)
                    {
                        var compTransform = m_ReferenceComponentMassPropertyLazy.Value.Component.Transformation;

                        ix = ix.Transform(compTransform) * -1;
                        iy = iy.Transform(compTransform);
                        iz = iz.Transform(compTransform) * -1;
                    }

                    if (RelativeTo != null)
                    {
                        var relTransform = RelativeTo.Inverse();

                        ix = ix.Transform(relTransform);
                        iy = iy.Transform(relTransform);
                        iz = iz.Transform(relTransform);
                    }

                    return new PrincipalAxesOfInertia(ix, iy, iz);
                }
                else
                {
                    return base.PrincipalAxesOfInertia;
                }
            }
        }

        public override PrincipalMomentOfInertia PrincipalMomentOfInertia
        {
            get
            {
                ThrowIfScopeException();

                if (NeedToReadMassPropertiesFromReferencedDocument(m => m.OverrideMass || m.OverrideMomentsOfInertia, out IMassProperty compRefDocMassPrp))
                {
                    var units = m_ReferenceComponentMassPropertyLazy.Value.UserUnit;

                    //mass *  square length 
                    var confFactor = units != null ? units.GetMassConversionFactor() * Math.Pow(units.GetLengthConversionFactor(), 2) : 1;

                    var pmoi = base.GetPrincipalMomentOfInertia(compRefDocMassPrp);
                    
                    var px = pmoi.Px * confFactor;
                    var py = pmoi.Py * confFactor;
                    var pz = pmoi.Pz * confFactor;

                    return new PrincipalMomentOfInertia(px, py, pz);
                }
                else
                {
                    return base.PrincipalMomentOfInertia;
                }
            }
        }

        public override MomentOfInertia MomentOfInertia 
        {
            get 
            {
                ThrowIfScopeException();

                if (NeedToReadMassPropertiesFromReferencedDocument(m => 
                {
                    var refDoc = m_ReferenceComponentMassPropertyLazy.Value.Document;

                    if (refDoc is IXPart) 
                    {
                        if (m.OverrideMass || m.OverrideMomentsOfInertia)
                        {
                            throw new MomentOfInertiaOverridenException("Override Mass or Override Moments Of Intertia is set for part component");
                        }
                        else if (m.OverrideCenterOfMass && RelativeTo != null) 
                        {
                            throw new MomentOfInertiaOverridenException("Override Center Of Gravity is set to part component relative to the coordinate system");
                        }
                    }
                    else if (refDoc is IXAssembly)
                    {
                        if (m.OverrideMass || m.OverrideMomentsOfInertia || m.OverrideCenterOfMass)
                        {
                            throw new MomentOfInertiaOverridenException("Override Mass, Override Moments Of Intertia or Override Center Of Gravity is set for sub-assembly component");
                        }
                    }

                    return false;
                },
                    out IMassProperty compRefDocMassPrp))
                {
                    var transform = m_ReferenceComponentMassPropertyLazy.Value.Component.Transformation;

                    if (RelativeTo != null)
                    {
                        var relTransform = RelativeTo.Inverse();

                        transform = transform.Multiply(relTransform);
                    }

                    transform = transform.Inverse();

                    try
                    {
                        if (compRefDocMassPrp.SetCoordinateSystem((MathTransform)m_MathUtils.ToMathTransform(transform)))
                        {
                            var moi = base.GetMomentOfInertia(compRefDocMassPrp);
                            
                            var units = m_ReferenceComponentMassPropertyLazy.Value.UserUnit;

                            //mass *  square length 
                            var confFactor = units != null ? units.GetMassConversionFactor() * Math.Pow(units.GetLengthConversionFactor(), 2) : 1;

                            moi = new MomentOfInertia(moi.Lx * confFactor, moi.Ly * confFactor, moi.Lz * confFactor);

                            return moi;
                        }
                        else
                        {
                            throw new Exception("Failed to align Moment Of Inertia");
                        }
                    }
                    finally
                    {
                        if (!compRefDocMassPrp.SetCoordinateSystem((MathTransform)m_MathUtils.ToMathTransform(TransformMatrix.Identity)))
                        {
                            throw new Exception("Failed to reset Moment Of Inertia coordinate system");
                        }
                    }
                }
                else
                {
                    return base.MomentOfInertia;
                }
            }
        }

        private Lazy<ComponentMassProperty> m_ReferenceComponentMassPropertyLazy;

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

        private Lazy<bool> m_HasHiddenBodiesOrComponentsLazy;

        IXComponent[] IAssemblyEvaluation.Scope
        {
            get => m_Creator.CachedProperties.Get<IXComponent[]>(nameof(Scope) + "_Components");
            set
            {
                if (value?.Length == 1)
                {
                    m_ReferenceComponentMassPropertyLazy = new Lazy<ComponentMassProperty>(() =>
                    {
                        var comp = value.First();
                        var refDoc = (ISwDocument3D)comp.ReferencedDocument;

                        if (!refDoc.IsCommitted) 
                        {
                            throw new NotLoadedMassPropertyComponentException(comp);
                        }

                        var massPrps = refDoc.Model.Extension.CreateMassProperty();

                        //NOTE: always resolving the system units as it is requried to get units from the assembly (not the component) for the units and also by some reasons incorrect COG is returned for the user units
                        massPrps.UseSystemUnits = true;
                        
                        return new ComponentMassProperty(refDoc, comp, massPrps, UserUnits ? m_Assm.Units : null);
                    });

                    m_HasHiddenBodiesOrComponentsLazy = new Lazy<bool>(() => 
                    {
                        foreach (var body in value.First().IterateBodies(true).OfType<IXSolidBody>()) 
                        {
                            if (!body.Visible || body.Faces.First().Component.State.HasFlag(ComponentState_e.Hidden)) 
                            {
                                return true;
                            }
                        }

                        return false;
                    });
                }
                else if (value?.Length > 1)
                {
                    throw new NotSupportedException("Only single component is supported for scope in the assembly in SOLIDWORKS 2019 or older");
                }
                else
                {
                    m_ReferenceComponentMassPropertyLazy = null;
                }

                m_Creator.CachedProperties.Set(value, nameof(Scope) + "_Components");

                if (IsCommitted)
                {
                    UpdateScope(m_Creator.Element);
                }
            }
        }

        protected override IEnumerable<IXBody> IterateRootSolidBodies(bool includeHidden)
            => m_Assm.Configurations.Active.Components
            .IterateBodies(includeHidden).OfType<IXSolidBody>();

        // NOTE: overridden mass properties are not supported correctly by IMassProperty as individual bodies are added to the alculation
        // and document level overrides are not ignored. As a workaround we perform the calculation on the document level and adjusting to the assembly level
        private bool NeedToReadMassPropertiesFromReferencedDocument(Func<IMassProperty, bool> isOverriddenFunc, out IMassProperty massPrps)
        {
            massPrps = null;

            if (m_ReferenceComponentMassPropertyLazy != null)
            {
                var compMassPrp = m_ReferenceComponentMassPropertyLazy.Value;

                massPrps = compMassPrp.MassProperty;

                var isOverriden = isOverriddenFunc.Invoke(massPrps);

                if (isOverriden || HasOverridenMassPropertiesChildren(compMassPrp.Document)) 
                {
                    if (VisibleOnly && compMassPrp.Document is IXAssembly)
                    {
                        if (m_HasHiddenBodiesOrComponentsLazy.Value)
                        {
                            //read summary for this exception for more information about this condition
                            throw new MassPropertiesHiddenComponentBodiesNotSupported();
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        private bool HasOverridenMassPropertiesChildren(IXDocument3D doc) => doc is ISwAssembly;//NOTE: for simplicity we assume that assembly might have overriden mass properties in children to avoid checking all components
    }
}
