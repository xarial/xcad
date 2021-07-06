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
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Exceptions;

namespace Xarial.XCad.SolidWorks.Geometry
{
    internal class SwLegacyMassProperty : ISwMassProperty
    {
        private class SwSolidBodyEqualityComparer : IEqualityComparer<ISwSolidBody>
        {
            public bool Equals(ISwSolidBody x, ISwSolidBody y)
                => x.Equals(y);

            public int GetHashCode(ISwSolidBody obj)
                => 0;
        }

        protected class MassPropertyHandler 
        {
            public Point CenterOfGravity { get; private set; }
            public double SurfaceArea { get; private set; }
            public double Volume { get; private set; }
            public double Mass { get; private set; }
            public double Density { get; private set; }
            public PrincipalAxesOfInertia PrincipalAxesOfInertia { get; private set; }
            public PrincipalMomentOfInertia PrincipalMomentOfInertia { get; private set; }
            public MomentOfInertia MomentOfInertia { get; private set; }

            private readonly SwDocument3D m_Doc;
            private readonly ISwApplication m_App;

            internal MassPropertyHandler(SwDocument3D doc) 
            {
                m_Doc = doc;
                m_App = doc.App;
            }

            internal void Update(ISwSolidBody[] scope, bool systemUnits, bool includeHidden, MathTransform transform) 
            {
                var momentType = transform == null
                        ? swMassPropertyMoment_e.swMassPropertyMomentAboutCenterOfMass
                        : swMassPropertyMoment_e.swMassPropertyMomentAboutCoordSys;

                if (scope != null)
                {
                    var compGroups = scope.GroupBy(b =>
                    {
                        var swBody = (b as ISwBody).Body;
                        var ent = swBody.GetFirstFace() as IEntity;
                        var comp = (IComponent2)ent.GetComponent();
                        return comp;
                    });

                    IComponent2 ownerComp = null;

                    if (compGroups.Count() == 1 && compGroups.First() != null)
                    {
                        ownerComp = compGroups.First().Key;
                    }
                    else
                    {
                        if (compGroups.All(x => x.Key != null))
                        {
                            //TODO: try to find the common parent of the components
                            //owner = GetCommonParent(compGroups.Select(x => x.Key).ToArray());
                            //TODO: if owner is not a current assembly then get corresponding pointers for all bodies in the context of this document so those can be compared later
                        }
                    }

                    var ownerDoc = m_App.Documents[(IModelDoc2)ownerComp?.GetModelDoc2() ?? m_Doc.Model];

                    ISwSolidBody[] currentBodiesScope;

                    if (ownerComp != null)
                    {
                        var comp = SwObjectFactory.FromDispatch<ISwComponent>(ownerComp, m_Doc);
                        currentBodiesScope = comp.IterateBodies(true).OfType<ISwSolidBody>().ToArray();
                    }
                    else
                    {
                        if (ownerDoc is ISwPart)
                        {
                            currentBodiesScope = ((ISwPart)ownerDoc).Bodies.OfType<ISwSolidBody>().ToArray();
                        }
                        else if (ownerDoc is ISwAssembly)
                        {
                            currentBodiesScope = ((ISwAssembly)ownerDoc).Configurations.Active.Components
                                .Where(c => !c.State.HasFlag(ComponentState_e.Suppressed))
                                .SelectMany(c => c.IterateBodies(true).OfType<ISwSolidBody>()).ToArray();
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }
                    }

                    var isFullScope = currentBodiesScope.OrderBy(b => GetFullName(ownerDoc, b))
                        .SequenceEqual(scope.OrderBy(b => GetFullName(ownerDoc, b)),
                        new SwSolidBodyEqualityComparer());

                    if (ownerComp != null) 
                    {
                        var compTransform = ownerComp.Transform2;

                        if (transform != null)
                        {
                            transform = (MathTransform)compTransform.Multiply(transform);
                        }
                        else 
                        {
                            transform = compTransform;
                        }
                    }

                    if (isFullScope)
                    {
                        CalculateMassProperties(ownerDoc.Model, systemUnits, transform, momentType);
                    }
                    else
                    {
                        CalculateMassPropertiesInTempModel(ownerDoc, scope, systemUnits, transform, momentType);
                    }
                }
                else 
                {
                    if (m_Doc is ISwPart)
                    {
                        var bodies = ((ISwPart)m_Doc).Bodies.OfType<ISwSolidBody>();

                        if (!includeHidden)
                        {
                            bodies = bodies.Where(b => b.Visible);
                        }

                        Update(bodies.ToArray(), systemUnits, includeHidden, transform);
                    }
                    else if (m_Doc is ISwAssembly)
                    {
                        if (includeHidden)
                        {
                            CalculateMassProperties(m_Doc.Model, systemUnits, transform, momentType);
                        }
                        else 
                        {
                            Update(((ISwAssembly)m_Doc).Configurations.Active.Components
                                .Where(c => !c.State.HasFlag(ComponentState_e.Suppressed) && (includeHidden || !c.State.HasFlag(ComponentState_e.Hidden)))
                                .SelectMany(c => c.IterateBodies(includeHidden)).OfType<ISwSolidBody>().ToArray(),
                                systemUnits, includeHidden, transform);
                        }
                    }
                    else 
                    {
                        throw new NotSupportedException();
                    }
                }
            }

