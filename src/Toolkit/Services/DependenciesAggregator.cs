using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Exceptions;
using Xarial.XCad.Exceptions;

namespace Xarial.XCad.Toolkit.Services
{
    /// <summary>
    /// Returns all dependencies including nested dependencies, skipping the errors
    /// </summary>
    /// <remarks>use this utility to implement <see cref="IXDocumentDependencies.All"/></remarks>
    public class DependenciesAggregator<TDeps> : IEnumerable<IXDocument3D>
        where TDeps : IXDocumentDependencies
    {
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private readonly TDeps m_Deps;
        private readonly bool m_AllowCommit;
        private readonly bool m_AllowReadOnly;

        /// <summary>Constructor</summary>
        /// <param name="deps">Input dependencies</param>
        /// <param name="allowCommit">Allow commit documents if not commited</param>
        /// <param name="allowReadOnly">Allow commit documents in read-only state if allowCommit is true and source document is open with write access</param>
        public DependenciesAggregator(TDeps deps, bool allowCommit = true, bool allowReadOnly = true) 
        {
            if (deps == null)
            {
                throw new ArgumentNullException(nameof(deps));
            }

            m_Deps = deps;
            m_AllowCommit = allowCommit;
            m_AllowReadOnly = allowReadOnly;
        }
        
        /// <summary>
        /// Enumerates all dependencies
        /// </summary>
        /// <returns>All dependencies</returns>
        public IEnumerator<IXDocument3D> GetEnumerator() 
            => EnumerateDependencies(m_Deps, m_Deps.OwnerDocument.State.HasFlag(DocumentState_e.ReadOnly), new List<string>()).GetEnumerator();

        /// <summary>
        /// Loads all dependencies on the top level
        /// </summary>
        /// <param name="deps">Dependencies</param>
        /// <returns>All dependencies</returns>
        /// <remarks>This needs to be an array so exception is thrown when reading dependencies (e.g. document needs to be committed)
        /// It also allows to load all dependencies from a single level, before loading children (e.g. if caching needs to be implemented)</remarks>
        protected virtual IXDocument3D[] LoadDependencies(TDeps deps) => deps.ToArray();

        private IEnumerable<IXDocument3D> EnumerateDependencies(TDeps deps, bool isSrcReadOnly,
            List<string> usedPaths)
        {
            IXDocument3D[] depDocs = null;

            try
            {
                depDocs = LoadDependencies(deps);
            }
            catch (NonCommittedElementAccessException)
            {
                if (m_AllowCommit)
                {
                    depDocs = TryCommitAndGetDependencies(deps.OwnerDocument);
                }
            }
            catch
            {
            }

            foreach (var dep in depDocs ?? Array.Empty<IXDocument3D>())
            {
                if (!usedPaths.Contains(dep.Path, StringComparer.CurrentCultureIgnoreCase))
                {
                    if (!isSrcReadOnly)
                    {
                        if (!dep.IsCommitted)
                        {
                            var depState = dep.State;

                            if (depState.HasFlag(DocumentState_e.ReadOnly))
                            {
                                //removing read-only state as it was forcibly assigned to the parent
                                depState -= DocumentState_e.ReadOnly;
                                dep.State = depState;
                            }
                        }
                    }

                    usedPaths.Add(dep.Path);
                    yield return dep;

                    var childDeps = dep.Dependencies;
                    var origCached = childDeps.Cached;

                    childDeps.Cached = deps.Cached;

                    try
                    {
                        foreach (var childDep in EnumerateDependencies((TDeps)childDeps, isSrcReadOnly, usedPaths))
                        {
                            yield return childDep;
                        }
                    }
                    finally
                    {
                        childDeps.Cached = origCached;
                    }
                }
            }
        }

        private IXDocument3D[] TryCommitAndGetDependencies(IXDocument doc)
        {
            try
            {
                doc.Commit();
                return LoadDependencies((TDeps)doc.Dependencies);
            }
            catch (DocumentWriteAccessDeniedException)
            {
                if (m_AllowReadOnly)
                {
                    try
                    {
                        var docState = doc.State;

                        if (!docState.HasFlag(DocumentState_e.ReadOnly))
                        {
                            doc.State = docState | DocumentState_e.ReadOnly;

                            try
                            {
                                doc.Commit();
                            }
                            catch
                            {
                                if (!doc.IsCommitted)
                                {
                                    //restoring state if failed to open
                                    doc.State = docState;
                                }
                            }

                            return LoadDependencies((TDeps)doc.Dependencies);
                        }
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }

            return null;
        }
    }
}
