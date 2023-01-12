//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents;
using Xarial.XCad.Base;
using SolidWorks.Interop.swdocumentmgr;
using System.Linq;
using Xarial.XCad.SwDocumentManager.Services;
using System.IO;
using Xarial.XCad.Toolkit.Exceptions;
using Xarial.XCad.Exceptions;
using System.Threading;
using Xarial.XCad.Toolkit.Utils;
using Xarial.XCad.Documents.Delegates;

namespace Xarial.XCad.SwDocumentManager.Documents
{
    public interface ISwDmComponentCollection : IXComponentRepository 
    {
    }

    internal class SwDmComponentCollection : ISwDmComponentCollection
    {
        #region Not Supported
        public void AddRange(IEnumerable<IXComponent> ents, CancellationToken cancellationToken) => throw new NotSupportedException();
        public void RemoveRange(IEnumerable<IXComponent> ents, CancellationToken cancellationToken) => throw new NotSupportedException();
        public T PreCreate<T>() where T : IXComponent => throw new NotSupportedException();
        #endregion

        private readonly ISwDmConfiguration m_Conf;
        private readonly SwDmAssembly m_ParentAssm;

        private IFilePathResolver m_PathResolver;

        private readonly Dictionary<string, SwDmComponent> m_ComponentsCache;

        internal SwDmComponentCollection(SwDmAssembly parentAssm, ISwDmConfiguration conf) 
        {
            m_ParentAssm = parentAssm;
            m_Conf = conf;

            m_PathResolver = new SwDmFilePathResolver();
            m_ComponentsCache = new Dictionary<string, SwDmComponent>(StringComparer.CurrentCultureIgnoreCase);
        }

        public IXComponent this[string name] => RepositoryHelper.Get(this, name);

        public int Count 
            => (((ISwDMConfiguration2)m_Conf.Configuration).GetComponents() as object[])?.Length ?? 0;

        public int TotalCount 
        {
            get 
            {
                var totalCount = 0;

                var cachedCount = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);

                CountComponents(m_Conf.Configuration, cachedCount, ref totalCount);

                return totalCount;
            }
        }
        
        private void CountComponents(ISwDMConfiguration conf, Dictionary<string, int> cachedCount, ref int totalCount) 
        {
            foreach (ISwDMComponent6 comp in ((ISwDMConfiguration2)conf).GetComponents() as object[] ?? new object[0])
            {
                totalCount++;

                if (comp.DocumentType == SwDmDocumentType.swDmDocumentAssembly && !comp.IsSuppressed())
                {
                    try
                    {
                        var path = m_PathResolver.ResolvePath(Path.GetDirectoryName(m_ParentAssm.Path), comp.PathName);

                        var confName = comp.ConfigurationName;

                        var cacheKey = $"{path}:{confName}";
                        
                        if (cachedCount.TryGetValue(cacheKey, out int count))
                        {
                            totalCount += count;
                        }
                        else
                        {
                            if (File.Exists(path))
                            {
                                int subTotalCount = 0;

                                var subAssm = m_ParentAssm.OwnerApplication.SwDocMgr
                                    .GetDocument(path, SwDmDocumentType.swDmDocumentAssembly, true, out SwDmDocumentOpenError err);

                                try
                                {
                                    if (subAssm != null)
                                    {
                                        var subConf = subAssm.ConfigurationManager.GetConfigurationByName(confName);

                                        if (subConf != null)
                                        {
                                            CountComponents(subConf, cachedCount, ref subTotalCount);
                                        }
                                    }

                                    totalCount += subTotalCount;
                                    cachedCount.Add(cacheKey, subTotalCount);
                                }
                                finally 
                                {
                                    subAssm?.CloseDoc();
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        private IEnumerable<ISwDMComponent> IterateDmComponents() 
        {
            if (m_Conf.IsCommitted)
            {
                //if the parent documetn was closed calling the below method will open the document into the memory
                return (((ISwDMConfiguration2)m_Conf.Configuration)
                    .GetComponents() as object[] ?? new object[0])
                    .Cast<ISwDMComponent>();
            }
            else
            {
                throw new NonCommittedElementAccessException();
            }
        }

        public IEnumerator<IXComponent> GetEnumerator()
            => IterateDmComponents()
            .Select(c => CreateComponentInstance(c))
            .GetEnumerator();

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) => RepositoryHelper.FilterDefault(this, filters, reverseOrder);

        protected virtual SwDmComponent CreateComponentInstance(ISwDMComponent dmComp) 
        {
            var compName = ((ISwDMComponent7)dmComp).Name2;

            if (!m_ComponentsCache.TryGetValue(compName, out SwDmComponent comp))
            {
                comp = SwDmObjectFactory.FromDispatch<SwDmComponent>(dmComp, m_ParentAssm);
                m_ComponentsCache.Add(compName, comp);
            }

            return comp;
        }

        public bool TryGet(string name, out IXComponent ent)
        {
            var comp = IterateDmComponents().FirstOrDefault(c => string.Equals(((ISwDMComponent7)c).Name2,
                name, StringComparison.CurrentCultureIgnoreCase));

            if (comp != null)
            {
                ent = CreateComponentInstance(comp);
                return true;
            }
            else
            {
                ent = null;
                return false;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
