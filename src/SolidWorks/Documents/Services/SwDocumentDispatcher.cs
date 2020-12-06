//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Documents.Services
{
    /// <summary>
    /// This services dispatches the model docs and creates SwDocument objects
    /// </summary>
    /// <remarks>This service is also responsible to using the objects pre-created templates wheer applicable instead of creating new ones.
    /// DocumentLoadNotify2 even is fired async so it is not ensured that it is raised before or after OpenDoc6 or NewDocument APIs. This services is respnsibel for handlign the race conditions</remarks>
    internal class SwDocumentDispatcher
    {
        internal event Action<SwDocument> Dispatched;

        private readonly List<SwDocument> m_DocsDispatchQueue;
        private readonly List<IModelDoc2> m_ModelsDispatchQueue;

        private readonly object m_Lock;

        private readonly ISwApplication m_App;
        private readonly IXLogger m_Logger;

        private readonly IEqualityComparer<IModelDoc2> m_Comparer;
                
        internal SwDocumentDispatcher(ISwApplication app, IXLogger logger)
        {
            m_App = app;
            m_Logger = logger;

            m_Comparer = new SwModelPointerEqualityComparer(app.Sw);

            m_DocsDispatchQueue = new List<SwDocument>();
            m_ModelsDispatchQueue = new List<IModelDoc2>();

            m_Lock = new object();
        }

        internal void Dispatch(IModelDoc2 model) 
        {
            lock (m_Lock) 
            {
                m_Logger.Log($"Adding '{model.GetTitle()}' to the dispatch queue");

                m_ModelsDispatchQueue.Add(model);

                if (!m_DocsDispatchQueue.Any())
                {
                    DispatchAllModels();
                }
            }
        }

        internal void BeginDispatch(SwDocument doc) 
        {
            m_DocsDispatchQueue.Add(doc);
        }

        internal void EndDispatch(SwDocument doc) 
        {
            lock (m_Lock)
            {
                m_DocsDispatchQueue.Remove(doc);

                var index = m_ModelsDispatchQueue.FindIndex(d => m_Comparer.Equals(d, doc.Model));

                if (index != -1) 
                {
                    m_Logger.Log($"Removing '{doc.Title}' from the dispatch queue");

                    m_ModelsDispatchQueue.RemoveAt(index);
                }

                if (doc.IsCommitted)
                {
                    if (doc is SwUnknownDocument)
                    {
                        doc = (SwDocument)(doc as SwUnknownDocument).GetSpecific();
                    }
                    else
                    {
                        doc.AttachEvents();
                    }

                    NotifyDispatchedSafe(doc);
                }

                if (!m_DocsDispatchQueue.Any()) 
                {
                    DispatchAllModels();
                }
            }
        }

        private void DispatchAllModels() 
        {
            lock (m_Lock) 
            {
                m_Logger.Log($"Dispatching all ({m_ModelsDispatchQueue.Count}) models");

                foreach (var model in m_ModelsDispatchQueue)
                {
                    SwDocument doc;

                    switch (model)
                    {
                        case IPartDoc part:
                            doc = new SwPart(part, (SwApplication)m_App, m_Logger, true);
                            break;

                        case IAssemblyDoc assm:
                            doc = new SwAssembly(assm, (SwApplication)m_App, m_Logger, true);
                            break;

                        case IDrawingDoc drw:
                            doc = new SwDrawing(drw, (SwApplication)m_App, m_Logger, true);
                            break;

                        default:
                            throw new NotSupportedException();
                    }

                    NotifyDispatchedSafe(doc);
                }

                m_ModelsDispatchQueue.Clear();
                m_Logger.Log($"Cleared models queue");
            }
        }

        private void NotifyDispatchedSafe(SwDocument doc)
        {
            try
            {
                m_Logger.Log($"Dispatched '{doc.Title}'");
                Dispatched?.Invoke(doc);
            }
            catch (Exception ex)
            {
                m_Logger.Log($"Unhandled exception while dispatching the document '{doc.Title}'");
                m_Logger.Log(ex);
            }
        }
    }
}
