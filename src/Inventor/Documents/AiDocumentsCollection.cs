//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Inventor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Documents.Services;
using Xarial.XCad.Inventor.Documents.Services;
using Xarial.XCad.Inventor.Utils;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.Inventor.Documents
{
    public interface IAiDocumentsCollection : IXDocumentRepository 
    {
        IAiDocument this[Document doc] { get; }
    }

    internal class AiDocumentsCollection : IAiDocumentsCollection, IDisposable
    {
        public event DocumentEventDelegate DocumentActivated;
        public event DocumentEventDelegate DocumentLoaded;
        public event DocumentEventDelegate DocumentOpened;
        public event DocumentEventDelegate NewDocumentCreated;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private readonly AiApplication m_App;

        private readonly Dictionary<Document, AiDocument> m_Documents;

        internal AiDocumentDispatcher Dispatcher { get; }

        private readonly object m_Lock;

        private bool m_IsAttached;

        private readonly IXLogger m_Logger;

        internal AiDocumentsCollection(AiApplication app, IXLogger logger) 
        {
            m_App = app;
            m_Logger = logger;

            m_Documents = new Dictionary<Document, AiDocument>(new AiDocumentPointerEqualityComparer());

            m_Lock = new object();

            Dispatcher = new AiDocumentDispatcher(app, m_Logger);

            m_IsAttached = false;
        }

        internal void Attach()
        {
            if (!m_IsAttached)
            {
                m_IsAttached = true;
                Dispatcher.Dispatched += OnDocumentDispatched;

                m_App.Application.ApplicationEvents.OnCloseDocument += OnCloseDocument;
                m_App.Application.ApplicationEvents.OnInitializeDocument += OnInitializeDocument;

                AttachToAllOpenedDocuments();
            }
        }

        private void AttachToAllOpenedDocuments()
        {
            foreach (Document doc in m_App.Application.Documents) 
            {
                Dispatcher.Dispatch(doc);
            }
        }

        private void OnDocumentDispatched(AiDocument doc)
        {
            lock (m_Lock)
            {
                if (!m_Documents.ContainsKey(doc.Document))
                {
                    m_Documents.Add(doc.Document, doc);

                    try
                    {
                        DocumentLoaded?.Invoke(doc);
                    }
                    catch (Exception ex)
                    {
                        m_Logger.Log(ex);
                    }
                }
                else 
                {
                    m_Logger.Log($"Conflict. {doc.Document.DisplayName} already dispatched", LoggerMessageSeverity_e.Warning);
                }
            }
        }

        private void OnInitializeDocument(_Document DocumentObject, string FullDocumentName, EventTimingEnum BeforeOrAfter, NameValueMap Context, out HandlingCodeEnum HandlingCode)
        {
            if (BeforeOrAfter == EventTimingEnum.kAfter) 
            {
                Dispatcher.Dispatch(DocumentObject);
            }

            HandlingCode = HandlingCodeEnum.kEventHandled;
        }

        private void OnCloseDocument(_Document DocumentObject, string FullDocumentName, EventTimingEnum BeforeOrAfter, NameValueMap Context, out HandlingCodeEnum HandlingCode)
        {
            if (BeforeOrAfter == EventTimingEnum.kAfter)
            {
                ReleaseDocument(m_Documents[DocumentObject]);
            }

            HandlingCode = HandlingCodeEnum.kEventHandled;
        }

        public IXDocument this[string name] => RepositoryHelper.Get(this, name);

        public IAiDocument this[Document doc] => m_Documents[doc];

        public IXDocument Active 
        {
            get 
            {
                var activeDoc = m_App.Application.ActiveDocument;

                if (activeDoc != null)
                {
                    return m_Documents[activeDoc];
                }
                else 
                {
                    return null;
                }
            }
            set 
            {
                if (value == null) 
                {
                    throw new ArgumentNullException();
                }

                ((IAiDocument)value).Document.Activate();
            }
        }

        public int Count => m_Documents.Count;

        public void AddRange(IEnumerable<IXDocument> ents, CancellationToken cancellationToken)
            => RepositoryHelper.AddRange(ents, cancellationToken);

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters)
            => RepositoryHelper.FilterDefault(this, filters, reverseOrder);

        public IEnumerator<IXDocument> GetEnumerator() => m_Documents.Values.GetEnumerator();

        public THandler GetHandler<THandler>(IXDocument doc) where THandler : IDocumentHandler
        {
            throw new NotImplementedException();
        }

        public T PreCreate<T>() where T : IXDocument
        {
            var doc = RepositoryHelper.PreCreate<IXDocument, T>(this,
                () => new AiDocument(null, m_App));

            if (!(doc is AiDocument))
            {
                throw new InvalidCastException("Document type must be of type AiDocument");
            }

            return doc;
        }

        public void RegisterHandler<THandler>(Func<THandler> handlerFact) where THandler : IDocumentHandler
        {
            throw new NotImplementedException();
        }

        public void RemoveRange(IEnumerable<IXDocument> ents, CancellationToken cancellationToken)
        {
            foreach (var doc in ents.Cast<IAiDocument>().ToArray()) 
            {
                doc.Close();
            }
        }

        public bool TryGet(string name, out IXDocument ent)
        {
            var doc = m_App.Application.Documents.ItemByName[name];

            if (doc != null)
            {
                ent = m_Documents[doc];
                return true;
            }
            else 
            {
                ent = null;
                return false;
            }
        }

        private void ReleaseDocument(AiDocument doc)
        {
            m_Documents.Remove(doc.Document);

            doc.SetClosed();
            doc.Dispose();
        }

        public void Dispose()
        {
            if (m_IsAttached)
            {
                Dispatcher.Dispatched -= OnDocumentDispatched;

                foreach (var doc in m_Documents.Values.ToArray())
                {
                    ReleaseDocument(doc);
                }

                m_Documents.Clear();
            }
        }
    }
}
