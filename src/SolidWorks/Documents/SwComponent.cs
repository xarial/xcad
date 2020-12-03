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
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Features;
using Xarial.XCad.Features.CustomFeature;
using Xarial.XCad.Geometry;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.Toolkit;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwComponent : IXComponent, ISwSelObject 
    {
        new ISwComponentCollection Children { get; }
        new ISwDocument3D Document { get; }
        new TSelObject ConvertObject<TSelObject>(TSelObject obj)
            where TSelObject : ISwSelObject;

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
        TSelObject IXObjectContainer.ConvertObject<TSelObject>(TSelObject obj) => ConvertObjectBoxed(obj) as TSelObject;

        public IComponent2 Component { get; }

        private readonly SwAssembly m_ParentAssembly;

        public ISwComponentCollection Children { get; }

        private readonly IFilePathResolver m_FilePathResolver;

        internal SwComponent(IComponent2 comp, SwAssembly parentAssembly) : base(comp)
        {
            m_ParentAssembly = parentAssembly;
            Component = comp;
            Children = new SwChildComponentsCollection(parentAssembly, comp);
            Features = new ComponentFeatureRepository(parentAssembly, comp);
            Bodies = new SwComponentBodyCollection(comp, parentAssembly);

            m_FilePathResolver = m_ParentAssembly.App.Services.GetService<IFilePathResolver>();
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

                if (compModel != null)
                {
                    return (SwDocument3D)m_ParentAssembly.App.Documents[compModel];
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

                    if (((SwDocumentCollection)m_ParentAssembly.App.Documents).TryFindExistingDocumentByPath(path, out SwDocument doc))
                    {
                        return (ISwDocument3D)doc;
                    }
                    else 
                    {
                        return (ISwDocument3D)((SwDocumentCollection)m_ParentAssembly.App.Documents).PreCreateFromPath(path);
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
                    state |= ComponentState_e.Rapid;
                }
                else if (swState == (int)swComponentSuppressionState_e.swComponentSuppressed) 
                {
                    state |= ComponentState_e.Suppressed;
                }
                
                if (m_ParentAssembly.Model.IsOpenedViewOnly()) //Large design review
                {
                    state |= ComponentState_e.ViewOnly;
                }

                return state;
            } 
        }
                          
        public IXFeatureRepository Features { get; }

        public IXBodyRepository Bodies { get; }

        public string CachedPath => Component.GetPathName();

        public string Path 
        {
            get 
            {
                var cachedPath = CachedPath;

                var needResolve = m_ParentAssembly.Model.IsOpenedViewOnly() 
                    || Component.GetSuppression2() == (int)swComponentSuppressionState_e.swComponentSuppressed;

                if (needResolve)
                {
                    return m_FilePathResolver.ResolvePath(m_ParentAssembly.Path, cachedPath);
                }
                else 
                {
                    return cachedPath;
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
                    return SwSelObject.FromDispatch(corrDisp, m_ParentAssembly);
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

        public SwChildComponentsCollection(ISwAssembly assm, IComponent2 comp) : base(assm)
        {
            m_Comp = comp;
        }

        protected override IEnumerable<IComponent2> GetChildren()
            => (m_Comp.GetChildren() as object[])?.Cast<IComponent2>();

        protected override int GetChildrenCount() => m_Comp.IGetChildrenCount();
    }
}
