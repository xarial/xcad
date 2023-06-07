//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;

namespace Xarial.XCad.SwDocumentManager.Documents
{
    internal class SwDmDocumentDependencies : IXDocumentDependencies
    {
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

        internal SwDmDocumentDependencies(SwDmDocument doc) 
        {
            m_Doc = doc;
            m_VirtualDocumentsCache = new List<SwDmDocument3D>();
        }

        public IEnumerator<IXDocument3D> GetEnumerator() => IterateDependencies().GetEnumerator();

        public void Replace(IXDocument3D source, IXDocument3D target)
        {
            m_Doc.Document.ReplaceReference(source.Path, target.Path);
        }

        private IEnumerable<SwDmDocument3D> IterateDependencies() 
        {
            var deps = GetRawDependencies();

            if (deps.Any())
            {
                var compsLazy = new Lazy<ISwDmComponent[]>(
                    () =>
                    {
                        if (string.Equals(System.IO.Path.GetExtension(m_Doc.Path), ".sldasm", StringComparison.CurrentCultureIgnoreCase))
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
                                return new ISwDmComponent[0];
                            }
                        }
                        else
                        {
                            throw new Exception("Components can only be extracted from the assembly");
                        }
                    });

                bool TryFindVirtualDocument(string filePath, out SwDmDocument3D virtCompDoc)
                {
                    try
                    {
                        var virtCompFileName = System.IO.Path.GetFileName(filePath);

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
                            System.IO.Path.GetFileName(c.CachedPath), virtCompFileName,
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
                        if (!isVirtual || !TryFindVirtualDocument(depPath, out depDoc))
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

        private Tuple<string, bool>[] GetRawDependencies()
        {
            string[] deps;
            object isVirtualObj;

            var searchOpts = m_Doc.OwnerApplication.SwDocMgr.GetSearchOptionObject();

            searchOpts.SearchFilters = (int)(
                SwDmSearchFilters.SwDmSearchExternalReference
                | SwDmSearchFilters.SwDmSearchRootAssemblyFolder
                | SwDmSearchFilters.SwDmSearchSubfolders
                | SwDmSearchFilters.SwDmSearchInContextReference);

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
                    res[i] = new Tuple<string, bool>(deps[i], isVirtual[i]);
                }

                return res;
            }
            else
            {
                return new Tuple<string, bool>[0];
            }
        }
    }
}
