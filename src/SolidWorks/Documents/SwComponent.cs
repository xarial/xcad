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
using System.Linq;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XCad.Features.CustomFeature;
using Xarial.XCad.Geometry;
using Xarial.XCad.SolidWorks.Documents.Exceptions;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Geometry;

namespace Xarial.XCad.SolidWorks.Documents
{
    public class SwComponent : SwSelObject, IXComponent
    {
        IXDocument3D IXComponent.Document => Document;
        IXComponentRepository IXComponent.Children => Children;
        TSelObject IXObjectContainer.ConvertObject<TSelObject>(TSelObject obj) => ConvertObjectBoxed(obj) as TSelObject;

        public IComponent2 Component { get; }

        private readonly SwAssembly m_ParentAssembly;

        public SwComponentCollection Children { get; }

        internal SwComponent(IComponent2 comp, SwAssembly parentAssembly) : base(comp)
        {
            m_ParentAssembly = parentAssembly;
            Component = comp;
            Children = new SwChildComponentsCollection(parentAssembly, comp);
            Features = new ComponentFeatureRepository(parentAssembly, comp);
            Bodies = new SwComponentBodyCollection(comp, parentAssembly);
        }

        public string Name
        {
            get => Component.Name2;
            set => Component.Name2 = value;
        }

        public SwDocument3D Document
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
                    throw new ComponentNotLoadedException(Name);
                }
            }
        }

        public bool IsResolved
        {
            get 
            {
                var state = Component.GetSuppression2();

                return state == (int)swComponentSuppressionState_e.swComponentResolved
                    || state == (int)swComponentSuppressionState_e.swComponentFullyResolved;
            } 
        }
                          
        public IXFeatureRepository Features { get; }

        public IXBodyRepository Bodies { get; }

        public override void Select(bool append)
        {
            if(!Component.Select4(append, null, false)) 
            {
                throw new Exception("Failed to select component");
            }
        }

        public TSelObject ConvertObject<TSelObject>(TSelObject obj) 
            where TSelObject : SwSelObject
        {
            return (TSelObject)ConvertObjectBoxed(obj);
        }

        private SwSelObject ConvertObjectBoxed(object obj) 
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

    internal class ComponentFeatureRepository : IXFeatureRepository
    {
        IXFeature IXRepository<IXFeature>.this[string name] => this[name];

        private readonly SwAssembly m_Assm;
        private readonly IComponent2 m_Comp;

        public ComponentFeatureRepository(SwAssembly assm, IComponent2 comp) 
        {
            m_Assm = assm;
            m_Comp = comp;
        }
        
        public int Count => throw new NotImplementedException();

        public void AddRange(IEnumerable<IXFeature> ents)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<IXFeature> GetEnumerator() => new ComponentFeatureEnumerator(m_Assm, m_Comp);

        public IXSketch2D PreCreate2DSketch()
        {
            throw new NotImplementedException();
        }

        public IXSketch3D PreCreate3DSketch()
        {
            throw new NotImplementedException();
        }

        public IXCustomFeature PreCreateCustomFeature()
        {
            throw new NotImplementedException();
        }

        public IXCustomFeature<TParams> PreCreateCustomFeature<TParams>() where TParams : class, new()
        {
            throw new NotImplementedException();
        }

        public void RemoveRange(IEnumerable<IXFeature> ents)
        {
            throw new NotImplementedException();
        }

        public SwFeature this[string name]
        {
            get
            {
                if (TryGet(name, out IXFeature ent))
                {
                    return (SwFeature)ent;
                }
                else
                {
                    throw new NullReferenceException($"Feature '{name}' is not found");
                }
            }
        }

        public bool TryGet(string name, out IXFeature ent)
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

        void IXFeatureRepository.CreateCustomFeature<TDef, TParams, TPage>()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal class ComponentFeatureEnumerator : FeatureEnumerator
    {
        private readonly IComponent2 m_Comp;

        public ComponentFeatureEnumerator(SwDocument rootDoc, IComponent2 comp) : base(rootDoc)
        {
            m_Comp = comp;
            Reset();
        }

        protected override IFeature GetFirstFeature() => m_Comp.FirstFeature();
    }

    internal class SwComponentBodyCollection : SwBodyCollection
    {
        private IComponent2 m_Comp;

        public SwComponentBodyCollection(IComponent2 comp, SwDocument rootDoc) : base(rootDoc)
        {
            m_Comp = comp;
        }

        protected override IEnumerable<IBody2> GetSwBodies()
            => (m_Comp.GetBodies3((int)swBodyType_e.swAllBodies, out _) as object[])?.Cast<IBody2>();
    }

    internal class SwChildComponentsCollection : SwComponentCollection
    {
        private readonly IComponent2 m_Comp;

        public SwChildComponentsCollection(SwAssembly assm, IComponent2 comp) : base(assm)
        {
            m_Comp = comp;
        }

        protected override IComponent2 GetRootComponent() => m_Comp;
    }
}
