//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Documents.Services;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SwDocumentManager.Documents
{
    public interface ISwDmDocumentCollection : IXDocumentRepository, IDisposable 
    {
        bool TryGet(string name, out ISwDmDocument ent);
        new ISwDmDocument this[string name] { get; }
        new ISwDmDocument Active { get; set; }
    }

    internal class SwDmDocumentCollection : ISwDmDocumentCollection
    {
        #region NotSupported
        
        public event DocumentEventDelegate DocumentActivated 
        {
            add => throw new NotSupportedException();
            remove => throw new NotSupportedException();
        }

        public event DocumentEventDelegate DocumentLoaded
        {
            add => throw new NotSupportedException();
            remove => throw new NotSupportedException();
        }

        public event DocumentEventDelegate DocumentOpened
        {
            add => throw new NotSupportedException();
            remove => throw new NotSupportedException();
        }

        public event DocumentEventDelegate NewDocumentCreated
        {
            add => throw new NotSupportedException();
            remove => throw new NotSupportedException();
        }

        #endregion

        IXDocument IXRepository<IXDocument>.this[string name] => this[name];

        IXDocument IXDocumentRepository.Active
        {
            get => Active;
            set => Active = (ISwDmDocument)value;
        }

        bool IXRepository<IXDocument>.TryGet(string name, out IXDocument ent)
        {
            var res = this.TryGet(name, out ISwDmDocument doc);
            ent = doc;
            return res;
        }

        public ISwDmDocument this[string name] => (ISwDmDocument)m_RepoHelper.Get(name);

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

        private List<ISwDmDocument> m_Documents;

        private readonly SwDmApplication m_DmApp;

        private readonly RepositoryHelper<IXDocument> m_RepoHelper;

        internal SwDmDocumentCollection(SwDmApplication dmApp)
        {
            m_DmApp = dmApp;

            m_RepoHelper = new RepositoryHelper<IXDocument>(this,
                () => new SwDmUnknownDocument(m_DmApp, null, false, OnDocumentCreated, OnDocumentClosed, null),
                () => new SwDmUnknownDocument3D(m_DmApp, null, false, OnDocumentCreated, OnDocumentClosed, null),
                () => new SwDmPart(m_DmApp, null, false, OnDocumentCreated, OnDocumentClosed, null),
                () => new SwDmAssembly(m_DmApp, null, false, OnDocumentCreated, OnDocumentClosed, null),
                () => new SwDmDrawing(m_DmApp, null, false, OnDocumentCreated, OnDocumentClosed, null));

            m_Documents = new List<ISwDmDocument>();
        }

        public void AddRange(IEnumerable<IXDocument> ents, CancellationToken cancellationToken) => m_RepoHelper.AddRange(ents, cancellationToken);

        public IEnumerator<IXDocument> GetEnumerator() => m_Documents.GetEnumerator();

        public THandler GetHandler<THandler>(IXDocument doc) where THandler : IDocumentHandler => throw new NotImplementedException();

        public void RegisterHandler<THandler>(Func<THandler> handlerFact) where THandler : IDocumentHandler => throw new NotImplementedException();

        public void UnregisterHandler<THandler>() where THandler : IDocumentHandler => throw new NotImplementedException();

        public void RemoveRange(IEnumerable<IXDocument> ents, CancellationToken cancellationToken)
        {
            foreach (var doc in ents)
            {
                doc.Close();
            }
        }

        public bool TryGet(string name, out ISwDmDocument ent)
        {
            if (System.IO.Path.IsPathRooted(name))
            {
                ent = m_Documents.FirstOrDefault(
                    d => string.Equals(d.Path, name, StringComparison.CurrentCultureIgnoreCase));
            }
            else if (System.IO.Path.HasExtension(name))
            {
                ent = m_Documents.FirstOrDefault(
                    d => string.Equals(System.IO.Path.GetFileName(d.Path), name,
                    StringComparison.CurrentCultureIgnoreCase));
            }
            else
            {
                ent = m_Documents.FirstOrDefault(
                    d => string.Equals(System.IO.Path.GetFileNameWithoutExtension(d.Path),
                    name, StringComparison.CurrentCultureIgnoreCase));
            }

            if (ent?.IsAlive == false) 
            {
                ent.Close();
                ent = null;
            }

            return ent != null;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) 
            => m_RepoHelper.FilterDefault(this, filters, reverseOrder);

        public T PreCreate<T>() where T : IXDocument
            => m_RepoHelper.PreCreate<T>();

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
