//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xarial.XCad.Base;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Exceptions;
using Xarial.XCad.Exceptions;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Manages dependencies of the <see cref="IXDocument"/>
    /// </summary>
    public interface IXDocumentDependencies : IEnumerable<IXDocument3D>
    {
        /// <summary>
        /// Owner of these dependencies
        /// </summary>
        IXDocument OwnerDocument { get; }

        /// <summary>
        /// True to read the cached dependencies, false to automaticaly resolve the paths
        /// </summary>
        bool Cached { get; set; }

        /// <summary>
        /// Replaces the dependency
        /// </summary>
        /// <param name="source">Source dependency to replace</param>
        /// <param name="target">New dependency</param>
        void Replace(IXDocument3D source, IXDocument3D target);

        /// <summary>
        /// Returns all dependencies (recursive)
        /// </summary>
        IEnumerable<IXDocument3D> All { get; }
    }

    /// <summary>
    /// Additional methods for <see cref="IXDocumentDependencies"/>
    /// </summary>
    public static class XDocumentDependenciesExtension 
    {
        /// <summary>
        /// Replaces all dependencies recursively
        /// </summary>
        /// <param name="deps">Dependencies to replace</param>
        /// <param name="replacementPathProvider">File path replacer</param>
        /// <param name="conflictResolver">This function is called when target reference has a dependency not in a map. This happens when dependencies are loaded and resolved (not cached) as CAD system can automatically match new file path</param>
        public static void ReplaceAll(this IXDocumentDependencies deps, ReplaceReferencePathProviderDelegate replacementPathProvider,
            ReplaceReferencePathMissingConflictResolverDelegate conflictResolver = null)
        {
            var app = deps.OwnerDocument.OwnerApplication;

            var renameMaps = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);

            void BuildRenameMaps(IXDocumentDependencies curDeps)
            {
                foreach (var dep in curDeps)
                {
                    var srcDocPath = dep.Path;

                    if (!renameMaps.ContainsKey(srcDocPath))
                    {
                        var destDocPath = replacementPathProvider.Invoke(srcDocPath);

                        renameMaps.Add(srcDocPath, destDocPath);

                        BuildRenameMaps(dep.Dependencies);
                    }
                }
            }

            BuildRenameMaps(deps);

            var targetDependencies = renameMaps.Select(x =>
            {
                var doc = app.Documents.PreCreate<IXDocument>();
                doc.Path = x.Value;
                return doc.Dependencies;
            }).ToList();

            if (!targetDependencies.Any(d => string.Equals(d.OwnerDocument.Path, deps.OwnerDocument.Path, StringComparison.CurrentCultureIgnoreCase))) 
            {
                targetDependencies.Insert(0, deps);
            }

            foreach (var targDeps in targetDependencies)
            {
                var origCached = targDeps.Cached;

                targDeps.Cached = deps.Cached;

                try
                {
                    foreach (var docDep in targDeps.ToArray())
                    {
                        var srcDocPath = docDep.Path;

                        if (!renameMaps.TryGetValue(srcDocPath, out var newPath))
                        {
                            if (conflictResolver != null)
                            {
                                newPath = conflictResolver.Invoke(targDeps.OwnerDocument.Path, srcDocPath);
                            }
                            else
                            {
                                throw new Exception($"Failed to find the rename map of {srcDocPath}. Specify '{nameof(conflictResolver)}' delegate to handle this case. Alternatively try setting the {nameof(IXDocumentDependencies)}::{nameof(IXDocumentDependencies.Cached)} to '{true}' to prevent automatic resolution of new references");
                            }
                        }

                        if (!string.Equals(srcDocPath, newPath, StringComparison.CurrentCultureIgnoreCase))
                        {
                            var newDoc = app.Documents.PreCreate<IXDocument3D>();
                            newDoc.Path = newPath;
                            try
                            {
                                targDeps.Replace(docDep, newDoc);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception($"{ex.Message} ({targDeps.OwnerDocument.Path}: {srcDocPath} => {newPath})", ex);
                            }
                        }
                    }
                }
                finally 
                {
                    targDeps.Cached = origCached;
                }
            }
        }

        /// <summary>
        /// Returns all dependencies including nested dependencies, skipping the errors
        /// </summary>
        /// <param name="deps">Input dependencies</param>
        /// <param name="topLevelOnly">True to only load top level references, false to recursively load all references</param>
        /// <param name="allowCommit">Allow commit documents if not commited</param>
        /// <param name="allowReadOnly">Allow commit documents in read-only state if allowCommit is true and source document is open with write access</param>
        /// <returns>All dependencies</returns>
        public static IEnumerable<IXDocument3D> TryIterateAll(this IXDocumentDependencies deps, bool topLevelOnly = false,
            bool allowCommit = true, bool allowReadOnly = true)
        {
            if (deps == null)
            {
                throw new ArgumentNullException(nameof(deps));
            }

            return EnumerateDependencies(deps, !topLevelOnly, deps.OwnerDocument.State.HasFlag(DocumentState_e.ReadOnly),
                allowCommit, allowReadOnly, new List<string>());
        }

        private static IEnumerable<IXDocument3D> EnumerateDependencies(IXDocumentDependencies deps, bool recurse, bool isSrcReadOnly,
            bool allowCommit, bool allowReadOnly, List<string> usedPaths)
        {
            IXDocument3D[] depDocs = null;

            try
            {
                depDocs = deps.ToArray();
            }
            catch (NonCommittedElementAccessException)
            {
                if (allowCommit)
                {
                    depDocs = TryCommitAndGetDependencies(deps.OwnerDocument, allowReadOnly);
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

                    if (recurse)
                    {
                        var childDeps = dep.Dependencies;
                        var origCached = childDeps.Cached;

                        childDeps.Cached = deps.Cached;

                        try
                        {
                            foreach (var childDep in EnumerateDependencies(childDeps, recurse, isSrcReadOnly, allowCommit, allowReadOnly, usedPaths))
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
        }

        private static IXDocument3D[] TryCommitAndGetDependencies(IXDocument doc, bool allowReadOnly)
        {
            try
            {
                doc.Commit();
                return doc.Dependencies.ToArray();
            }
            catch (DocumentWriteAccessDeniedException)
            {
                if (allowReadOnly)
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

                            return doc.Dependencies.ToArray();
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