            internal ISwBody[] Update(ISwComponent[] comps, bool systemUnits, bool includeHidden, MathTransform transform) 
            {
                var momentType = transform == null
                    ? swMassPropertyMoment_e.swMassPropertyMomentAboutCenterOfMass
                    : swMassPropertyMoment_e.swMassPropertyMomentAboutCoordSys;

                if (comps != null)
                {
                    if (comps.Length == 1 && comps.First().Component.IGetModelDoc() != null
                        && (includeHidden || !comps.First().Bodies.OfType<ISwSolidBody>().Any(b => !b.Visible)))
                    {
                        var comp = comps.First();
                        
                        var compTransform = comp.Component.Transform2;
                        
                        if (transform != null) 
                        {
                            compTransform = (MathTransform)compTransform.Multiply(transform);
                        }

                        CalculateMassProperties(comp.Component.IGetModelDoc(), systemUnits, compTransform, momentType);

                        return comp.Bodies.OfType<ISwSolidBody>().ToArray();
                    }
                    else
                    {
                        var bodies = comps.SelectMany(c => c.IterateBodies(includeHidden).OfType<ISwSolidBody>()).ToArray();
                        Update(bodies, systemUnits, includeHidden, transform);
                        return bodies;
                    }
                }
                else 
                {
                    if (m_Doc is ISwAssembly)
                    {
                        var allBodies = ((ISwAssembly)m_Doc).Configurations.Active.Components
                                .Where(c => !c.State.HasFlag(ComponentState_e.Suppressed))
                                .SelectMany(c => c.IterateBodies(true).OfType<ISwSolidBody>()).ToArray();

                        if (includeHidden || !allBodies.Any(b => b.Visible))
                        {
                            CalculateMassProperties(m_Doc.Model, systemUnits, transform, momentType);
                        }
                        else 
                        {
                            Update(allBodies, systemUnits, includeHidden, transform);
                        }

                        return allBodies;
                    }
                    else 
                    {
                        throw new NotSupportedException();
                    }
                }
            }

