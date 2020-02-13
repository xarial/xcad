//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Services;

namespace Xarial.XCad.Toolkit.Services
{
    public class DocumentsHandler
    {
        private readonly IXApplication m_App;

        private readonly List<Type> m_Handlers;
        private readonly Dictionary<IXDocument, List<IDocumentHandler>> m_DocsMap;

        public DocumentsHandler(IXApplication app) 
        {
            m_App = app;
            m_Handlers = new List<Type>();
            m_DocsMap = new Dictionary<IXDocument, List<IDocumentHandler>>();
        }

        public void RegisterHandler<THandler>() where THandler : IDocumentHandler, new() 
        {
            var type = typeof(THandler);

            if (!m_Handlers.Contains(type))
            {
                m_Handlers.Add(type);

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
            where THandler : IDocumentHandler, new()
        {
            var handlers = m_DocsMap[doc];

            return (THandler)handlers.First(h => h.GetType() == typeof(THandler));
        }

        public void InitHandlers(IXDocument doc) 
        {
            var handlers = m_Handlers.Select(t => CreateHandler(doc, t)).ToList();

            m_DocsMap.Add(doc, handlers);
        }

        public void ReleaseHandlers(IXDocument doc)
        {
            foreach (var handler in m_DocsMap[doc])
            {
                handler.Dispose();
            }

            m_DocsMap[doc].Clear();
            m_DocsMap.Remove(doc);
        }

        private IDocumentHandler CreateHandler(IXDocument doc, Type type)
        {
            var handler = (IDocumentHandler)Activator.CreateInstance(type);
            handler.Init(m_App, doc);
            return handler;
        }
    }
}
