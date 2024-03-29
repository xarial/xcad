﻿//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
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
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XCad.Features.CustomFeature;
using Xarial.XCad.Features.Delegates;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features.CustomFeature;
using Xarial.XCad.SolidWorks.Features.CustomFeature.Toolkit;
using Xarial.XCad.SolidWorks.Sketch;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.CustomFeature;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Utils;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.Features
{
    public interface ISwFeatureManager : IXFeatureRepository
    {
        new ISwFeature this[string name] { get; }
    }

    /// <inheritdoc/>
    internal abstract class SwFeatureManager : ISwFeatureManager
    {
        //NOTE: this event is not raised when feature is added via API
        public event FeatureCreatedDelegate FeatureCreated
        {
            add => m_FeatureCreatedEventsHandler.Attach(value);
            remove => m_FeatureCreatedEventsHandler.Detach(value);
        }

        internal SwDocument Document { get; }

        public int Count
        {
            get
            {
                if (Document.IsCommitted)
                {
                    return FeatMgr.GetFeatureCount(false);
                }
                else 
                {
                    return m_Cache.Count;
                }
            }
        }

        IXFeature IXRepository<IXFeature>.this[string name] => this[name];

        public ISwFeature this[string name] => (ISwFeature)RepositoryHelper.Get(this, name);

        public abstract bool TryGet(string name, out IXFeature ent);

        private IFeatureManager FeatMgr => Document.Model.FeatureManager;

        private readonly SwApplication m_App;

        protected readonly Context m_Context;

        protected readonly EntityCache<IXFeature> m_Cache;

        private readonly FeatureCreatedEventsHandler m_FeatureCreatedEventsHandler;

        internal SwFeatureManager(SwDocument doc, SwApplication app, Context context)
        {
            m_App = app;
            Document = doc;
            m_Context = context;

            m_FeatureCreatedEventsHandler = new FeatureCreatedEventsHandler(doc, app);

            m_Cache = new EntityCache<IXFeature>(doc, this, f => f.Name);
        }

        public abstract void AddRange(IEnumerable<IXFeature> feats, CancellationToken cancellationToken);

        internal void CommitCache(CancellationToken cancellationToken) => m_Cache.Commit(cancellationToken);

        public abstract IEnumerator<IXFeature> GetEnumerator();

        public abstract IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters);

        internal protected abstract IFeature GetFirstFeature();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void RemoveRange(IEnumerable<IXFeature> ents, CancellationToken cancellationToken)
        {
            if (Document.IsCommitted)
            {
                using (var viewFreeze = new UiFreeze(Document))
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
            }
            else 
            {
                m_Cache.RemoveRange(ents, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public void CreateCustomFeature<TDef, TParams, TPage>(TParams data)
            where TParams : class
            where TPage : class
            where TDef : class, IXCustomFeatureDefinition<TParams, TPage>, new()
        {
            var inst = (TDef)CustomFeatureDefinitionInstanceCache.GetInstance(typeof(TDef));
            inst.Insert(Document, data);
        }

        public void Enable(bool enable)
        {
            FeatMgr.EnableFeatureTree = enable;
            FeatMgr.EnableFeatureTreeWindow = enable;
        }

        public T PreCreate<T>() where T : IXFeature
        {
            if (typeof(T).IsAssignableToGenericType(typeof(IXCustomFeature<>)))
            {
                var macroFeatureParamsType = typeof(T).GetArgumentsOfGenericType(typeof(IXCustomFeature<>)).First();
                var feat = SwMacroFeature<object>.CreateSpecificInstance(null, Document, m_App, macroFeatureParamsType);
                return (T)(object)feat;
            }
            else 
            {
                return RepositoryHelper.PreCreate<IXFeature, T>(this,
                    () => new SwSketch2D(default(ISketch), Document, m_App, false),
                    () => new SwSketch3D(default(ISketch), Document, m_App, false),
                    () => new SwMacroFeature(null, Document, m_App, false),
                    () => new SwDumbBody(null, Document, m_App, false),
                    () => new SwPlane(null, Document, m_App, false),
                    () => new SwSketchPicture(default(IFeature), Document, m_App, false));
            }
        }

        protected void CommitFeatures(IEnumerable<IXFeature> feats, CancellationToken cancellationToken)
        {
            if (Document.IsCommitted)
            {
                using (var viewFreeze = new UiFreeze(Document))
                {
                    RepositoryHelper.AddRange(feats, cancellationToken);
                }
            }
            else
            {
                m_Cache.AddRange(feats, cancellationToken);
            }
        }
    }

    internal class SwDocumentFeatureManager : SwFeatureManager
    {
        public SwDocumentFeatureManager(SwDocument doc, SwApplication app, Context context) : base(doc, app, context)
        {
        }

        public override void AddRange(IEnumerable<IXFeature> feats, CancellationToken cancellationToken)
            => CommitFeatures(feats, cancellationToken);

        public override IEnumerator<IXFeature> GetEnumerator()
        {
            if (Document.IsCommitted)
            {
                return new DocumentFeatureEnumerator(Document, GetFirstFeature(), new Context(Document));
            }
            else
            {
                return m_Cache.GetEnumerator();
            }
        }

        public override IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) 
        {
            if (reverseOrder)
            {
                foreach (var swFeat in IterateFeaturesReverse(Document.Model)) 
                {
                    var feat = Document.CreateObjectFromDispatch<ISwFeature>(swFeat);

                    if (RepositoryHelper.MatchesFilters(feat, filters))
                    {
                        yield return feat;
                    }
                }
            }
            else 
            {
                foreach (var ent in RepositoryHelper.FilterDefault(this, filters, reverseOrder)) 
                {
                    yield return ent;
                }
            }
        }

        private IEnumerable<IFeature> IterateFeaturesReverse(IModelDoc2 model)
        {
            int pos = 0;
            var processedFeats = new List<IFeature>();

            IFeature feat;

            do
            {
                feat = model.IFeatureByPositionReverse(pos++);

                if (feat != null)
                {
                    if (feat.GetTypeName2() != "HistoryFolder")
                    {
                        foreach (var subFeat in FeatureEnumerator.IterateSubFeatures(feat, true).Reverse())
                        {
                            if (!processedFeats.Contains(subFeat))
                            {
                                processedFeats.Add(subFeat);
                                yield return subFeat;
                            }
                        }
                    }

                    if (!processedFeats.Contains(feat)) 
                    {
                        processedFeats.Add(feat);
                        yield return feat;
                    }
                }

            } while (feat != null);
        }

        internal protected override IFeature GetFirstFeature() => Document.Model.IFirstFeature();

        public override bool TryGet(string name, out IXFeature ent)
        {
            if (Document.IsCommitted)
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
            else
            {
                return m_Cache.TryGet(name, out ent);
            }
        }
    }

    internal class DocumentFeatureEnumerator : FeatureEnumerator
    {
        public DocumentFeatureEnumerator(ISwDocument rootDoc, IFeature firstFeat, Context context) : base(rootDoc, firstFeat, context)
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