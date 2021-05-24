//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Data;
using Xarial.XCad.Data.Delegates;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Features;
using Xarial.XCad.Features.CustomFeature;
using Xarial.XCad.Geometry;
using Xarial.XCad.SolidWorks.Data;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.Toolkit;
using Xarial.XCad.Toolkit.Services;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwComponent : IXComponent, ISwSelObject 
    {
        new ISwComponentCollection Children { get; }
        new ISwDocument3D Document { get; }
        new TSelObject ConvertObject<TSelObject>(TSelObject obj)
            where TSelObject : ISwSelObject;
        IComponent2 Component { get; }
        new ISwFeatureManager Features { get; }

        /// <summary>
        /// Returns the cached path of the component as stored in SOLIDWORKS
        /// </summary>
        /// <remarks>This path might not correspond to actual file if component is not resolved or document is opened in view only mode. <see cref="IXComponent.Path"/> will return the resolved path</remarks>
        string CachedPath { get; }
    }

    internal class SwComponent : SwSelObject, ISwComponent
    {
        IXDocument3D IXComponent.Document => Document;
        IXComponentRepository IXComponent.Children => Children;
        IXFeatureRepository IXComponent.Features => Features;
        TSelObject IXObjectContainer.ConvertObject<TSelObject>(TSelObject obj) => ConvertObjectBoxed(obj) as TSelObject;

        public IComponent2 Component { get; }

        private readonly SwAssembly m_RootAssembly;

        public ISwComponentCollection Children { get; }

        private readonly IFilePathResolver m_FilePathResolver;

        private readonly Lazy<ISwFeatureManager> m_Features;

        public override object Dispatch => Component;

        internal SwComponent(IComponent2 comp, SwAssembly rootAssembly) : base(comp)
        {
            m_RootAssembly = rootAssembly;
            Component = comp;
            Children = new SwChildComponentsCollection(rootAssembly, comp);
            m_Features = new Lazy<ISwFeatureManager>(() => new ComponentFeatureRepository(rootAssembly, comp));
            Bodies = new SwComponentBodyCollection(comp, rootAssembly);

            m_FilePathResolver = m_RootAssembly.App.Services.GetService<IFilePathResolver>();
        }

        public string Name
        {
            get => Component.Name2;
            set => Component.Name2 = value;
        }

        public ISwDocument3D Document
        {
            get
            {
                var compModel = Component.IGetModelDoc();

                //Note: for LDR assembly IGetModelDoc returns the pointer to root assembly
                if (compModel != null && !m_RootAssembly.Model.IsOpenedViewOnly())
                {
                    return (SwDocument3D)m_RootAssembly.App.Documents[compModel];
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

                    if (((SwDocumentCollection)m_RootAssembly.App.Documents).TryFindExistingDocumentByPath(path, out SwDocument doc))
                    {
                        return (ISwDocument3D)doc;
                    }
                    else 
                    {
                        return (ISwDocument3D)((SwDocumentCollection)m_RootAssembly.App.Documents).PreCreateFromPath(path);
                    }
                }
            }
        }

        public ComponentState_e State
        {
            get 
            {
                var state = ComponentState_e.Default;

                var swState = Component.GetSuppression2();

                if (swState == (int)swComponentSuppressionState_e.swComponentLightweight
                    || swState == (int)swComponentSuppressionState_e.swComponentFullyLightweight)
                {
                    state |= ComponentState_e.Lightweight;
                }
                else if (swState == (int)swComponentSuppressionState_e.swComponentSuppressed) 
                {
                    state |= ComponentState_e.Suppressed;
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
                    state |= ComponentState_e.ExcludedFromBom;
                }

                if (Component.IsEnvelope()) 
                {
                    state |= ComponentState_e.Envelope;
                }
                               
                return state;
            }
            set 
            {
                var swState = Component.GetSuppression2();

                if ((swState == (int)swComponentSuppressionState_e.swComponentSuppressed && !value.HasFlag(ComponentState_e.Suppressed))
                    || (swState == (int)swComponentSuppressionState_e.swComponentLightweight
                        || swState == (int)swComponentSuppressionState_e.swComponentFullyLightweight
                        && !value.HasFlag(ComponentState_e.Lightweight)))
                {
                    if (Component.SetSuppression2((int)swComponentSuppressionState_e.swComponentFullyResolved) != (int)swSuppressionError_e.swSuppressionChangeOk) 
                    {
                        throw new Exception("Failed to resovle component state");
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

                if (Component.ExcludeFromBOM && !value.HasFlag(ComponentState_e.ExcludedFromBom))
                {
                    Component.ExcludeFromBOM = false;
                }

                if (Component.IsEnvelope() && !value.HasFlag(ComponentState_e.Envelope))
                {
                    throw new Exception("Envelope state cannot be changed");
                }
            }
        }

        public ISwFeatureManager Features => m_Features.Value;

        public IXBodyRepository Bodies { get; }

        public string CachedPath => Component.GetPathName();

        public string Path 
        {
            get 
            {
                var cachedPath = CachedPath;

                var needResolve = m_RootAssembly.Model.IsOpenedViewOnly() 
                    || Component.GetSuppression2() == (int)swComponentSuppressionState_e.swComponentSuppressed;

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
        {
            get 
            {
                var doc = Document;

                if (doc.IsCommitted)
                {
                    return doc.Configurations[Component.ReferencedConfiguration];
                }
                else 
                {
                    return new SwConfiguration((SwDocument3D)doc, null, false)
                    {
                        Name = Component.ReferencedConfiguration
                    };
                }
            }
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
                    return SwSelObject.FromDispatch(corrDisp, m_RootAssembly);
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
    }
    
    internal class ComponentFeatureRepository : SwFeatureManager
    {
        private readonly SwAssembly m_Assm;
        private readonly IComponent2 m_Comp;

        public ComponentFeatureRepository(SwAssembly assm, IComponent2 comp) 
            : base(assm)
        {
            m_Assm = assm;
            m_Comp = comp;
        }

        public override void AddRange(IEnumerable<IXFeature> feats)
        {
            try
            {
                if (m_Comp.Select4(false, null, false))
                {
                    var isAssm = string.Equals(Path.GetExtension(m_Comp.GetPathName()),
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

        public override IEnumerator<IXFeature> GetEnumerator() => new ComponentFeatureEnumerator(m_Assm, m_Comp);
        
        public override bool TryGet(string name, out IXFeature ent)
        {
            var feat = m_Comp.FeatureByName(name);

            if (feat != null)
            {
                ent = SwObject.FromDispatch<SwFeature>(feat, m_Assm);
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
        private IComponent2 m_Comp;

        public SwComponentBodyCollection(IComponent2 comp, ISwDocument rootDoc) : base(rootDoc)
        {
            m_Comp = comp;
        }

        protected override IEnumerable<IBody2> GetSwBodies()
            => (m_Comp.GetBodies3((int)swBodyType_e.swAllBodies, out _) as object[])?.Cast<IBody2>();
    }

    internal class SwChildComponentsCollection : SwComponentCollection
    {
        private readonly IComponent2 m_Comp;

        public SwChildComponentsCollection(ISwAssembly rootAssm, IComponent2 comp) : base(rootAssm)
        {
            m_Comp = comp;
        }

        protected override int GetTotalChildrenCount()
        {
            if (!!m_Comp.IsSuppressed())
            {
                var refModel = m_Comp.GetModelDoc2();

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
            if (!m_Comp.IsSuppressed())
            {
                return (m_Comp.GetChildren() as object[])?.Cast<IComponent2>();
            }
            else 
            {
                return Enumerable.Empty<IComponent2>();
            }
        }

        protected override int GetChildrenCount() => m_Comp.IGetChildrenCount();
    }
}
