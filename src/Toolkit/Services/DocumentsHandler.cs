//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Services;

namespace Xarial.XCad.Toolkit.Services
{
    public class DocumentsHandler
    {
        private readonly IXApplication m_App;

        private readonly Dictionary<Type, Delegate> m_Handlers;
        private readonly Dictionary<IXDocument, List<IDocumentHandler>> m_DocsMap;

        private readonly IXLogger m_Logger;

        public DocumentsHandler(IXApplication app, IXLogger logger) 
        {
            m_App = app;
            m_Logger = logger;

            m_Handlers = new Dictionary<Type, Delegate>();
            m_DocsMap = new Dictionary<IXDocument, List<IDocumentHandler>>();
        }

        public void RegisterHandler<THandler>(Func<THandler> handlerFact) where THandler : IDocumentHandler
        {
            var type = typeof(THandler);

            if (!m_Handlers.ContainsKey(type))
            {
                m_Handlers.Add(type, handlerFact);

                foreach (var map in m_DocsMap) 
                {
                    var handler = CreateHandler(map.Key, type);
                    map.Value.Add(handler);
                }
            }
            else 
            {
                throw new Exception("Handler for this type is already registered");
            }
        }

        public THandler GetHandler<THandler>(IXDocument doc) 
            where THandler : IDocumentHandler
        {
            var handlers = m_DocsMap[doc];

            return (THandler)handlers.First(h => h.GetType() == typeof(THandler));
        }

        public void TryInitHandlers(IXDocument doc) 
        {
            var handlers = new List<IDocumentHandler>();

            foreach (var type in m_Handlers.Keys) 
            {
                try
                {
                    var handler = CreateHandler(doc, type);
                    handlers.Add(handler);
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
            foreach (var handler in m_DocsMap[doc])
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

            m_DocsMap[doc].Clear();
            m_DocsMap.Remove(doc);
        }

        private IDocumentHandler CreateHandler(IXDocument doc, Type type)
        {
            var handler = (IDocumentHandler)m_Handlers[type].DynamicInvoke();
            handler.Init(m_App, doc);
            return handler;
        }
    }
}
