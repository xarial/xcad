//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Exceptions;
using Xarial.XCad.Exceptions;

namespace Xarial.XCad.Documents.Extensions
{
    /// <summary>
    /// Additional methods for <see cref="IXDocument"/>
    /// </summary>
    public static class XDocumentExtension
    {
        /// <summary>
        /// Returns all dependencies including nested dependencies
        /// </summary>
        /// <param name="doc">Input document</param>
        /// <param name="topLevelOnly">True to only load top level references, false to recursively load all references</param>
        /// <param name="allowCommit">Allow commit documents if not commited</param>
        /// <param name="allowReadOnly">Allow commit documents in read-only state if allowCommit is true and source document is open with write access</param>
        /// <returns>All dependencies</returns>
        public static IEnumerable<IXDocument3D> IterateDependencies(this IXDocument doc, bool topLevelOnly = false, bool allowCommit = true, bool allowReadOnly = true)
        {
            if (doc == null) 
            {
                throw new ArgumentNullException(nameof(doc));
            }

            return EnumerateDependencies(doc, !topLevelOnly, doc.State.HasFlag(DocumentState_e.ReadOnly),
                allowCommit, allowReadOnly, new List<string>());
        }

        /// <summary>
        /// Saves the document as new file
        /// </summary>
        /// <param name="doc">Input document</param>
        /// <param name="filePath">Output file path</param>
        public static void SaveAs(this IXDocument doc, string filePath) 
        {
            var oper = doc.PreCreateSaveAsOperation(filePath);
            oper.Commit();
        }

        /// <summary>
        /// Saves the document and configure options
        /// </summary>
        /// <typeparam name="T">Type of save operation</typeparam>
        /// <param name="doc">Input document</param>
        /// <param name="filePath">Output file path</param>
        /// <param name="configurer">Specific configurer</param>
        public static void SaveAs<T>(this IXDocument doc, string filePath, Action<T> configurer)
            where T : IXSaveOperation
        {
            var oper = (T)doc.PreCreateSaveAsOperation(filePath);
            configurer.Invoke(oper);
            oper.Commit();
        }

        private static IEnumerable<IXDocument3D> EnumerateDependencies(IXDocument doc, bool recurse, bool isSrcReadOnly, bool allowCommit, bool allowReadOnly, List<string> usedPaths) 
        {
            IXDocument3D[] deps = null;

            try
            {
                deps = doc.Dependencies.ToArray();
            }
            catch (NonCommittedElementAccessException)
            {
                if (allowCommit)
                {
                    deps = TryCommitAndGetDependencies(doc, allowReadOnly);
                }
            }
            catch 
            {
            }

            foreach (var dep in deps ?? new IXDocument3D[0])
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
                        foreach (var childDep in EnumerateDependencies(dep, recurse, isSrcReadOnly, allowCommit, allowReadOnly, usedPaths))
                        {
                            yield return childDep;
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
