//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Data;
using Xarial.XCad.Data.Delegates;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Features;
using Xarial.XCad.Features.CustomFeature;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Annotations;
using Xarial.XCad.SolidWorks.Data;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit;
using Xarial.XCad.Toolkit.Services;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwComponent : IXComponent, ISwSelObject
    {
        new ISwComponentCollection Children { get; }
        new ISwDocument3D ReferencedDocument { get; }
        new TSelObject ConvertObject<TSelObject>(TSelObject obj)
            where TSelObject : ISwSelObject;
        IComponent2 Component { get; }
        new ISwFeatureManager Features { get; }
        new ISwDimensionsCollection Dimensions { get; }

        /// <summary>
        /// Returns the cached path of the component as stored in SOLIDWORKS
        /// </summary>
        /// <remarks>This path might not correspond to actual file if component is not resolved or document is opened in view only mode. <see cref="IXComponent.Path"/> will return the resolved path</remarks>
        string CachedPath { get; }
    }

    public interface ISwPartComponent : ISwComponent, IXPartComponent
    {
        new ISwPart ReferencedDocument { get; set; }
    }

    public interface ISwAssemblyComponent : ISwComponent, IXAssemblyComponent
    {
        new ISwAssembly ReferencedDocument { get; set; }
    }

    internal abstract class SwComponentEditor : IEditor<SwComponent>
    {
        public SwComponent Target { get; set; }

        private readonly ISwAssembly m_Assm;

        internal SwComponentEditor(ISwAssembly assm, SwComponent comp)
        {
            m_Assm = assm;
            Target = comp;

            if (Target.Component.Select4(false, null, false))
            {
                StartEdit(m_Assm, Target);
            }
            else
            {
                throw new Exception("Failed to select component for editing");
            }
        }

        public bool Cancel
        {
            get => false;
            set => throw new NotSupportedException("This operation cannot be cancelled");
        }

        protected abstract void StartEdit(ISwAssembly assm, SwComponent comp);

        public void Dispose()
        {
            m_Assm.Model.ClearSelection2(true);
            m_Assm.Assembly.EditAssembly();
        }
    }

    internal class SwPartComponentEditor : SwComponentEditor
    {
        internal SwPartComponentEditor(ISwAssembly assm, SwPartComponent comp) : base(assm, comp)
        {
        }

        protected override void StartEdit(ISwAssembly assm, SwComponent comp)
        {
            int inf = -1;
            var res = (swEditPartCommandStatus_e)assm.Assembly.EditPart2(true, true, ref inf);

            if (res != swEditPartCommandStatus_e.swEditPartSuccessful)
            {
                throw new Exception($"Failed to edit component: {res}");
            }
        }
    }

    internal class SwAssemblyComponentEditor : SwComponentEditor
    {
        public SwAssemblyComponentEditor(ISwAssembly assm, SwAssemblyComponent comp) : base(assm, comp)
        {
        }

        protected override void StartEdit(ISwAssembly assm, SwComponent comp)
        {
            assm.Assembly.EditAssembly();
        }
    }

    [DebuggerDisplay("{" + nameof(FullName) + "}")]
    internal abstract class SwComponent : SwSelObject, ISwComponent
    {
        IXDocument3D IXComponent.ReferencedDocument { get => ReferencedDocument; set => ReferencedDocument = (ISwDocument3D)value; }
        IXComponentRepository IXComponent.Children => Children;
        IXFeatureRepository IXComponent.Features => Features;
        TSelObject IXObjectContainer.ConvertObject<TSelObject>(TSelObject obj) => ConvertObjectBoxed(obj) as TSelObject;
        IXDimensionRepository IDimensionable.Dimensions => Dimensions;

        public IComponent2 Component => m_Creator.Element;

        internal SwAssembly RootAssembly { get; }

        public ISwComponentCollection Children { get; }

        private readonly IFilePathResolver m_FilePathResolver;

        private readonly Lazy<ISwFeatureManager> m_FeaturesLazy;
        private readonly Lazy<ISwDimensionsCollection> m_DimensionsLazy;

        public override object Dispatch => Component;

        private readonly IMathUtility m_MathUtils;

        public override bool IsCommitted => m_Creator.IsCreated;

        private readonly ElementCreator<IComponent2> m_Creator;

        internal SwComponent(IComponent2 comp, SwAssembly rootAssembly, ISwApplication app) : base(comp, rootAssembly, app)
        {
            RootAssembly = rootAssembly;
            m_Creator = new ElementCreator<IComponent2>(CreateComponent, comp, comp != null);
            Children = new SwChildComponentsCollection(rootAssembly, this);
            m_FeaturesLazy = new Lazy<ISwFeatureManager>(() => new SwComponentFeatureManager(this, rootAssembly, app, new Context(this)));
            m_DimensionsLazy = new Lazy<ISwDimensionsCollection>(() => new SwFeatureManagerDimensionsCollection(Features, new Context(this)));

            m_MathUtils = app.Sw.IGetMathUtility();

            Bodies = new SwComponentBodyCollection(comp, rootAssembly);

            m_FilePathResolver = ((SwApplication)OwnerApplication).Services.GetService<IFilePathResolver>();
        }

        public string Name
        {
            get
            {
                var fullName = FullName;
                return fullName.Substring(fullName.LastIndexOf('/') + 1);
            }
            set => Component.Name2 = value;
        }

        public string FullName
        {
            get
            {
                if (IsCommitted)
                {
                    return Component.Name2;
                }
                else 
                {
                    var path = ReferencedDocument?.Path;

                    if (!string.IsNullOrEmpty(path)) 
                    {
                        return Path.GetFileName(path);
                    }

                    return path;
                }
            }
        }

        public ISwDocument3D ReferencedDocument
        {
            get
            {
                if (IsCommitted)
                {
                    var compModel = Component.IGetModelDoc();

                    //Note: for LDR assembly IGetModelDoc returns the pointer to root assembly
                    if (compModel != null && !RootAssembly.Model.IsOpenedViewOnly())
                    {
                        return (ISwDocument3D)OwnerApplication.Documents[compModel];
                    }
                    else
                    {
                        string path;

                        try
                        {
                            path = GetPath();
                        }
                        catch
                        {
                            path = CachedPath;
                        }

                        if (((SwDocumentCollection)OwnerApplication.Documents).TryFindExistingDocumentByPath(path, out SwDocument doc))
                        {
                            return (ISwDocument3D)doc;
                        }
                        else
                        {
                            return (ISwDocument3D)((SwDocumentCollection)OwnerApplication.Documents).PreCreateFromPath(path);
                        }
                    }
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<ISwDocument3D>();
                }
            }
            set 
            {
                if (IsCommitted)
                {
                    //TODO: implement ReplaceComponent feature
                    throw new CommitedElementReadOnlyParameterException();
                }
                else 
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public ComponentState_e State
        {
            get
            {
                if (IsCommitted)
                {
                    var state = ComponentState_e.Default;

                    var swState = GetSuppressionState();

                    if (swState == swComponentSuppressionState_e.swComponentLightweight
                        || swState == swComponentSuppressionState_e.swComponentFullyLightweight)
                    {
                        state |= ComponentState_e.Lightweight;
                    }
                    else if (swState == swComponentSuppressionState_e.swComponentSuppressed)
                    {
                        state |= ComponentState_e.Suppressed;
                    }
                    else if (swState == swComponentSuppressionState_e.swComponentInternalIdMismatch)
                    {
                        state |= ComponentState_e.SuppressedIdMismatch;
                    }

                    if (RootAssembly.Model.IsOpenedViewOnly()) //Large design review
                    {
                        state |= ComponentState_e.ViewOnly;
                    }

                    if (Component.IsHidden(false))
                    {
                        state |= ComponentState_e.Hidden;
                    }

                    if (Component.ExcludeFromBOM)
                    {
                        if (!Component.IsEnvelope())
                        {
                            state |= ComponentState_e.ExcludedFromBom;
                        }
                    }

                    if (Component.IsEnvelope())
                    {
                        state |= ComponentState_e.Envelope;
                    }

                    if (Component.IsVirtual)
                    {
                        state |= ComponentState_e.Embedded;
                    }

                    if (Component.IsFixed())
                    {
                        state |= ComponentState_e.Fixed;
                    }

                    return state;
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<ComponentState_e>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    var swState = GetSuppressionState();

                    if ((swState == swComponentSuppressionState_e.swComponentSuppressed
                        || swState == swComponentSuppressionState_e.swComponentLightweight
                        || swState == swComponentSuppressionState_e.swComponentFullyLightweight)
                            && !value.HasFlag(ComponentState_e.Lightweight) && !value.HasFlag(ComponentState_e.Suppressed))
                    {
                        if (Component.SetSuppression2((int)swComponentSuppressionState_e.swComponentFullyResolved) != (int)swSuppressionError_e.swSuppressionChangeOk)
                        {
                            throw new Exception("Failed to resolve component state");
                        }
                    }
                    else if (swState != swComponentSuppressionState_e.swComponentSuppressed
                            && value.HasFlag(ComponentState_e.Suppressed))
                    {
                        if (Component.SetSuppression2((int)swComponentSuppressionState_e.swComponentSuppressed) != (int)swSuppressionError_e.swSuppressionChangeOk)
                        {
                            throw new Exception("Failed to suppress component");
                        }
                    }
                    else if (swState != swComponentSuppressionState_e.swComponentFullyLightweight
                            && swState != swComponentSuppressionState_e.swComponentLightweight
                            && value.HasFlag(ComponentState_e.Lightweight))
                    {
                        if (Component.SetSuppression2((int)swComponentSuppressionState_e.swComponentFullyLightweight) != (int)swSuppressionError_e.swSuppressionChangeOk)
                        {
                            throw new Exception("Failed to resolve component state");
                        }
                    }

                    if (RootAssembly.Model.IsOpenedViewOnly() && !value.HasFlag(ComponentState_e.ViewOnly)) //Large design review
                    {
                        throw new Exception("Component cannot be resolved when opened as view only");
                    }

                    EditState(value, () => Component.IsHidden(false), ComponentState_e.Hidden, () => Component.Visible = (int)swComponentVisibilityState_e.swComponentHidden, () => Component.Visible = (int)swComponentVisibilityState_e.swComponentVisible);

                    EditState(value, () => Component.ExcludeFromBOM, ComponentState_e.ExcludedFromBom, () => Component.ExcludeFromBOM = true, () => Component.ExcludeFromBOM = false);

                    if (Component.IsEnvelope() && !value.HasFlag(ComponentState_e.Envelope))
                    {
                        throw new Exception("Envelope state cannot be changed");
                    }

                    EditState(value, () => Component.IsVirtual, ComponentState_e.Embedded,
                        () =>
                        {
                            if (OwnerApplication.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2016))
                            {
                                Component.MakeVirtual2(false);
                            }
                            else if (OwnerApplication.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2013))
                            {
                                Component.MakeVirtual();
                            }
                            else
                            {
                                throw new Exception("Component can only be set to virtual starting from SOLIDWORKS 2013");
                            }
                        },
                        () =>
                        {
                            throw new NotSupportedException("Changing component from virtual is not supported");
                        });

                    EditState(value, () => Component.IsFixed(), ComponentState_e.Fixed,
                        () =>
                        {
                            Select(false);
                            RootAssembly.Assembly.FixComponent();
                        },
                        () =>
                        {
                            Select(false);
                            RootAssembly.Assembly.UnfixComponent();
                        });
                }
                else 
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        private void EditState(ComponentState_e state, Func<bool> getFunc, ComponentState_e type, Action setFunc, Action unsetFunc) 
        {
            var curVal = getFunc();

            if (curVal && !state.HasFlag(type))
            {
                unsetFunc.Invoke();
            }
            else if (!curVal && state.HasFlag(type))
            {
                setFunc.Invoke();
            }
        }

        public ISwFeatureManager Features => m_FeaturesLazy.Value;

        public IXBodyRepository Bodies { get; }

        public string CachedPath => Component.GetPathName();

        private string GetPath()
        {
            var cachedPath = CachedPath;

            var needResolve = RootAssembly.Model.IsOpenedViewOnly()
                || GetSuppressionState() == swComponentSuppressionState_e.swComponentSuppressed;

            if (needResolve)
            {
                return m_FilePathResolver.ResolvePath(RootAssembly.Path, cachedPath);
            }
            else
            {
                return cachedPath;
            }
        }

        public IXConfiguration ReferencedConfiguration 
        {
            get 
            {
                if (IsCommitted)
                {
                    return GetReferencedConfiguration();
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<IXConfiguration>();
                }
            }
            set 
            {
                if (IsCommitted)
                {
                    Component.ReferencedConfiguration = value.Name;

                    if (!string.Equals(Component.ReferencedConfiguration, value.Name, StringComparison.CurrentCultureIgnoreCase)) 
                    {
                        throw new Exception("Failed to change referenced configuration");
                    }
                }
                else 
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        protected abstract IXConfiguration GetReferencedConfiguration();

        public System.Drawing.Color? Color
        {
            get => SwColorHelper.GetColor(null,
                (o, c) => Component.GetMaterialPropertyValues2((int)o, c) as double[]);
            set => SwColorHelper.SetColor(value, null,
                (m, o, c) => Component.SetMaterialPropertyValues2(m, (int)o, c),
                (o, c) => Component.RemoveMaterialProperty2((int)o, c));
        }

        public ISwDimensionsCollection Dimensions => m_DimensionsLazy.Value;

        public TransformMatrix Transformation 
        {
            get
            {
                if (IsCommitted)
                {
                    return Component.Transform2.ToTransformMatrix();
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<TransformMatrix>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    Component.Transform2 = (MathTransform)m_MathUtils.ToMathTransform(value);
                }
                else 
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public override void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);

        internal override void Select(bool append, ISelectData selData)
        {
            if(!Component.Select4(append, (SelectData)selData, false)) 
            {
                throw new Exception("Failed to select component");
            }
        }

        public TSelObject ConvertObject<TSelObject>(TSelObject obj) 
            where TSelObject : ISwSelObject
        {
            return (TSelObject)ConvertObjectBoxed(obj);
        }

        private ISwSelObject ConvertObjectBoxed(object obj) 
        {
            if (obj is SwSelObject)
            {
                var disp = (obj as SwSelObject).Dispatch;
                var corrDisp = Component.GetCorresponding(disp);

                if (corrDisp != null)
                {
                    return RootAssembly.CreateObjectFromDispatch<ISwSelObject>(corrDisp);
                }
                else
                {
                    throw new Exception("Failed to convert the pointer of the object");
                }
            }
            else
            {
                throw new InvalidCastException("Object is not SOLIDWORKS object");
            }
        }

        internal swComponentSuppressionState_e GetSuppressionState()
        {
            if (RootAssembly.OwnerApplication.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2019))
            {
                return (swComponentSuppressionState_e)Component.GetSuppression2();
            }
            else
            {
                return (swComponentSuppressionState_e)Component.GetSuppression();
            }
        }

        public abstract IEditor<IXComponent> Edit();

        internal IComponent2 BatchComponentBuffer { get; set; }

        private IComponent2 CreateComponent(CancellationToken cancellationToken)
        {
            if (BatchComponentBuffer == null)
            {
                SwComponentCollection.BatchAdd(RootAssembly, new SwComponent[] { this }, false);
            }

            var batchComp = BatchComponentBuffer;

            BatchComponentBuffer = null;

            return batchComp;
        }
    }

    internal class SwPartComponent : SwComponent, ISwPartComponent
    {
        ISwPart ISwPartComponent.ReferencedDocument { get => (ISwPart)base.ReferencedDocument; set => base.ReferencedDocument = value; }
        IXPart IXPartComponent.ReferencedDocument { get => (IXPart)base.ReferencedDocument; set => base.ReferencedDocument = (ISwDocument3D)value; }
        IXPartConfiguration IXPartComponent.ReferencedConfiguration { get => (IXPartConfiguration)base.ReferencedConfiguration; set => base.ReferencedConfiguration = value; }

        internal SwPartComponent(IComponent2 comp, SwAssembly rootAssembly, ISwApplication app) : base(comp, rootAssembly, app)
        {
        }

        public override IEditor<IXComponent> Edit() => new SwPartComponentEditor(RootAssembly, this);

        protected override IXConfiguration GetReferencedConfiguration() => new SwPartComponentConfiguration(this, OwnerApplication);
    }

    internal class SwAssemblyComponent : SwComponent, ISwAssemblyComponent
    {
        ISwAssembly ISwAssemblyComponent.ReferencedDocument { get => (ISwAssembly)base.ReferencedDocument; set => base.ReferencedDocument = value; }
        IXAssembly IXAssemblyComponent.ReferencedDocument { get => (IXAssembly)base.ReferencedDocument; set => base.ReferencedDocument = (ISwDocument3D)value; }
        IXAssemblyConfiguration IXAssemblyComponent.ReferencedConfiguration { get => (IXAssemblyConfiguration)base.ReferencedConfiguration; set => base.ReferencedConfiguration = value; }

        internal SwAssemblyComponent(IComponent2 comp, SwAssembly rootAssembly, ISwApplication app) : base(comp, rootAssembly, app)
        {
        }

        public override IEditor<IXComponent> Edit() => new SwAssemblyComponentEditor(RootAssembly, this);

        protected override IXConfiguration GetReferencedConfiguration() => new SwAssemblyComponentConfiguration(this, OwnerApplication);
    }

    internal class SwComponentFeatureManager : SwFeatureManager
    {
        private readonly SwAssembly m_Assm;
        internal SwComponent Component { get; }

        public SwComponentFeatureManager(SwComponent comp, SwAssembly assm, ISwApplication app, Context context) 
            : base(assm, app, context)
        {
            m_Assm = assm;
            Component = comp;
        }

        public override void AddRange(IEnumerable<IXFeature> feats, CancellationToken cancellationToken)
        {
            using (var editor = Component.Edit()) 
            {
                base.AddRange(feats, cancellationToken);
            }
        }

        public override IEnumerator<IXFeature> GetEnumerator() => new ComponentFeatureEnumerator(m_Assm, GetFirstFeature(), new Context(Component));

        protected internal override IFeature GetFirstFeature() => Component.Component.FirstFeature();

        public override bool TryGet(string name, out IXFeature ent)
        {
            var swFeat = Component.Component.FeatureByName(name);

            if (swFeat != null)
            {
                var feat = m_Assm.CreateObjectFromDispatch<SwFeature>(swFeat);
                feat.SetContext(m_Context);
                ent = feat;
                return true;
            }
            else
            {
                ent = null;
                return false;
            }
        }
    }

    internal class ComponentFeatureEnumerator : FeatureEnumerator
    {
        public ComponentFeatureEnumerator(ISwDocument rootDoc, IFeature firstFeat, Context context) : base(rootDoc, firstFeat, context)
        {
            Reset();
        }
    }

    internal class SwComponentBodyCollection : SwBodyCollection
    {
        private readonly IComponent2 m_Comp;

        public SwComponentBodyCollection(IComponent2 comp, ISwDocument rootDoc) : base(rootDoc)
        {
            m_Comp = comp;
        }

        protected override IEnumerable<IBody2> GetSwBodies()
            => (m_Comp.GetBodies3((int)swBodyType_e.swAllBodies, out _) as object[])?.Cast<IBody2>();
    }

    internal class SwChildComponentsCollection : SwComponentCollection
    {
        private readonly SwComponent m_Comp;

        public SwChildComponentsCollection(SwAssembly rootAssm, SwComponent comp) : base(rootAssm)
        {
            m_Comp = comp;
        }

        protected override int GetTotalChildrenCount()
        {
            if (m_Comp.GetSuppressionState() != swComponentSuppressionState_e.swComponentSuppressed)
            {
                var refModel = m_Comp.Component.GetModelDoc2();

                if (refModel is IAssemblyDoc)
                {
                    return (refModel as IAssemblyDoc).GetComponentCount(false);
                }
                else if (refModel == null)
                {
                    throw new Exception("Cannot retrieve the total count of chidren of the unloaded document");
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        protected override IEnumerable<IComponent2> GetChildren()
        {
            if (m_Comp.GetSuppressionState() != swComponentSuppressionState_e.swComponentSuppressed)
            {
                return (m_Comp.Component.GetChildren() as object[])?.Cast<IComponent2>();
            }
            else 
            {
                return Enumerable.Empty<IComponent2>();
            }
        }

        protected override int GetChildrenCount() => m_Comp.Component.IGetChildrenCount();
    }
}
