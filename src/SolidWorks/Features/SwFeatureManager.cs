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
        private readonly SwDocument m_Model;

        public int Count => m_FeatMgr.GetFeatureCount(false);

        IXFeature IXRepository<IXFeature>.this[string name] => this[name];

        public SwFeature this[string name]
        {
            get
            {
                IFeature feat;

                switch (m_Model.Model)
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
                    return SwObject.FromDispatch<SwFeature>(feat, m_Model.Model);
                }
                else
                {
                    throw new NullReferenceException("Feature is not found");
                }
            }
        }

        internal SwFeatureManager(SwDocument model, IFeatureManager featMgr, ISldWorks app)
        {
            m_Model = model;
            m_ParamsParser = new MacroFeatureParametersParser(app);
            m_FeatMgr = featMgr;
        }

        public void AddRange(IEnumerable<IXFeature> feats)
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

        public IXSketch2D PreCreate2DSketch()
        {
            return new SwSketch2D(m_Model.Model, null, false);
        }

        public IXSketch3D PreCreate3DSketch()
        {
            return new SwSketch3D(m_Model.Model, null, false);
        }

        public IEnumerator<IXFeature> GetEnumerator()
        {
            return new FeatureEnumerator(m_Model.Model);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IXCustomFeature<TParams> PreCreateCustomFeature<TParams>()
            where TParams : class, new()
        {
            return new SwMacroFeature<TParams>(m_Model, m_FeatMgr, null, m_ParamsParser, false);
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
            inst.Insert(m_Model);
        }
    }

    internal class FeatureEnumerator : IEnumerator<IXFeature>
    {
        public IXFeature Current => SwObject.FromDispatch<SwFeature>(m_CurFeat, m_Model);

        object IEnumerator.Current => Current;

        private readonly IModelDoc2 m_Model;
        private IFeature m_CurFeat;

        //TODO: implement proper handling of sub features

        internal FeatureEnumerator(IModelDoc2 model)
        {
            m_Model = model;
            Reset();
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            m_CurFeat = m_CurFeat.IGetNextFeature();
            return m_CurFeat != null;
        }

        public void Reset()
        {
            m_CurFeat = m_Model.IFirstFeature();
        }
    }
}