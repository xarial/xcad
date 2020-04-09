//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Documents.Services;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Utils.Diagnostics;

namespace Xarial.XCad.SolidWorks.Documents
{
    [DebuggerDisplay("Documents: {" + nameof(Count) + "}")]
    public class SwDocumentCollection : IXDocumentCollection, IDisposable
    {
        public event DocumentCreateDelegate DocumentCreated;

        IXDocument IXDocumentCollection.Active => Active;
        IXDocument IXDocumentCollection.Open(DocumentOpenArgs args) => Open(args);

        private readonly SldWorks m_App;
        private readonly Dictionary<IModelDoc2, SwDocument> m_Documents;
        private readonly ILogger m_Logger;
        private readonly DocumentsHandler m_DocsHandler;

        public SwDocument Active
        {
            get
            {
                var activeDoc = m_App.IActiveDoc2;

                if (activeDoc != null)
                {
                    return this[activeDoc];
                }
                else
                {
                    return null;
                }
            }
        }

        public int Count => m_Documents.Count;

        internal SwDocumentCollection(SwApplication app, ILogger logger)
        {
            m_App = (SldWorks)app.Sw;
            m_Logger = logger;

            m_Documents = new Dictionary<IModelDoc2, SwDocument>(
                new SwPointerEqualityComparer<IModelDoc2>(m_App));
            m_DocsHandler = new DocumentsHandler(app);
            AttachToAllOpenedDocuments();

            m_App.DocumentLoadNotify2 += OnDocumentLoadNotify2;
        }

        public SwDocument this[IModelDoc2 model]
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

        public SwDocument Open(DocumentOpenArgs args)
        {
            var docSpec = m_App.GetOpenDocSpec(args.Path) as IDocumentSpecification;

            docSpec.ReadOnly = args.ReadOnly;
            docSpec.ViewOnly = args.ViewOnly;
            var model = m_App.OpenDoc7(docSpec);

            return this[model];
        }

        public void Dispose()
        {
            m_App.DocumentLoadNotify2 -= OnDocumentLoadNotify2;

            foreach (var doc in m_Documents.Keys.ToArray())
            {
                ReleaseDocument(doc);
            }

            m_Documents.Clear();
        }

        private void AttachToAllOpenedDocuments()
        {
            var openDocs = m_App.GetDocuments() as object[];

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
                SwDocument doc = null;

                switch (model)
                {
                    case IPartDoc part:
                        doc = new SwPart(part, m_App, m_Logger);
                        break;

                    case IAssemblyDoc assm:
                        doc = new SwAssembly(assm, m_App, m_Logger);
                        break;

                    case IDrawingDoc drw:
                        doc = new SwDrawing(drw, m_App, m_Logger);
                        break;

                    default:
                        throw new NotSupportedException();
                }

                doc.Destroyed += OnDocumentDestroyed;

                m_Documents.Add(model, doc);

                DocumentCreated?.Invoke(doc);

                m_DocsHandler.InitHandlers(doc);
            }
            else
            {
                m_Logger.Log($"Conflict. {model.GetTitle()} already registered");
                Debug.Assert(false, "Document was not unregistered");
            }
        }

        private int OnDocumentLoadNotify2(string docTitle, string docPath)
        {
            const int S_OK = 0;

            IModelDoc2 model;

            if (!string.IsNullOrEmpty(docPath))
            {
                model = m_App.GetOpenDocumentByName(docPath) as IModelDoc2;
            }
            else
            {
                model = (m_App.GetDocuments() as object[])?.FirstOrDefault(
                    d => string.Equals((d as IModelDoc2).GetTitle(), docTitle)) as IModelDoc2;
            }

            if (model == null)
            {
                throw new NullReferenceException($"Failed to find the loaded model: {docTitle} ({docPath})");
            }

            AttachDocument(model);

            return S_OK;
        }

        private void OnDocumentDestroyed(IModelDoc2 model)
        {
            ReleaseDocument(model);
        }

        private void ReleaseDocument(IModelDoc2 model)
        {
            var doc = this[model];
            doc.Destroyed -= OnDocumentDestroyed;
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
    }
}