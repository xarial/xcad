//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Toolkit.Services;

namespace Xarial.XCad.SwDocumentManager.Documents
{
    internal class SwDmDocumentDependencies : IXDocumentDependencies
    {
        private class SwDmDependenciesAggregator : DependenciesAggregator<SwDmDocumentDependencies>
        {
            private readonly List<string> m_Cache;

            public SwDmDependenciesAggregator(SwDmDocumentDependencies deps, List<string> cache) : base(deps, true, true)
            {
                m_Cache = cache;
            }

            protected override IXDocument3D[] LoadDependencies(SwDmDocumentDependencies deps) => deps.IterateDependencies(m_Cache).ToArray();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private readonly SwDmDocument m_Doc;

        private readonly List<SwDmDocument3D> m_VirtualDocumentsCache;

        public IXDocument OwnerDocument => m_Doc;

        public bool Cached 
        {
            get => false;
            set 
            {
                if (value == true)
                {
                    throw new NotSupportedException("Document Manager does not allow to return the cached references");
                }
            }
        }

        public IEnumerable<IXDocument3D> All => new SwDmDependenciesAggregator(this, new List<string>());

        internal SwDmDocumentDependencies(SwDmDocument doc) 
        {
            m_Doc = doc;
            m_VirtualDocumentsCache = new List<SwDmDocument3D>();
        }

        public IEnumerator<IXDocument3D> GetEnumerator() => IterateDependencies(new List<string>()).GetEnumerator();

        public void Replace(IXDocument3D source, IXDocument3D target) => m_Doc.Document.ReplaceReference(source.Path, target.Path);

        protected IEnumerable<SwDmDocument3D> IterateDependencies(List<string> cache) 
        {
            var deps = GetRawDependencies(cache);

            if (deps.Any())
            {
                var compsLazy = new Lazy<ISwDmComponent[]>(GetAssemblyComponents);

                for (int i = 0; i < deps.Length; i++)
                {
                    var depPath = deps[i].Item1;
                    var isVirtual = deps[i].Item2;

                    SwDmDocument3D depDoc = null;

                    if (m_Doc.OwnerApplication.Documents.TryGet(depPath, out ISwDmDocument curDoc))
                    {
                        depDoc = (SwDmDocument3D)curDoc;
                    }

                    if (depDoc == null)
                    {
                        if (!isVirtual || !TryFindVirtualDocument(depPath, compsLazy, out depDoc))
                        {
                            try
                            {
                                depDoc = (SwDmDocument3D)m_Doc.OwnerApplication.Documents.PreCreateFromPath(depPath);
                            }
                            catch
                            {
                                depDoc = m_Doc.OwnerApplication.Documents.PreCreate<SwDmDocument3D>();
                                depDoc.Path = depPath;
                            }

                            if (m_Doc.State.HasFlag(DocumentState_e.ReadOnly))
                            {
                                depDoc.State = DocumentState_e.ReadOnly;
                            }
                        }
                    }

                    yield return depDoc;
                }
            }
        }

        private ISwDmComponent[] GetAssemblyComponents()
        {
            if (string.Equals(Path.GetExtension(m_Doc.Path), ".sldasm", StringComparison.CurrentCultureIgnoreCase))
            {
                var activeConfName = m_Doc.Document.ConfigurationManager.GetActiveConfigurationName();
                var conf = (ISwDMConfiguration2)m_Doc.Document.ConfigurationManager.GetConfigurationByName(activeConfName);
                var comps = (object[])conf.GetComponents();
                if (comps != null)
                {
                    return comps.Select(c => m_Doc.CreateObjectFromDispatch<ISwDmComponent>(c)).ToArray();
                }
                else
                {
                    return Array.Empty<ISwDmComponent>();
                }
            }
            else
            {
                throw new Exception("Components can only be extracted from the assembly");
            }
        }

        private bool TryFindVirtualDocument(string filePath, Lazy<ISwDmComponent[]> compsLazy, out SwDmDocument3D virtCompDoc)
        {
            try
            {
                var virtCompFileName = Path.GetFileName(filePath);

                virtCompDoc = m_VirtualDocumentsCache.FirstOrDefault(d => string.Equals(d.Title,
                    virtCompFileName, StringComparison.CurrentCultureIgnoreCase));

                if (virtCompDoc != null)
                {
                    if (virtCompDoc.IsAlive)
                    {
                        return true;
                    }
                    else
                    {
                        m_VirtualDocumentsCache.Remove(virtCompDoc);
                        virtCompDoc = null;
                    }
                }

                var comp = compsLazy.Value.FirstOrDefault(c => string.Equals(
                    Path.GetFileName(c.CachedPath), virtCompFileName,
                    StringComparison.CurrentCultureIgnoreCase));

                if (comp != null)
                {
                    virtCompDoc = (SwDmDocument3D)comp.ReferencedDocument;
                    m_VirtualDocumentsCache.Add(virtCompDoc);
                    return true;
                }
            }
            catch
            {
            }

            virtCompDoc = null;
            return false;
        }

        /// <remarks>Document manager does not consider loaded references when resolving paths, so using the cache to retrieve the already loaded files by name</remarks>
        private Tuple<string, bool>[] GetRawDependencies(List<string> cache)
        {
            string[] deps;
            object isVirtualObj;

            var searchOpts = m_Doc.OwnerApplication.SwDocMgr.GetSearchOptionObject();

            var searchFilter = SwDmSearchFilters.SwDmSearchExternalReference
                | SwDmSearchFilters.SwDmSearchSubfolders
                | SwDmSearchFilters.SwDmSearchInContextReference
                | SwDmSearchFilters.SwDmSearchForPart
                | SwDmSearchFilters.SwDmSearchForAssembly;

            if (m_Doc is ISwDmVirtualDocument3D) 
            {
                //NOTE:Need to search in the root assembly folder for the components which are part of virtual assembly
                searchFilter |= SwDmSearchFilters.SwDmSearchRootAssemblyFolder;
            }

            searchOpts.SearchFilters = (int)searchFilter;

            if (m_Doc.IsVersionNewerOrEqual(SwDmVersion_e.Sw2017))
            {
                deps = ((ISwDMDocument21)m_Doc.Document).GetAllExternalReferences5(searchOpts, out _, out isVirtualObj, out _, out _) as string[];
            }
            else
            {
                deps = ((ISwDMDocument13)m_Doc.Document).GetAllExternalReferences4(searchOpts, out _, out isVirtualObj, out _) as string[];
            }

            if (deps != null)
            {
                var isVirtual = (bool[])isVirtualObj;

                if (isVirtual.Length != deps.Length)
                {
                    throw new Exception("Invalid API. Number of virtual components information does not match references count");
                }

                var res = new Tuple<string, bool>[deps.Length];

                for (int i = 0; i < res.Length; i++)
                {
                    var depPath = deps[i];
                    var fileName = Path.GetFileName(depPath);

                    var usedPathIndex = cache.FindIndex(p => string.Equals(Path.GetFileName(p), fileName, StringComparison.CurrentCultureIgnoreCase));

                    if (usedPathIndex != -1)
                    {
                        depPath = cache[usedPathIndex];
                    }
                    else 
                    {
                        cache.Add(depPath);
                    }

                    res[i] = new Tuple<string, bool>(depPath, isVirtual[i]);
                }

                return res;
            }
            else
            {
                return Array.Empty<Tuple<string, bool>>();
            }
        }
    }
}
