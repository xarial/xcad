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
using System.Linq;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XCad.Features.CustomFeature;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features.CustomFeature;
using Xarial.XCad.SolidWorks.Features.CustomFeature.Toolkit;
using Xarial.XCad.Toolkit.CustomFeature;

namespace Xarial.XCad.SolidWorks.Features
{
    public interface ISwFeatureManager : IXFeatureRepository
    {
        new ISwFeature this[string name] { get; }
    }

    /// <inheritdoc/>
    internal class SwFeatureManager : ISwFeatureManager
    {
        private IFeatureManager FeatMgr => Document.Model.FeatureManager;

        private readonly MacroFeatureParametersParser m_ParamsParser;

        private readonly ISwApplication m_App;
        internal SwDocument Document { get; }

        public int Count => FeatMgr.GetFeatureCount(false);

        IXFeature IXRepository<IXFeature>.this[string name] => this[name];

        public ISwFeature this[string name]
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

            switch (Document.Model)
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
                ent = Document.CreateObjectFromDispatch<SwFeature>(feat);
                return true;
            }
            else
            {
                ent = null;
                return false;
            }
        }

        internal SwFeatureManager(SwDocument doc, ISwApplication app)
        {
            m_App = app;
            Document = doc;
            m_ParamsParser = new MacroFeatureParametersParser(app);
        }

        public virtual void AddRange(IEnumerable<IXFeature> feats)
        {
            if (feats == null)
            {
                throw new ArgumentNullException(nameof(feats));
            }

            foreach (SwFeature feat in feats)
            {
                feat.Commit();
            }
        }

        public IXSketch2D PreCreate2DSketch() => new SwSketch2D(null, Document, m_App, false);
        public IXSketch3D PreCreate3DSketch() => new SwSketch3D(null, Document, m_App, false);
        
        public virtual IEnumerator<IXFeature> GetEnumerator()
        {
            return new DocumentFeatureEnumerator(Document);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IXCustomFeature<TParams> PreCreateCustomFeature<TParams>()
            where TParams : class, new()
            => new SwMacroFeature<TParams>(null, Document, m_App, m_ParamsParser, false);

        public IXCustomFeature PreCreateCustomFeature()
            => new SwMacroFeature(null, Document, m_App, false);

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
            inst.Insert(Document);
        }
    }

    internal class DocumentFeatureEnumerator : FeatureEnumerator
    {
        private readonly IModelDoc2 m_Model;

        public DocumentFeatureEnumerator(ISwDocument rootDoc) : base(rootDoc)
        {
            m_Model = rootDoc.Model;
            Reset();
        }

        protected override IFeature GetFirstFeature() => m_Model.IFirstFeature();
    }

    internal static class SwFeatureManagerExtension 
    {
        internal static SwCutListItem[] GetCutLists(this SwFeatureManager featMgr) 
        {
            var cutLists = new List<SwCutListItem>();

            ISwConfiguration refConf;

            SwDocument3D doc;

            if (featMgr is SwComponentFeatureManager)
            {
                var comp = (featMgr as SwComponentFeatureManager).Component;
                refConf = (ISwConfiguration)comp.ReferencedConfiguration;
                doc = (SwDocument3D)comp.ReferencedDocument;
            }
            else 
            {
                doc = (SwDocument3D)featMgr.Document;
                refConf = doc.Configurations.Active;
            }

            if (doc.DocumentType == swDocumentTypes_e.swDocPART)
            {
                var part = doc.Model as IPartDoc;

                IEnumerable<IBody2> IterateBodies()
                {
                    object bodies;
                    if (featMgr is SwComponentFeatureManager)
                    {
                        bodies = ((SwComponentFeatureManager)featMgr).Component.Component.GetBodies3((int)swBodyType_e.swSolidBody, out _);
                    }
                    else 
                    {
                        bodies = part.GetBodies2((int)swBodyType_e.swSolidBody, false);
                    }

                    return (bodies as object[] ?? new object[0]).Cast<IBody2>();
                }

                if (part.IsWeldment()
                    || IterateBodies().Any(b => b.IsSheetMetal()))
                {
                    foreach (ISwFeature feat in featMgr)
                    {
                        if (feat is SwCutListItem)
                        {
                            var cutList = (SwCutListItem)feat;
                            cutList.SetOwner(doc, refConf);
                            cutLists.Add(cutList);
                        }
                        else if (feat.Feature.GetTypeName2() == "RefPlane")
                        {
                            break;
                        }
                    }
                }
            }

            return cutLists.ToArray();
        }
    }
}