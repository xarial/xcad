//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections;
using System.Collections.Generic;
using Xarial.XCad.Base;
using Xarial.XCad.Features;
using Xarial.XCad.Features.CustomFeature;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features.CustomFeature;
using Xarial.XCad.SolidWorks.Features.CustomFeature.Toolkit;
using Xarial.XCad.Toolkit.CustomFeature;

namespace Xarial.XCad.SolidWorks.Features
{
    /// <inheritdoc/>
    public class SwFeatureManager : IXFeatureRepository
    {
        private readonly IFeatureManager m_FeatMgr;
        private readonly MacroFeatureParametersParser m_ParamsParser;
        private readonly SwDocument m_Doc;

        public int Count => m_FeatMgr.GetFeatureCount(false);

        IXFeature IXRepository<IXFeature>.this[string name] => this[name];

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

        public virtual bool TryGet(string name, out IXFeature ent)
        {
            IFeature feat;

            switch (m_Doc.Model)
            {
                case IPartDoc part:
                    feat = part.FeatureByName(name) as IFeature;
                    break;

                case IAssemblyDoc assm:
                    feat = assm.FeatureByName(name) as IFeature;
                    break;

                case IDrawingDoc drw:
                    feat = drw.FeatureByName(name) as IFeature;
                    break;

                default:
                    throw new NotSupportedException();
            }

            if (feat != null)
            {
                ent = SwObject.FromDispatch<SwFeature>(feat, m_Doc);
                return true;
            }
            else
            {
                ent = null;
                return false;
            }
        }

        internal SwFeatureManager(SwDocument doc, IFeatureManager featMgr)
        {
            m_Doc = doc;
            m_ParamsParser = new MacroFeatureParametersParser(doc.SwApp);
            m_FeatMgr = featMgr;
        }

        public virtual void AddRange(IEnumerable<IXFeature> feats)
        {
            if (feats == null)
            {
                throw new ArgumentNullException(nameof(feats));
            }

            foreach (SwFeature feat in feats)
            {
                feat.Create();
            }
        }

        public TSketch PreCreateSketch<TSketch>() where TSketch : class, IXSketchBase
        {
            if (typeof(TSketch).IsAssignableFrom(typeof(IXSketch2D)))
            {
                return new SwSketch2D(m_Doc, null, false) as TSketch;
            }
            else if (typeof(TSketch).IsAssignableFrom(typeof(IXSketch3D)))
            {
                return new SwSketch3D(m_Doc, null, false) as TSketch;
            }
            else 
            {
                throw new Exception("Sketch type is not supported");
            }
        }

        public virtual IEnumerator<IXFeature> GetEnumerator()
        {
            return new DocumentFeatureEnumerator(m_Doc);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IXCustomFeature<TParams> PreCreateCustomFeature<TParams>()
            where TParams : class, new()
        {
            return new SwMacroFeature<TParams>(m_Doc, m_FeatMgr, null, m_ParamsParser, false);
        }

        public IXCustomFeature PreCreateCustomFeature()
        {
            return new SwMacroFeature(m_Doc, m_FeatMgr, null, false);
        }

        public void RemoveRange(IEnumerable<IXFeature> ents)
        {
            //TODO: implement deletion
        }

        /// <inheritdoc/>
        public void CreateCustomFeature<TDef, TParams, TPage>()
            where TParams : class, new()
            where TPage : class, new()
            where TDef : class, IXCustomFeatureDefinition<TParams, TPage>, new()
        {
            var inst = (TDef)CustomFeatureDefinitionInstanceCache.GetInstance(typeof(TDef));
            inst.Insert(m_Doc);
        }
    }

    internal class DocumentFeatureEnumerator : FeatureEnumerator
    {
        private readonly IModelDoc2 m_Model;

        public DocumentFeatureEnumerator(SwDocument rootDoc) : base(rootDoc)
        {
            m_Model = rootDoc.Model;
            Reset();
        }

        protected override IFeature GetFirstFeature() => m_Model.IFirstFeature();
    }
}