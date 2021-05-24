//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Documents.Services;
using Xarial.XCad.Exceptions;

namespace Xarial.XCad.SwDocumentManager.Documents
{
    public interface ISwDmDocumentCollection : IXDocumentRepository, IDisposable 
    {
        new ISwDmDocument this[string name] { get; }
        new ISwDmDocument Active { get; set; }
    }

    internal class SwDmDocumentCollection : ISwDmDocumentCollection
    {
        IXDocument IXRepository<IXDocument>.this[string name] => this[name];

        IXDocument IXDocumentRepository.Active
        {
            get => Active;
            set => Active = (ISwDmDocument)value;
        }

        public ISwDmDocument this[string name]
        {
            get
            {
                if (TryGet(name, out IXDocument doc))
                {
                    return (ISwDmDocument)doc;
                }
                else
                {
                    throw new EntityNotFoundException(name);
                }
            }
        }

        private ISwDmDocument m_Active;

        public ISwDmDocument Active
        {
            get => m_Active;
            set
            {
                if (value == null || m_Documents.Contains(value))
                {
                    m_Active = value;
                }
                else
                {
                    throw new Exception("Document does not belong to documents repository");
                }
            }
        }

        public int Count => m_Documents.Count;

        public event DocumentActivateDelegate DocumentActivated;
        public event DocumentCreateDelegate DocumentCreated;

        private List<ISwDmDocument> m_Documents;

        private readonly ISwDmApplication m_DmApp;

        internal SwDmDocumentCollection(ISwDmApplication dmApp)
        {
            m_DmApp = dmApp;
            m_Documents = new List<ISwDmDocument>();
        }

        public void AddRange(IEnumerable<IXDocument> ents)
        {
            foreach (var doc in ents)
            {
                doc.Commit(default);
            }
        }

        public IEnumerator<IXDocument> GetEnumerator() => m_Documents.GetEnumerator();

        public THandler GetHandler<THandler>(IXDocument doc) where THandler : IDocumentHandler
        {
            throw new NotImplementedException();
        }

        public void RegisterHandler<THandler>(Func<THandler> handlerFact) where THandler : IDocumentHandler
        {
            throw new NotImplementedException();
        }

        public void RemoveRange(IEnumerable<IXDocument> ents)
        {
            foreach (var doc in ents)
            {
                doc.Close();
            }
        }

        public bool TryGet(string name, out IXDocument ent)
        {
            if (System.IO.Path.IsPathRooted(name))
            {
                ent = m_Documents.FirstOrDefault(
                    d => string.Equals(d.Path, name, StringComparison.CurrentCultureIgnoreCase));

                return ent != null;
            }
            else if (System.IO.Path.HasExtension(name))
            {
                ent = m_Documents.FirstOrDefault(
                    d => string.Equals(System.IO.Path.GetFileName(d.Path), name,
                    StringComparison.CurrentCultureIgnoreCase));

                return ent != null;
            }
            else
            {
                ent = m_Documents.FirstOrDefault(
                    d => string.Equals(System.IO.Path.GetFileNameWithoutExtension(d.Path),
                    name, StringComparison.CurrentCultureIgnoreCase));

                return ent != null;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public TDocument PreCreate<TDocument>()
            where TDocument : class, IXDocument
        {
            SwDmDocument templateDoc;

            if (typeof(IXPart).IsAssignableFrom(typeof(TDocument)))
            {
                templateDoc = new SwDmPart(m_DmApp, null, false, OnDocumentCreated, OnDocumentClosed);
            }
            else if (typeof(IXAssembly).IsAssignableFrom(typeof(TDocument)))
            {
                templateDoc = new SwDmAssembly(m_DmApp, null, false, OnDocumentCreated, OnDocumentClosed);
            }
            else if (typeof(IXDrawing).IsAssignableFrom(typeof(TDocument)))
            {
                templateDoc = new SwDmDrawing(m_DmApp, null, false, OnDocumentCreated, OnDocumentClosed);
            }
            else if (typeof(IXDocument).IsAssignableFrom(typeof(TDocument))
                || typeof(IXUnknownDocument).IsAssignableFrom(typeof(TDocument)))
            {
                templateDoc = new SwDmUnknownDocument(m_DmApp, null, false, OnDocumentCreated, OnDocumentClosed);
            }
            else
            {
                throw new NotSupportedException("Creation of this type of document is not supported");
            }

            return templateDoc as TDocument;
        }

        internal void OnDocumentCreated(ISwDmDocument doc)
        {
            m_Documents.Add(doc);
            Active = doc;
        }

        internal void OnDocumentClosed(ISwDmDocument doc)
        {
            m_Documents.Remove(doc);
            Active = m_Documents.FirstOrDefault();
        }

        public void Dispose()
        {
            foreach (var doc in m_Documents) 
            {
                doc.Close();
            }
        }
    }

    public static class ISwDmDocumentCollectionExtension 
    {
        public static ISwDmDocument PreCreateFromPath(this ISwDmDocumentCollection docs, string path) 
        {
            ISwDmDocument doc;

            switch (SwDmDocument.GetDocumentType(path)) 
            {
                case SwDmDocumentType.swDmDocumentPart:
                    doc = docs.PreCreate<ISwDmPart>();
                    break;

                case SwDmDocumentType.swDmDocumentAssembly:
                    doc = docs.PreCreate<ISwDmAssembly>();
                    break;

                case SwDmDocumentType.swDmDocumentDrawing:
                    doc = docs.PreCreate<ISwDmDrawing>();
                    break;

                default:
                    throw new NotSupportedException("Document type is not supported");
            }

            doc.Path = path;
            return doc;
        }
    }
}
