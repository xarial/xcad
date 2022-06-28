//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Attributes;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Services;
using Xarial.XCad.Reflection;

namespace Xarial.XCad.Toolkit.Services
{
    /// <summary>
    /// Handles documents lifecycle
    /// </summary>
    public class DocumentsHandler
    {
        private class DocumentHandlerInfo 
        {
            internal Type HandlerType { get; }
            internal Type[] DocumentTypeFilters { get; }
            internal Delegate Factory { get; }

            internal DocumentHandlerInfo(Type handlerType, Type[] docTypeFilters, Delegate factory) 
            {
                HandlerType = handlerType;
                DocumentTypeFilters = docTypeFilters;
                Factory = factory;
            }
        }

        private readonly IXApplication m_App;

        private readonly List<DocumentHandlerInfo> m_Handlers;
        private readonly Dictionary<IXDocument, List<IDocumentHandler>> m_DocsMap;

        private readonly IXLogger m_Logger;

        public DocumentsHandler(IXApplication app, IXLogger logger) 
        {
            m_App = app;
            m_Logger = logger;

            m_Handlers = new List<DocumentHandlerInfo>();
            m_DocsMap = new Dictionary<IXDocument, List<IDocumentHandler>>();
        }

        public void RegisterHandler<THandler>(Func<THandler> handlerFact) where THandler : IDocumentHandler
        {
            var type = typeof(THandler);

            if (m_Handlers.Any(h => h.HandlerType == type)) 
            {
                throw new Exception("Handler for this type is already registered");
            }

            Type[] filters = null;

            type.TryGetAttribute<DocumentHandlerFilterAttribute>(a => filters = a.Filters);

            var handlerInfo = new DocumentHandlerInfo(type, filters, handlerFact);

            m_Handlers.Add(handlerInfo);

            foreach (var map in m_DocsMap)
            {
                CreateHandler(map.Key, handlerInfo, map.Value);
            }
        }

        public THandler GetHandler<THandler>(IXDocument doc) 
            where THandler : IDocumentHandler
        {
            var handlers = m_DocsMap[doc].Where(h => h.GetType() == typeof(THandler));

            if (handlers.Any())
            {
                Debug.Assert(handlers.Count() == 1, "Must be only one handler of the specified type");

                return (THandler)handlers.First();
            }
            else 
            {
                throw new Exception("Handler of the specified type is not registered for the specified document");
            }
        }

        public void TryInitHandlers(IXDocument doc) 
        {
            var handlers = new List<IDocumentHandler>();

            foreach (var handlerInfo in m_Handlers) 
            {
                try
                {
                    CreateHandler(doc, handlerInfo, handlers);
                }
                catch (Exception ex)
                {
                    m_Logger.Log(ex);
                }
            }

            m_DocsMap.Add(doc, handlers);
        }

        public void ReleaseHandlers(IXDocument doc)
        {
            if (m_DocsMap.TryGetValue(doc, out List<IDocumentHandler> handlers))
            {
                foreach (var handler in handlers)
                {
                    try
                    {
                        handler.Dispose();
                    }
                    catch (Exception ex)
                    {
                        m_Logger.Log(ex);
                    }
                }

                handlers.Clear();
                m_DocsMap.Remove(doc);
            }
            else 
            {
                Debug.Assert(false, "Cannot find the handler of this document. All the documents must be in the map");
            }
        }

        private void CreateHandler(IXDocument doc, DocumentHandlerInfo handlerInfo, List<IDocumentHandler> handlersList)
        {
            var createHandler = false;

            var docType = doc.GetType();

            if (handlerInfo.DocumentTypeFilters == null)
            {
                createHandler = true;
            }
            else 
            {
                createHandler = handlerInfo.DocumentTypeFilters.Any(t => t.IsAssignableFrom(docType));
            }

            if (createHandler)
            {
                m_Logger.Log($"Creating document handler '{handlerInfo.HandlerType.FullName}' for document type: {docType}", LoggerMessageSeverity_e.Debug);

                var handler = (IDocumentHandler)handlerInfo.Factory.DynamicInvoke();
                handler.Init(m_App, doc);
                handlersList.Add(handler);
            }
            else 
            {
                m_Logger.Log($"Skipping creation of document handler '{handlerInfo.HandlerType.FullName}' for document type: {docType}", LoggerMessageSeverity_e.Debug);
            }
        }
    }
}