            private void CalculateMassPropertiesInTempModel(ISwDocument doc, ISwBody[] bodies, bool useSystemUnits,
                MathTransform transform, swMassPropertyMoment_e momentType)
            {
                if (bodies == null)
                {
                    throw new ArgumentNullException(nameof(bodies));
                }

                var bodiesData = bodies.ToDictionary(b =>
                {
                    var tempBody = b.Body.ICopy();

                    var comp = TryGetComponent(b.Body);

                    if (comp != null)
                    {
                        if (!tempBody.ApplyTransform(comp.Transform2))
                        {
                            throw new Exception("Failed to apply transform to the body");
                        }
                    }

                    return tempBody;
                }, 
                b => 
                {
                    var comp = TryGetComponent(b.Body);
                    
                    string material = "";
                    string materialDb = "";
                    double? density = null;

                    var confName = comp?.ReferencedConfiguration ?? doc.Model.ConfigurationManager.ActiveConfiguration.Name;

                    if (!TryGetMaterial(b.Body, confName, out material, out materialDb))
                    {
                        var part = (comp?.IGetModelDoc() ?? doc.Model) as IPartDoc;

                        if (part != null)
                        {
                            if (!TryGetMaterial(part, confName, out material, out materialDb)) 
                            {
                                material = "";
                                materialDb = "";
                                density = GetUserAssignedDensity(part);
                            }
                        }
                        else
                        {
                            throw new Exception("Part for the body is not loaded");
                        }
                    }

                    return new Tuple<string, string, double?>(material, materialDb, density);
                });

                if (bodiesData.Where(b => string.IsNullOrEmpty(b.Value.Item1)).GroupBy(b => b.Value.Item3).Count() > 1) 
                {
                    //TODO: this should be imlemented by creating temp assembly and grouping bodies into the virtual components by density
                    throw new NotSupportedException("Parts with different densities are nto currently supported");
                }

                var tempDoc = m_App.Documents.PreCreate<ISwPart>();

                try
                {
                    tempDoc.State = DocumentState_e.Hidden | DocumentState_e.Silent;
                    tempDoc.Commit();

                    foreach (var bodyData in bodiesData) 
                    {
                        var feat = (IFeature)tempDoc.Part.CreateFeatureFromBody3(bodyData.Key, 
                            false, (int)swCreateFeatureBodyOpts_e.swCreateFeatureBodyCheck);

                        var body = ((IFace2)((object[])feat.GetFaces()).First()).IGetBody();

                        if (!string.IsNullOrEmpty(bodyData.Value.Item1)) 
                        {
                            var material = bodyData.Value.Item1;
                            var materialDb = bodyData.Value.Item2;

                            if (body.Select2(false, null))
                            {
                                var res = (swBodyMaterialApplicationError_e)body.SetMaterialProperty(
                                    tempDoc.Model.ConfigurationManager.ActiveConfiguration.Name, materialDb, material);

                                if (res != swBodyMaterialApplicationError_e.swBodyMaterialApplicationError_NoError) 
                                {
                                    throw new Exception($"Failed to apply material: {res}");
                                }
                            }
                            else 
                            {
                                throw new Exception("Failed to select body");
                            }
                        }

                        if (bodyData.Value.Item3.HasValue) 
                        {
                            var density = bodyData.Value.Item3.Value;

                            if (!tempDoc.Model.Extension.SetUserPreferenceDouble(
                                (int)swUserPreferenceDoubleValue_e.swMaterialPropertyDensity,
                                (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified,
                                density)) 
                            {
                                throw new Exception("Failed to set density for the part");
                            }
                        }
                    }
                    
                    CalculateMassProperties(tempDoc.Model, useSystemUnits, transform, momentType);
                }
                finally 
                {
                    tempDoc?.Close();
                }
            }

            private bool TryGetMaterial(IBody2 body, string confName, out string matName, out string dbName) 
            {
                matName = body.GetMaterialPropertyName(confName, out dbName);
                return !string.IsNullOrEmpty(matName);
            }

            private bool TryGetMaterial(IPartDoc part, string confName, out string matName, out string dbName)
            {
                matName = part.GetMaterialPropertyName2(confName, out dbName);
                return !string.IsNullOrEmpty(matName);
            }

            private double GetUserAssignedDensity(IPartDoc part)
                => ((IModelDoc2)part).Extension.GetUserPreferenceDouble(
                    (int)swUserPreferenceDoubleValue_e.swMaterialPropertyDensity,
                    (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified);

            private IComponent2 TryGetComponent(IBody2 body)
                => (body.IGetFirstFace() as IEntity).IGetComponent2();

            private void CalculateMassProperties(IModelDoc2 model, bool useSystemUnits,
                MathTransform transform, swMassPropertyMoment_e momentType)
            {
                var massPrps = (IMassProperty)model.Extension.CreateMassProperty();

                massPrps.UseSystemUnits = useSystemUnits;

                if (transform != null)
                {
                    if (!massPrps.SetCoordinateSystem(transform))
                    {
                        throw new Exception("Failed to set coordinate system");
                    }
                }

                CenterOfGravity = new Point((double[])massPrps.CenterOfMass);
                SurfaceArea = massPrps.SurfaceArea;
                Volume = massPrps.Volume;
                Mass = massPrps.Mass;
                Density = massPrps.Density;

                PrincipalAxesOfInertia
                        = new PrincipalAxesOfInertia(
                            new Vector((double[])massPrps.PrincipleAxesOfInertia[0]),
                            new Vector((double[])massPrps.PrincipleAxesOfInertia[1]),
                            new Vector((double[])massPrps.PrincipleAxesOfInertia[2]));

                PrincipalMomentOfInertia = new PrincipalMomentOfInertia((double[])massPrps.PrincipleMomentsOfInertia);

                var moi = (double[])massPrps.GetMomentOfInertia((int)momentType);

                MomentOfInertia = new MomentOfInertia(
                        new Vector(moi[0], moi[1], moi[2]),
                        new Vector(moi[3], moi[4], moi[5]),
                        new Vector(moi[6], moi[7], moi[8]));
            }

            private string GetFullName(ISwDocument doc, ISwSolidBody body)
            {
                doc.Model.ISelectionManager.GetSelectByIdSpecification(body.Body, out string selectByIdStr, out _, out _);
                return selectByIdStr;
            }

            //NOTE: this function is not tested and not yet used - once implemented - test
            //private IComponent2 GetCommonParent(IComponent2[] comps) 
            //{
            //    IComponent2 curCommonParent = null;

            //    var parentHierarchyLazy = new Lazy<List<IComponent2>>(()=> 
            //    {
            //        var res = new List<IComponent2>();

            //        var curParent = curCommonParent.GetParent();

            //        while (curParent != null)
            //        {
            //            res.Add(curParent);
            //            curParent = curParent.GetParent();
            //        }

            //        return res;
            //    });

            //    foreach (var comp in comps) 
            //    {
            //        var thisParent = comp.GetParent();

            //        if (thisParent == null)
            //        {
            //            return null;
            //        }
            //        else 
            //        {
            //            if (curCommonParent == null)
            //            {
            //                curCommonParent = thisParent;
            //            }
            //            else 
            //            {
            //                if (curCommonParent != thisParent) 
            //                {
            //                    int curHierIndex = -1;

            //                    do
            //                    {
            //                        thisParent = thisParent.GetParent();

            //                        if (thisParent == null) 
            //                        {
            //                            return null;
            //                        }

            //                        curHierIndex = parentHierarchyLazy.Value.IndexOf(thisParent);

            //                        if (curHierIndex != - 1) 
            //                        {
            //                            parentHierarchyLazy.Value.RemoveRange(0, curHierIndex + 1);
            //                        }

            //                    } while (curHierIndex == -1);

            //                    curCommonParent = thisParent;
            //                    parentHierarchyLazy = null;
            //                }
            //            }
            //        }
            //    }

            //    return curCommonParent;
            //}
        }

        public Point CenterOfGravity => m_Creator.Element.CenterOfGravity;
        public double SurfaceArea => m_Creator.Element.SurfaceArea;
        public double Volume => m_Creator.Element.Volume;
        public double Mass => m_Creator.Element.Mass;
        public double Density => m_Creator.Element.Density;

        public PrincipalAxesOfInertia PrincipalAxesOfInertia
            => m_Creator.Element.PrincipalAxesOfInertia;

        public PrincipalMomentOfInertia PrincipalMomentOfInertia
            => m_Creator.Element.PrincipalMomentOfInertia;

        public MomentOfInertia MomentOfInertia
            => m_Creator.Element.MomentOfInertia;

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

        public bool IsCommitted => m_Creator.IsCreated;

        protected readonly SwDocument3D m_Doc;
        protected readonly IMathUtility m_MathUtils;

        protected readonly ElementCreator<MassPropertyHandler> m_Creator;

        internal SwLegacyMassProperty(SwDocument3D doc, IMathUtility mathUtils)
        {
            m_Doc = doc;
            m_MathUtils = mathUtils;

            m_Creator = new ElementCreator<MassPropertyHandler>(CreateMassProperty, null, false);

            UserUnits = false;
        }

        private MassPropertyHandler CreateMassProperty(CancellationToken cancellationToken)
        {
            var handler = new MassPropertyHandler(m_Doc);

            UpdateScope(handler);

            return handler;
        }

        protected virtual void UpdateScope(MassPropertyHandler handler)
        {
            IMathTransform transform = null;

            if (RelativeTo != null)
            {
                transform = m_MathUtils.ToMathTransform(RelativeTo);
            }

            handler.Update(Scope?.Cast<ISwSolidBody>().ToArray(),
                !UserUnits, !VisibleOnly, (MathTransform)transform);
        }

        public void Commit(CancellationToken cancellationToken)
            => m_Creator.Create(cancellationToken);

        public void Dispose()
        {
        }
    }

