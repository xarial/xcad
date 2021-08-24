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
        public virtual Point CenterOfGravity => new Point((double[])m_Creator.Element.CenterOfMass);
        public double SurfaceArea => m_Creator.Element.SurfaceArea;
        public double Volume => m_Creator.Element.Volume;
        public virtual double Mass => m_Creator.Element.Mass;
        public double Density => m_Creator.Element.Density;

        public virtual PrincipalAxesOfInertia PrincipalAxesOfInertia
            => new PrincipalAxesOfInertia(
                new Vector((double[])m_Creator.Element.PrincipleAxesOfInertia[(int)PrincipalAxesOfInertia_e.X]),
                new Vector((double[])m_Creator.Element.PrincipleAxesOfInertia[(int)PrincipalAxesOfInertia_e.Y]),
                new Vector((double[])m_Creator.Element.PrincipleAxesOfInertia[(int)PrincipalAxesOfInertia_e.Z]));

        public virtual PrincipalMomentOfInertia PrincipalMomentOfInertia
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
                    scope = IterateSolidBodies(false).ToArray();
                }
            }

            if (scope == null)
            {
                if (!IterateSolidBodies(!VisibleOnly).Any()) 
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

            //IMPORTANT: if this property is not called then the values are not calculated correctly
            var testRefreshCall = massPrps.OverrideCenterOfMass;
        }

        protected virtual IEnumerable<IXBody> IterateSolidBodies(bool includeHidden)
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
            internal IXComponent Component { get; }
            internal IMassProperty MassProperty { get; }
            internal IXUnits UserUnit { get; }

            internal ComponentMassProperty(IXComponent component, IMassProperty massProperty, IXUnits userUnit)
            {
                Component = component;
                MassProperty = massProperty;
                UserUnit = userUnit;
            }
        }

        public override Point CenterOfGravity
        {
            get
            {
                if (m_ReferenceComponentMassPropertyLazy != null)
                {
                    var compRefDocMassPrp = m_ReferenceComponentMassPropertyLazy.Value.MassProperty;
                    
                    if (compRefDocMassPrp.OverrideCenterOfMass)
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
                if (m_ReferenceComponentMassPropertyLazy != null)
                {
                    var compRefDocMassPrp = m_ReferenceComponentMassPropertyLazy.Value.MassProperty;

                    if (compRefDocMassPrp.OverrideMass)
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

        public override PrincipalAxesOfInertia PrincipalAxesOfInertia
        {
            get
            {
                if (m_ReferenceComponentMassPropertyLazy != null)
                {
                    var compRefDocMassPrp = m_ReferenceComponentMassPropertyLazy.Value.MassProperty;

                    if (compRefDocMassPrp.OverrideMomentsOfInertia)
                    {
                        var ix = new Vector((double[])compRefDocMassPrp.PrincipleAxesOfInertia[(int)PrincipalAxesOfInertia_e.X]);
                        var iy = new Vector((double[])compRefDocMassPrp.PrincipleAxesOfInertia[(int)PrincipalAxesOfInertia_e.Y]);
                        var iz = new Vector((double[])compRefDocMassPrp.PrincipleAxesOfInertia[(int)PrincipalAxesOfInertia_e.Z]);

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
                if (m_ReferenceComponentMassPropertyLazy != null)
                {
                    var compRefDocMassPrp = m_ReferenceComponentMassPropertyLazy.Value.MassProperty;

                    if (compRefDocMassPrp.OverrideMomentsOfInertia)
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
                        
                        return new ComponentMassProperty(comp, massPrps, UserUnits ? m_Assm.Units : null);
                    });
                }
                else
                {
                    m_ReferenceComponentMassPropertyLazy = null;
                }

                m_Creator.CachedProperties.Set(value, nameof(Scope) + "_Components");

                if (IsCommitted)
                {
                    SetScope(m_Creator.Element);
                }
            }
        }

        protected override IEnumerable<IXBody> IterateSolidBodies(bool includeHidden)
            => m_Assm.Configurations.Active.Components
            .IterateBodies(includeHidden).OfType<IXSolidBody>();
    }
}
