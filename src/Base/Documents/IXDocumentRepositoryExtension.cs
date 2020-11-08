//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Additonal methods for <see cref="IXDocumentRepository"/>
    /// </summary>
    public static class IXDocumentRepositoryExtension
    {
        /// <summary>
        /// Creates new part document
        /// </summary>
        /// <param name="repo">This repository</param>
        /// <returns>New part</returns>
        public static IXPart NewPart(this IXDocumentRepository repo)
            => CreateAndCommitNewDocument<IXPart>(repo);

        /// <summary>
        /// Creates new assembly document
        /// </summary>
        /// <param name="repo">This repository</param>
        /// <returns>New aseembly</returns>
        public static IXAssembly NewAssembly(this IXDocumentRepository repo)
            => CreateAndCommitNewDocument<IXAssembly>(repo);

        /// <summary>
        /// Creates new drawing document
        /// </summary>
        /// <param name="repo">This repository</param>
        /// <returns>New drawing</returns>
        public static IXDrawing NewDrawing(this IXDocumentRepository repo)
            => CreateAndCommitNewDocument<IXDrawing>(repo);

        private static TDoc CreateAndCommitNewDocument<TDoc>(this IXDocumentRepository repo)
            where TDoc : class, IXDocument
        {
            var doc = repo.PreCreate<TDoc>();
            repo.Add(doc);

            if (doc is IXUnknownDocument) 
            {
                doc = (TDoc)(doc as IXUnknownDocument).GetSpecific();
            }

            return doc;
        }

        public static IXDocument Open(this IXDocumentRepository repo, string path, 
            bool silent = true, bool viewOnly = false,
            bool readOnly = false, bool rapid = false)
        {
            var doc = repo.PreCreate<IXUnknownDocument>();

            doc.Path = path;
            doc.Silent = silent;
            doc.ViewOnly = viewOnly;
            doc.ReadOnly = readOnly;
            doc.Rapid = rapid;

            repo.Add(doc);

            return doc.GetSpecific();
        }
    }
}