    internal class SwAssemblyLegacyMassProperty : SwLegacyMassProperty, ISwAssemblyMassProperty
    {
        internal SwAssemblyLegacyMassProperty(SwAssembly assm, IMathUtility mathUtils) : base(assm, mathUtils)
        {
        }
        
        IXComponent[] IAssemblyEvaluation.Scope
        {
            get => m_Creator.CachedProperties.Get<IXComponent[]>(nameof(Scope) + "_Components");
            set
            {
                m_Creator.CachedProperties.Set(value, nameof(Scope) + "_Components");

                if (IsCommitted)
                {
                    UpdateScope(m_Creator.Element);
                }
            }
        }

        protected override void UpdateScope(MassPropertyHandler handler)
        {
            IMathTransform transform = null;

            if (RelativeTo != null)
            {
                transform = m_MathUtils.ToMathTransform(RelativeTo);
            }

            var comps = ((IAssemblyEvaluation)this).Scope;

            if (comps != null)
            {
                handler.Update(comps.Cast<ISwComponent>().ToArray(),
                    !UserUnits, !VisibleOnly, (MathTransform)transform);
            }
            else 
            {
                handler.Update(Scope?.Cast<ISwSolidBody>().ToArray(),
                    !UserUnits, !VisibleOnly, (MathTransform)transform);
            }
        }
    }
}
