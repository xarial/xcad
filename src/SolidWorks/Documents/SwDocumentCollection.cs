//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Documents.Exceptions;
using Xarial.XCad.Documents.Services;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.SolidWorks.Documents.Services;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Utils.Diagnostics;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwDocumentCollection : IXDocumentRepository, IDisposable
    {
        new ISwDocument Active { get; set; }
        new IXDocument this[string name] { get; }
        ISwDocument this[IModelDoc2 model] { get; }
    }

    [DebuggerDisplay("Documents: {" + nameof(Count) + "}")]
    internal class SwDocumentCollection : ISwDocumentCollection
    {
        public event DocumentCreateDelegate DocumentCreated;
        public event DocumentActivateDelegate DocumentActivated;

        IXDocument IXDocumentRepository.Active 
        {
            get => Active;
            set => Active = (SwDocument)value;
        }
        
        private const int S_OK = 0;

        private readonly SwApplication m_App;
        private readonly SldWorks m_SwApp;
        private readonly Dictionary<IModelDoc2, SwDocument> m_Documents;
        private readonly IXLogger m_Logger;
        private readonly DocumentsHandler m_DocsHandler;

        private readonly SwDocumentDispatcher m_DocsDispatcher;

        private object m_Lock;

        public ISwDocument Active
        {
            get
            {
                var activeDoc = m_SwApp.IActiveDoc2;

                if (activeDoc != null)
                {
                    return this[activeDoc];
                }
                else
                {
                    return null;
                }
            }
            set 
            {
                int errors = -1;
                var doc = m_SwApp.ActivateDoc3(value.Title, true, (int)swRebuildOnActivation_e.swDontRebuildActiveDoc, ref errors);

                if (doc == null) 
                {
                    throw new Exception($"Failed to activate the document. Error code: {errors}");
                }
            }
        }

        public int Count => m_Documents.Count;

        public IXDocument this[string name] 
        {
            get 
            {
                if (TryGet(name, out IXDocument doc))
                {
                    return doc;
                }
                else 
                {
                    throw new Exception("Failed to find the document by name");
                }
            }
        }

        internal SwDocumentCollection(SwApplication app, IXLogger logger)
        {
            m_Lock = new object();

            m_App = app;
            m_SwApp = (SldWorks)m_App.Sw;
            m_Logger = logger;

            m_DocsDispatcher = new SwDocumentDispatcher(app, logger);
            m_DocsDispatcher.Dispatched += OnDocumentDispatched;

            m_Documents = new Dictionary<IModelDoc2, SwDocument>(
                new SwPointerEqualityComparer<IModelDoc2>(m_SwApp));
            m_DocsHandler = new DocumentsHandler(app);
            
            AttachToAllOpenedDocuments();

            m_SwApp.DocumentLoadNotify2 += OnDocumentLoadNotify2;
            m_SwApp.ActiveModelDocChangeNotify += OnActiveModelDocChangeNotify;
        }

        private int OnActiveModelDocChangeNotify()
        {
            DocumentActivated?.Invoke(Active);
            return S_OK;
        }

        public ISwDocument this[IModelDoc2 model]
        {
            get
            {
                SwDocument doc;

                if (m_Documents.TryGetValue(model, out doc))
                {
                    return doc;
                }
                else
                {
                    throw new KeyNotFoundException("Specified model document is not registered");
                }
            }
        }

        public IEnumerator<IXDocument> GetEnumerator()
        {
            return m_Documents.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_Documents.Values.GetEnumerator();
        }

        public void Dispose()
        {
            m_DocsDispatcher.Dispatched -= OnDocumentDispatched;
            m_SwApp.DocumentLoadNotify2 -= OnDocumentLoadNotify2;

            foreach (var doc in m_Documents.Keys.ToArray())
            {
                ReleaseDocument(doc);
            }

            m_Documents.Clear();
        }

        private void AttachToAllOpenedDocuments()
        {
            var openDocs = m_SwApp.GetDocuments() as object[];

            if (openDocs != null)
            {
                foreach (IModelDoc2 model in openDocs)
                {
                    AttachDocument(model);
                }
            }
        }

        private void AttachDocument(IModelDoc2 model)
        {
            if (!m_Documents.ContainsKey(model))
            {
                m_DocsDispatcher.Dispatch(model);
            }
            else
            {
                m_Logger.Log($"Conflict. {model.GetTitle()} already registered");
                Debug.Assert(false, "Document was not unregistered");
            }
        }

        private void OnDocumentDispatched(SwDocument doc)
        {
            lock (m_Lock)
            {
                if (!m_Documents.ContainsKey(doc.Model))
                {
                    doc.Closing += OnDocumentDestroyed;

                    m_Documents.Add(doc.Model, doc);

                    m_DocsHandler.InitHandlers(doc);

                    DocumentCreated?.Invoke(doc);
                }
                else 
                {
                    m_Logger.Log($"Conflict. {doc.Model.GetTitle()} already dispatched");
                    Debug.Assert(false, "Document already dispatched");
                }
            }
        }
        
        private int OnDocumentLoadNotify2(string docTitle, string docPath)
        {
            IModelDoc2 model;

            if (!string.IsNullOrEmpty(docPath))
            {
                model = m_SwApp.GetOpenDocumentByName(docPath) as IModelDoc2;
            }
            else
            {
                model = (m_SwApp.GetDocuments() as object[])?.FirstOrDefault(
                    d => string.Equals((d as IModelDoc2).GetTitle(), docTitle)) as IModelDoc2;
            }

            if (model == null)
            {
                throw new NullReferenceException($"Failed to find the loaded model: {docTitle} ({docPath})");
            }

            AttachDocument(model);

            return S_OK;
        }

        private void OnDocumentDestroyed(IXDocument model)
        {
            ReleaseDocument(((ISwDocument)model).Model);
        }

        private void ReleaseDocument(IModelDoc2 model)
        {
            var doc = this[model];
            doc.Closing -= OnDocumentDestroyed;
            m_Documents.Remove(model);
            m_DocsHandler.ReleaseHandlers(doc);
            doc.Dispose();
        }

        public void RegisterHandler<THandler>() where THandler : IDocumentHandler, new()
        {
            m_DocsHandler.RegisterHandler<THandler>();
        }

        public THandler GetHandler<THandler>(IXDocument doc) where THandler : IDocumentHandler, new()
        {
            return m_DocsHandler.GetHandler<THandler>(doc);
        }

        public TDocument PreCreate<TDocument>()
             where TDocument : class, IXDocument
        {
            SwDocument templateDoc = null;

            if (typeof(IXPart).IsAssignableFrom(typeof(TDocument)))
            {
                templateDoc = new SwPart(null, m_App, m_Logger, false);
            }
            else if (typeof(IXAssembly).IsAssignableFrom(typeof(TDocument)))
            {
                templateDoc = new SwAssembly(null, m_App, m_Logger, false);
            }
            else if (typeof(IXDrawing).IsAssignableFrom(typeof(TDocument)))
            {
                templateDoc = new SwDrawing(null, m_App, m_Logger, false);
            }
            else if (typeof(IXDocument).IsAssignableFrom(typeof(TDocument)) 
                || typeof(IXUnknownDocument).IsAssignableFrom(typeof(TDocument)))
            {
                templateDoc = new SwUnknownDocument(null, m_App, m_Logger, false);
            }
            else
            {
                throw new NotSupportedException("Creation of this type of document is not supported");
            }

            templateDoc.SetDispatcher(m_DocsDispatcher);

            return templateDoc as TDocument;
        }
        
        public bool TryGet(string name, out IXDocument ent)
        {
            var model = m_SwApp.GetOpenDocumentByName(name) as IModelDoc2;
            ent = null;

            if (model != null)
            {
                SwDocument doc;

                if (m_Documents.TryGetValue(model, out doc))
                {
                    ent = doc;
                    return true;
                }
            }

            return false;
        }

        public void AddRange(IEnumerable<IXDocument> ents)
        {
            foreach (SwDocument doc in ents) 
            {
                doc.Commit();
            }
        }

        public void RemoveRange(IEnumerable<IXDocument> ents)
        {
            throw new NotImplementedException();
        }
    }
}