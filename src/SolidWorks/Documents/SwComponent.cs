//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
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
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Data;
using Xarial.XCad.Data.Delegates;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Features;
using Xarial.XCad.Features.CustomFeature;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
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

    [DebuggerDisplay("{" + nameof(Name) + "}")]
    internal class SwComponent : SwSelObject, ISwComponent
    {
        IXDocument3D IXComponent.ReferencedDocument => ReferencedDocument;
        IXComponentRepository IXComponent.Children => Children;
        IXFeatureRepository IXComponent.Features => Features;
        TSelObject IXObjectContainer.ConvertObject<TSelObject>(TSelObject obj) => ConvertObjectBoxed(obj) as TSelObject;
        IXDimensionRepository IXComponent.Dimensions => Dimensions;

        public IComponent2 Component { get; }

        private readonly SwAssembly m_RootAssembly;

        public ISwComponentCollection Children { get; }

        private readonly IFilePathResolver m_FilePathResolver;

        private readonly Lazy<ISwFeatureManager> m_FeaturesLazy;
        private readonly Lazy<ISwDimensionsCollection> m_DimensionsLazy;

        public override object Dispatch => Component;

        private readonly IMathUtility m_MathUtils;

        internal SwComponent(IComponent2 comp, SwAssembly rootAssembly, ISwApplication app) : base(comp, rootAssembly, app)
        {
            m_RootAssembly = rootAssembly;
            Component = comp;
            Children = new SwChildComponentsCollection(rootAssembly, this);
            m_FeaturesLazy = new Lazy<ISwFeatureManager>(() => new SwComponentFeatureManager(this, rootAssembly, app));
            m_DimensionsLazy = new Lazy<ISwDimensionsCollection>(() => new SwFeatureManagerDimensionsCollection(Features));

            m_MathUtils = app.Sw.IGetMathUtility();

            Bodies = new SwComponentBodyCollection(comp, rootAssembly);

            m_FilePathResolver = ((SwApplication)OwnerApplication).Services.GetService<IFilePathResolver>();
        }

        public string Name
        {
            get => Component.Name2;
            set => Component.Name2 = value;
        }

        public ISwDocument3D ReferencedDocument
        {
            get
            {
                var compModel = Component.IGetModelDoc();

                //Note: for LDR assembly IGetModelDoc returns the pointer to root assembly
                if (compModel != null && !m_RootAssembly.Model.IsOpenedViewOnly())
                {
                    return (ISwDocument3D)OwnerApplication.Documents[compModel];
                }
                else
                {
                    string path;

                    try
                    {
                        path = Path;
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
        }

        public ComponentState_e State
        {
            get 
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

                if (m_RootAssembly.Model.IsOpenedViewOnly()) //Large design review
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
                               
                return state;
            }
            set 
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

                if (m_RootAssembly.Model.IsOpenedViewOnly() && !value.HasFlag(ComponentState_e.ViewOnly)) //Large design review
                {
                    throw new Exception("Component cannot be resolved when opened as view only");
                }

                if (Component.IsHidden(false) && !value.HasFlag(ComponentState_e.Hidden))
                {
                    Component.Visible = (int)swComponentVisibilityState_e.swComponentVisible;
                }
                else if (!Component.IsHidden(false) && value.HasFlag(ComponentState_e.Hidden))
                {
                    Component.Visible = (int)swComponentVisibilityState_e.swComponentHidden;
                }

                if (Component.ExcludeFromBOM && !value.HasFlag(ComponentState_e.ExcludedFromBom))
                {
                    Component.ExcludeFromBOM = false;
                }
                else if (!Component.ExcludeFromBOM && value.HasFlag(ComponentState_e.ExcludedFromBom))
                {
                    Component.ExcludeFromBOM = true;
                }

                if (Component.IsEnvelope() && !value.HasFlag(ComponentState_e.Envelope))
                {
                    throw new Exception("Envelope state cannot be changed");
                }

                if (!Component.IsVirtual && value.HasFlag(ComponentState_e.Embedded)) 
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
                }
                else if (Component.IsVirtual && !value.HasFlag(ComponentState_e.Embedded))
                {
                    throw new NotSupportedException("Changing component from virtual is not supported");
                }
            }
        }

        public ISwFeatureManager Features => m_FeaturesLazy.Value;

        public IXBodyRepository Bodies { get; }

        public string CachedPath => Component.GetPathName();

        public string Path 
        {
            get 
            {
                var cachedPath = CachedPath;

                var needResolve = m_RootAssembly.Model.IsOpenedViewOnly() 
                    || GetSuppressionState() == swComponentSuppressionState_e.swComponentSuppressed;

                if (needResolve)
                {
                    return m_FilePathResolver.ResolvePath(m_RootAssembly.Path, cachedPath);
                }
                else 
                {
                    return cachedPath;
                }
            }
        }

        public IXConfiguration ReferencedConfiguration
            => new SwComponentConfiguration(this, OwnerApplication);

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
            get => Component.Transform2.ToTransformMatrix();
            set => Component.Transform2 = (MathTransform)m_MathUtils.ToMathTransform(value);
        }

        public override void Select(bool append)
        {
            if(!Component.Select4(append, null, false)) 
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
                    return m_RootAssembly.CreateObjectFromDispatch<ISwSelObject>(corrDisp);
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
            if (m_RootAssembly.OwnerApplication.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2019))
            {
                return (swComponentSuppressionState_e)Component.GetSuppression2();
            }
            else
            {
                return (swComponentSuppressionState_e)Component.GetSuppression();
            }
        }
    }
    
    internal class SwComponentFeatureManager : SwFeatureManager
    {
        private readonly SwAssembly m_Assm;
        internal SwComponent Component { get; }

        public SwComponentFeatureManager(SwComponent comp, SwAssembly assm, ISwApplication app) 
            : base(assm, app)
        {
            m_Assm = assm;
            Component = comp;
        }

        public override void AddRange(IEnumerable<IXFeature> feats)
        {
            try
            {
                if (Component.Component.Select4(false, null, false))
                {
                    var isAssm = string.Equals(Path.GetExtension(Component.Component.GetPathName()),
                        ".sldasm", StringComparison.CurrentCultureIgnoreCase);

                    if (isAssm)
                    {
                        m_Assm.Assembly.EditAssembly();
                    }
                    else 
                    {
                        int inf = -1;
                        m_Assm.Assembly.EditPart2(true, false, ref inf);
                    }

                    base.AddRange(feats);
                }
                else 
                {
                    throw new Exception("Failed to select component to insert features");
                }
            }
            catch 
            {
                throw;
            }
            finally
            {
                m_Assm.Model.ClearSelection2(true);
                m_Assm.Assembly.EditAssembly();
            }
        }

        public override IEnumerator<IXFeature> GetEnumerator() => new ComponentFeatureEnumerator(m_Assm, Component.Component);
        
        public override bool TryGet(string name, out IXFeature ent)
        {
            var feat = Component.Component.FeatureByName(name);

            if (feat != null)
            {
                ent = m_Assm.CreateObjectFromDispatch<SwFeature>(feat);
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
        private readonly IComponent2 m_Comp;

        public ComponentFeatureEnumerator(ISwDocument rootDoc, IComponent2 comp) : base(rootDoc)
        {
            m_Comp = comp;
            Reset();
        }

        protected override IFeature GetFirstFeature() => m_Comp.FirstFeature();
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

        public SwChildComponentsCollection(ISwAssembly rootAssm, SwComponent comp) : base(rootAssm)
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
