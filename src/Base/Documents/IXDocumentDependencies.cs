using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        /// Replaces the dependency
        /// </summary>
        /// <param name="source">Source dependency to replace</param>
        /// <param name="target">New dependency</param>
        void Replace(IXDocument3D source, IXDocument3D target);
    }

    /// <summary>
    /// Additional methods for <see cref="IXDocumentDependencies"/>
    /// </summary>
    public static class XDocumentDependenciesExtension 
    {
        /// <summary>
        /// Replaces all dependencies recursively
        /// </summary>
        /// <param name="deps">Dependencies to reaplce</param>
        /// <param name="replacementPathProvider">File path replacer</param>
        public static void ReplaceAll(this IXDocumentDependencies deps, Func<string, string> replacementPathProvider)
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
                foreach (var docDep in targDeps.ToArray())
                {
                    if (renameMaps.TryGetValue(docDep.Path, out var newPath))
                    {
                        var newDoc = app.Documents.PreCreate<IXDocument3D>();
                        newDoc.Path = newPath;
                        targDeps.Replace(docDep, newDoc);
                    }
                    else
                    {
                        throw new Exception($"Failed to find the rename map of {docDep.Path}");
                    }
                }
            }
        }
    }
}
