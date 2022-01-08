﻿//*********************************************************************
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
using System.Runtime.InteropServices;
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
            IFeature swFeat;

            switch (Document.Model)
            {
                case IPartDoc part:
                    swFeat = part.FeatureByName(name) as IFeature;
                    break;

                case IAssemblyDoc assm:
                    swFeat = assm.FeatureByName(name) as IFeature;
                    break;

                case IDrawingDoc drw:
                    swFeat = drw.FeatureByName(name) as IFeature;
                    break;

                default:
                    throw new NotSupportedException();
            }

            if (swFeat != null)
            {
                var feat = Document.CreateObjectFromDispatch<SwFeature>(swFeat);
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

        protected readonly ISwObject m_Context;

        internal SwFeatureManager(SwDocument doc, ISwApplication app, ISwObject context)
        {
            m_App = app;
            Document = doc;
            m_Context = context;
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
            => new DocumentFeatureEnumerator(Document, GetFirstFeature(), Document);

        internal protected virtual IFeature GetFirstFeature() => Document.Model.IFirstFeature();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IXCustomFeature<TParams> PreCreateCustomFeature<TParams>()
            where TParams : class, new()
            => new SwMacroFeature<TParams>(null, Document, m_App, m_ParamsParser, false);

        public IXCustomFeature PreCreateCustomFeature()
            => new SwMacroFeature(null, Document, m_App, false);

        public void RemoveRange(IEnumerable<IXFeature> ents)
        {
            var disps = ents.Cast<SwFeature>().Select(e => new DispatchWrapper(e.Feature)).ToArray();

            if (Document.Model.Extension.MultiSelect2(disps, false, null) == disps.Length)
            {
                if (!Document.Model.Extension.DeleteSelection2((int)swDeleteSelectionOptions_e.swDelete_Absorbed))
                {
                    throw new Exception("Failed to delete features");
                }
            }
            else 
            {
                throw new Exception("Failed to select features for deletion");
            }            
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

        public void Enable(bool enable)
        {
            FeatMgr.EnableFeatureTree = enable;
            FeatMgr.EnableFeatureTreeWindow = enable;
        }
    }

    internal class DocumentFeatureEnumerator : FeatureEnumerator
    {
        public DocumentFeatureEnumerator(ISwDocument rootDoc, IFeature firstFeat, ISwObject context) : base(rootDoc, firstFeat, context)
        {
            Reset();
        }
    }

    internal static class SwFeatureManagerExtension 
    {
        internal static IEnumerable<SwCutListItem> IterateCutLists(this SwFeatureManager featMgr, ISwDocument3D parent, ISwConfiguration refConf)
        {
            foreach (var feat in FeatureEnumerator.IterateFeatures(featMgr.GetFirstFeature(), false)) 
            {
                if (feat.GetTypeName2() == "SolidBodyFolder") 
                {
                    foreach (var subFeat in FeatureEnumerator.IterateSubFeatures(feat, true)) 
                    {
                        if (subFeat.GetTypeName2() == "CutListFolder") 
                        {
                            var cutListFolder = (IBodyFolder)subFeat.GetSpecificFeature2();

                            if (cutListFolder.GetBodyCount() > 0)//no bodies for hidden cut-lists (not available in the specific configuration)
                            {
                                var cutList = featMgr.Document.CreateObjectFromDispatch<SwCutListItem>(subFeat);
                                cutList.SetParent(parent, refConf);
                                yield return cutList;
                            }
                        }
                    }

                    break;
                }
                else if (feat.GetTypeName2() == "RefPlane")
                {
                    break;
                }
            }

            yield break;
        }
    }
}