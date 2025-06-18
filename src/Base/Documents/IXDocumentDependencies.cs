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
    }
}
