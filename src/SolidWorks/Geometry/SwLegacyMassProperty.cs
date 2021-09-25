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
                //NOTE: must be called otherwise calling other propertis may clear the overriden value
                var testCall = m_Creator.Element.OverrideCenterOfMass;
                return new Point((double[])m_Creator.Element.CenterOfMass);
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

                //NOTE: must be called otherwise calling other propertis may clear the overriden value
                var testCall = m_Creator.Element.OverrideCenterOfMass;
                return m_Creator.Element.Mass;
            }
        }

        public virtual double Density
        {
            get
            {
                ThrowIfScopeException();
                return m_Creator.Element.Density;
            }
        }

        public virtual PrincipalAxesOfInertia PrincipalAxesOfInertia
        {
            get
            {
                ThrowIfScopeException();
                //NOTE: must be called otherwise calling other propertis may clear the overriden value
                var testCall = m_Creator.Element.OverrideMomentsOfInertia;
                return new PrincipalAxesOfInertia(
                    new Vector((double[])m_Creator.Element.PrincipleAxesOfInertia[(int)PrincipalAxesOfInertia_e.X]),
                    new Vector((double[])m_Creator.Element.PrincipleAxesOfInertia[(int)PrincipalAxesOfInertia_e.Y]),
                    new Vector((double[])m_Creator.Element.PrincipleAxesOfInertia[(int)PrincipalAxesOfInertia_e.Z]));
            }
        }

        public virtual PrincipalMomentOfInertia PrincipalMomentOfInertia
        {
            get 
            {
                ThrowIfScopeException();
                //NOTE: must be called otherwise calling other propertis may clear the overriden value
                var testCall = m_Creator.Element.OverrideMomentsOfInertia;
                return new PrincipalMomentOfInertia((double[])m_Creator.Element.PrincipleMomentsOfInertia);
            }
        }

        public virtual MomentOfInertia MomentOfInertia
        {
            get
            {
                ThrowIfScopeException();
                
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

        protected void ThrowIfScopeException()
        {
            if (m_CurrentScopeException != null)
            {
                throw m_CurrentScopeException;
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

        public void Commit(CancellationToken cancellationToken)
            => m_Creator.Create(cancellationToken);

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

                if (m_ReferenceComponentMassPropertyLazy != null)
                {
                    var compRefDocMassPrp = m_ReferenceComponentMassPropertyLazy.Value.MassProperty;
                    
                    if (compRefDocMassPrp.OverrideCenterOfMass || NeedToReadMassPropertiesFromReferencedDocument())
                    {
                        var comp = m_ReferenceComponentMassPropertyLazy.Value.Component;
                        var units = m_ReferenceComponentMassPropertyLazy.Value.UserUnit;

                        var cog = new Point((double[])compRefDocMassPrp.CenterOfMass);
                        
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

                if (m_ReferenceComponentMassPropertyLazy != null)
                {
                    var compRefDocMassPrp = m_ReferenceComponentMassPropertyLazy.Value.MassProperty;

                    if (compRefDocMassPrp.OverrideMass || NeedToReadMassPropertiesFromReferencedDocument())
                    {
                        var units = m_ReferenceComponentMassPropertyLazy.Value.UserUnit;

                        return compRefDocMassPrp.Mass * (units != null ? units.GetMassConversionFactor() : 1);
                    }
                    else 
                    {
                        return base.Mass;
                    }
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

                if (m_ReferenceComponentMassPropertyLazy != null)
                {
                    var compRefDocMassPrp = m_ReferenceComponentMassPropertyLazy.Value.MassProperty;

                    if (compRefDocMassPrp.OverrideMass || NeedToReadMassPropertiesFromReferencedDocument())
                    {
                        var units = m_ReferenceComponentMassPropertyLazy.Value.UserUnit;

                        //mass / cubic length 
                        var confFactor = units != null ? units.GetMassConversionFactor() / Math.Pow(units.GetLengthConversionFactor(), 3) : 1;

                        return compRefDocMassPrp.Density * (units != null ? confFactor : 1);
                    }
                    else
                    {
                        return base.Density;
                    }
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

                if (m_ReferenceComponentMassPropertyLazy != null)
                {
                    var compRefDocMassPrp = m_ReferenceComponentMassPropertyLazy.Value.MassProperty;

                    var overrideMoi = compRefDocMassPrp.OverrideMomentsOfInertia;

                    if (overrideMoi || NeedToReadMassPropertiesFromReferencedDocument())
                    {
                        var ix = new Vector((double[])compRefDocMassPrp.PrincipleAxesOfInertia[(int)PrincipalAxesOfInertia_e.X]);
                        var iy = new Vector((double[])compRefDocMassPrp.PrincipleAxesOfInertia[(int)PrincipalAxesOfInertia_e.Y]);
                        var iz = new Vector((double[])compRefDocMassPrp.PrincipleAxesOfInertia[(int)PrincipalAxesOfInertia_e.Z]);

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

                if (m_ReferenceComponentMassPropertyLazy != null)
                {
                    var compRefDocMassPrp = m_ReferenceComponentMassPropertyLazy.Value.MassProperty;

                    if (compRefDocMassPrp.OverrideMomentsOfInertia || NeedToReadMassPropertiesFromReferencedDocument())
                    {
                        var units = m_ReferenceComponentMassPropertyLazy.Value.UserUnit;

                        //mass *  square length 
                        var confFactor = units != null ? units.GetMassConversionFactor() * Math.Pow(units.GetLengthConversionFactor(), 2) : 1;

                        var p = (double[])compRefDocMassPrp.PrincipleMomentsOfInertia;
                        
                        p[0] *= confFactor;
                        p[1] *= confFactor;
                        p[2] *= confFactor;

                        return new PrincipalMomentOfInertia(p);
                    }
                    else
                    {
                        return base.PrincipalMomentOfInertia;
                    }
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

                if (m_ReferenceComponentMassPropertyLazy != null)
                {
                    var compRefDocMassPrp = m_ReferenceComponentMassPropertyLazy.Value.MassProperty;

                    if (NeedToReadMassPropertiesFromReferencedDocument())
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
                                var moiData = (double[])compRefDocMassPrp.GetMomentOfInertia(RelativeTo != null
                                    ? (int)swMassPropertyMoment_e.swMassPropertyMomentAboutCoordSys
                                    : (int)swMassPropertyMoment_e.swMassPropertyMomentAboutCenterOfMass);

                                var units = m_ReferenceComponentMassPropertyLazy.Value.UserUnit;

                                //mass *  square length 
                                var confFactor = units != null ? units.GetMassConversionFactor() * Math.Pow(units.GetLengthConversionFactor(), 2) : 1;

                                var moi = new MomentOfInertia(
                                    new Vector(moiData[0], moiData[1], moiData[2]) * confFactor,
                                    new Vector(moiData[3], moiData[4], moiData[5]) * confFactor,
                                    new Vector(moiData[6], moiData[7], moiData[8]) * confFactor);

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

        private bool NeedToReadMassPropertiesFromReferencedDocument()
        {
            if (m_ReferenceComponentMassPropertyLazy != null)
            {
                if (m_ReferenceComponentMassPropertyLazy.Value.Document is ISwAssembly)
                {
                    if (VisibleOnly) 
                    {
                        if (m_HasHiddenBodiesOrComponentsLazy.Value) 
                        {
                            throw new MassPropertiesHiddenComponentBodiesNotSupported();
                        }
                    }

                    return true;
                }
            }

            return false;
        }
    }
}
