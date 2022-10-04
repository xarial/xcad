using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Documents;
using Xarial.XCad.Toolkit.Exceptions;

namespace Xarial.XCad.Toolkit.Utils
{
    /// <summary>
    /// This services dispatches the model docs and creates IXDocument objects
    /// </summary>
    /// <remarks>This service is also responsible to using the objects pre-created templates where applicable instead of creating new ones.
    public abstract class DocumentDispatcherBase<TDocument, TUnderlineDoc>
        where TDocument : IXDocument
    {
        public event Action<TDocument> Dispatched;

        private readonly List<TDocument> m_DocsDispatchQueue;
        private readonly List<TUnderlineDoc> m_UnderlineDocsDispatchQueue;

        private readonly object m_Lock;

        protected readonly IXLogger m_Logger;

        protected DocumentDispatcherBase(IXLogger logger)
        {
            m_Logger = logger;

            m_DocsDispatchQueue = new List<TDocument>();
            m_UnderlineDocsDispatchQueue = new List<TUnderlineDoc>();

            m_Lock = new object();
        }

        /// <summary>
        /// Checks if there are any documents in the dispatch queue
        /// </summary>
        public bool HasDocumentsInDispatchQueue => m_DocsDispatchQueue.Any();

        /// <summary>
        /// Dispatches the loaded document
        /// </summary>
        /// <param name="underlineDoc">Model to add to the queue</param>
        public void Dispatch(TUnderlineDoc underlineDoc)
        {
            if (underlineDoc == null)
            {
                throw new ArgumentNullException(nameof(underlineDoc));
            }

            lock (m_Lock)
            {
                m_Logger.Log($"Adding '{GetTitle(underlineDoc)}' to the dispatch queue", LoggerMessageSeverity_e.Debug);

                m_UnderlineDocsDispatchQueue.Add(underlineDoc);

                if (!m_DocsDispatchQueue.Any())
                {
                    DispatchAllUnderlineDocuments();
                }
            }
        }

        /// <summary>
        /// Puts the document into the dispatch queue
        /// </summary>
        /// <param name="doc">Document to put into the queue</param>
        public void BeginDispatch(TDocument doc) => m_DocsDispatchQueue.Add(doc);

        /// <summary>
        /// Force removs the document from the dispatch queue
        /// </summary>
        /// <param name="doc">Document to remove</param>
        public void TryRemoveFromDispatchQueue(TDocument doc)
        {
            lock (m_Lock)
            {
                if (m_DocsDispatchQueue.Contains(doc))
                {
                    m_DocsDispatchQueue.Remove(doc);
                }
            }
        }

        /// <summary>
        /// Removes the document from the queue
        /// </summary>
        /// <param name="doc">Document to remove from the queue</param>
        /// <param name="underlineDoc">Actual pointer to the model. If null system will try to find the matching model</param>
        public void EndDispatch(TDocument doc, TUnderlineDoc underlineDoc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            if (underlineDoc == null)
            {
                throw new ArgumentNullException(nameof(underlineDoc));
            }

            lock (m_Lock)
            {
                m_DocsDispatchQueue.Remove(doc);

                var index = m_UnderlineDocsDispatchQueue.FindIndex(m => CompareUnderlineDocuments(m, underlineDoc));

                if (index != -1)
                {
                    m_Logger.Log($"Removing '{doc.Title}' from the dispatch queue", LoggerMessageSeverity_e.Debug);

                    m_UnderlineDocsDispatchQueue.RemoveAt(index);
                }
                else
                {
                    m_Logger.Log($"Document '{doc.Title}' is not in the dispatch queue", LoggerMessageSeverity_e.Warning);
                }

                BindDocument(doc, underlineDoc);

                if (doc.IsCommitted)
                {
                    NotifyDispatchedSafe(doc);
                }

                if (!m_DocsDispatchQueue.Any())
                {
                    DispatchAllUnderlineDocuments();
                }
            }
        }

        public TDocument RegisterUnderlineDocument(TUnderlineDoc underlineDoc)
        {
            if (underlineDoc == null)
            {
                throw new NullReferenceException("Model is null");
            }

            var doc = CreateDocument(underlineDoc);

            NotifyDispatchedSafe(doc);

            return doc;
        }

        private void DispatchAllUnderlineDocuments()
        {
            lock (m_Lock)
            {
                m_Logger.Log($"Dispatching all ({m_UnderlineDocsDispatchQueue.Count}) underline documents", LoggerMessageSeverity_e.Debug);

                var errors = new List<Exception>();

                foreach (var model in m_UnderlineDocsDispatchQueue)
                {
                    try
                    {
                        RegisterUnderlineDocument(model);
                    }
                    catch (Exception ex)
                    {
                        errors.Add(ex);
                    }
                }

                m_UnderlineDocsDispatchQueue.Clear();
                m_Logger.Log($"Cleared models queue", LoggerMessageSeverity_e.Debug);

                if (errors.Any())
                {
                    throw new DocumentsQueueDispatchException(errors.ToArray());
                }
            }
        }

        private void NotifyDispatchedSafe(TDocument doc)
        {
            try
            {
                m_Logger.Log($"Dispatched '{doc.Title}'", LoggerMessageSeverity_e.Debug);
                Dispatched?.Invoke(doc);
            }
            catch (Exception ex)
            {
                m_Logger.Log($"Unhandled exception while dispatching the document '{doc.Title}'", LoggerMessageSeverity_e.Error);
                m_Logger.Log(ex);
            }
        }

        protected abstract bool CompareUnderlineDocuments(TUnderlineDoc firstDoc, TUnderlineDoc secondDoc);
        protected abstract TDocument CreateDocument(TUnderlineDoc specDoc);
        protected abstract void BindDocument(TDocument doc, TUnderlineDoc underlineDoc);
        protected abstract string GetTitle(TUnderlineDoc underlineDoc);
    }
}
