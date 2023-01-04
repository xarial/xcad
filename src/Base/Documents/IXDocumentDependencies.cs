using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Manages dependencies of the <see cref="IXDocument"/>
    /// </summary>
    public interface IXDocumentDependencies : IEnumerable<IXDocument3D>
    {
        /// <summary>
        /// Replaces the dependency
        /// </summary>
        /// <param name="source">Source dependency to replace</param>
        /// <param name="target">New dependency</param>
        void Replace(IXDocument3D source, IXDocument3D target);
    }

    public static class XDocumentDependenciesExtension 
    {
        private class RootDocumentReferenceInfo : DocumentReferenceInfo
        {
            internal override IXDocumentDependencies TargetDependencies { get; }

            internal RootDocumentReferenceInfo(IXDocumentDependencies rootDeps, IReadOnlyList<DocumentReferenceInfo> children) 
                : base(null, null, children)
            {
                TargetDependencies = rootDeps;
            }
        }

        [DebuggerDisplay("{" + nameof(Source) + "} -> {" + nameof(Destination) + "}")]
        private class DocumentReferenceInfo 
        {
            internal IXDocument Source { get; }
            internal IXDocument Destination { get; set; }

            internal IReadOnlyList<DocumentReferenceInfo> Children { get; }

            internal virtual IXDocumentDependencies TargetDependencies => Destination.Dependencies;

            internal DocumentReferenceInfo(IXDocument source, IXDocument dest, IReadOnlyList<DocumentReferenceInfo> children)
            {
                Source = source;
                Destination = dest;
                Children = children;
            }
        }

        /// <summary>
        /// Renames all references keeping the references
        /// </summary>
        /// <param name="deps">Dependencies to rename</param>
        /// <param name="replacer">File path replacer</param>
        /// <remarks>All references will be deleted</remarks>
        public static void Rename(this IXDocumentDependencies deps, Func<string, string> replacer)
            => Rename(deps, replacer, File.Delete);

        /// <inheritdoc/>
        /// <param name="oldFilesHandler">Function to handle the old references</param>
        public static void Rename(this IXDocumentDependencies deps, Func<string, string> replacer, Action<string> oldFilesHandler)
        {
            var cache = new Dictionary<string, DocumentReferenceInfo>(StringComparer.CurrentCultureIgnoreCase);
            var children = new List<DocumentReferenceInfo>();
            var rootRef = new RootDocumentReferenceInfo(deps, children);
            IXApplication app = null;
            FillChildren(deps, children, cache, ref app);

            foreach (var replRef in Flatten(rootRef))
            {
                var destDoc = app.Documents.PreCreate<IXDocument3D>();
                destDoc.Path = replacer.Invoke(replRef.Source.Path);
                replRef.Destination = destDoc;
            }

            var processedFiles = new List<DocumentReferenceInfo>();

            ReplaceReferences(rootRef, processedFiles);

            foreach (var procFile in processedFiles)
            {
                if (!string.Equals(procFile.Source.Path, procFile.Destination.Path, StringComparison.CurrentCultureIgnoreCase))
                {
                    oldFilesHandler.Invoke(procFile.Source.Path);
                }
            }
        }

        private static void FillChildren(IXDocumentDependencies deps,
            List<DocumentReferenceInfo> children,
            Dictionary<string, DocumentReferenceInfo> cachedRefs, ref IXApplication app)
        {
            foreach (var dep in deps)
            {
                if (app == null) 
                {
                    app = dep.OwnerApplication;
                }

                if (!cachedRefs.TryGetValue(dep.Path, out DocumentReferenceInfo replRef))
                {
                    var subChildren = new List<DocumentReferenceInfo>();
                    replRef = new DocumentReferenceInfo(dep, dep, subChildren);
                    cachedRefs.Add(dep.Path, replRef);
                    FillChildren(replRef.TargetDependencies, subChildren, cachedRefs, ref app);
                }

                children.Add(replRef);
            }
        }

        private static IEnumerable<DocumentReferenceInfo> Flatten(DocumentReferenceInfo parentRefInfo)
        {
            var processed = new List<DocumentReferenceInfo>();

            IEnumerable<DocumentReferenceInfo> FlattenChildren(DocumentReferenceInfo parent)
            {
                if (parent.Children != null)
                {
                    foreach (var child in parent.Children)
                    {
                        if (!processed.Contains(child))
                        {
                            processed.Add(child);

                            yield return child;

                            foreach (var subItem in FlattenChildren(child))
                            {
                                yield return subItem;
                            }
                        }
                    }
                }
            }

            foreach (var replRef in FlattenChildren(parentRefInfo))
            {
                yield return replRef;
            }
        }

        private static void ReplaceReferences(DocumentReferenceInfo replRef, List<DocumentReferenceInfo> processedRefs)
        {
            for (int i = 0; i < replRef.Children.Count; i++)
            {
                var childRef = replRef.Children[i];

                if (!string.Equals(childRef.Source.Path, childRef.Destination.Path, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (!processedRefs.Contains(childRef))
                    {
                        File.Copy(childRef.Source.Path, childRef.Destination.Path);
                    }

                    replRef.TargetDependencies.Replace((IXDocument3D)childRef.Source, (IXDocument3D)childRef.Destination);
                }

                if (!processedRefs.Contains(childRef))
                {
                    processedRefs.Add(childRef);
                    ReplaceReferences(childRef, processedRefs);
                }
            }
        }
    }
}
