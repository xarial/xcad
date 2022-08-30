//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.SolidWorks.Documents.Exceptions;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Documents.Services
{
    /// <summary>
    /// This services dispatches the model docs and creates SwDocument objects
    /// </summary>
    /// <remarks>This service is also responsible to using the objects pre-created templates where applicable instead of creating new ones.
    /// DocumentLoadNotify2 even is fired async so it is not ensured that it is raised before or after OpenDoc6 or NewDocument APIs. This services is responsible for handling the race conditions</remarks>
    internal class SwDocumentDispatcher
    {
        internal event Action<SwDocument> Dispatched;

        private readonly List<SwDocument> m_DocsDispatchQueue;
        private readonly List<IModelDoc2> m_ModelsDispatchQueue;

        private readonly object m_Lock;

        private readonly SwApplication m_App;
        private readonly IXLogger m_Logger;
        
        internal SwDocumentDispatcher(SwApplication app, IXLogger logger)
        {
            m_App = app;
            m_Logger = logger;

            m_DocsDispatchQueue = new List<SwDocument>();
            m_ModelsDispatchQueue = new List<IModelDoc2>();

            m_Lock = new object();
        }

        /// <summary>
        /// Dispatches the loaded document
        /// </summary>
        /// <param name="model">Model to add to the queue</param>
        /// <remarks>It is not safe to reuse the pointer to IModelDoc2 as for assembly documents it can cause RPC_E_WRONG_THREAD when retrieved on EndDispatch</remarks>
        internal void Dispatch(IModelDoc2 model) 
        {
            if (model == null) 
            {
                throw new ArgumentNullException(nameof(model));
            }

            lock (m_Lock) 
            {
                m_Logger.Log($"Adding '{model.GetTitle()}' to the dispatch queue", LoggerMessageSeverity_e.Debug);

                m_ModelsDispatchQueue.Add(model);
                
                if (!m_DocsDispatchQueue.Any())
                {
                    DispatchAllModels();
                }
            }
        }

        /// <summary>
        /// Puts the document into the dispatch queue
        /// </summary>
        /// <param name="doc">Document to put into the queue</param>
        internal void BeginDispatch(SwDocument doc) => m_DocsDispatchQueue.Add(doc);

        internal void TryRemoveFromDispatchQueue(SwDocument doc)
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
        /// <param name="model">Actual pointer to the model. If null system will try to find the matching model</param>
        internal void EndDispatch(SwDocument doc, IModelDoc2 model) 
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            lock (m_Lock)
            {
                m_DocsDispatchQueue.Remove(doc);

                //NOTE: it might not be enough to compare the pointers. When LoadNotify2 event is called from different threads pointers might not be equal
                var index = m_ModelsDispatchQueue.FindIndex(m => m_App.Sw.IsSame(m, model) == (int)swObjectEquality.swObjectSame);

                if (index != -1)
                {
                    m_Logger.Log($"Removing '{doc.Title}' from the dispatch queue", LoggerMessageSeverity_e.Debug);

                    m_ModelsDispatchQueue.RemoveAt(index);

                    if (!doc.IsCommitted)
                    {
                        doc.SetModel(model);
                    }
                }
                else 
                {
                    m_Logger.Log($"Document '{doc.Title}' is not in the dispatch queue", LoggerMessageSeverity_e.Warning);
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

        internal SwDocument RegisterModel(IModelDoc2 model) 
        {
            if (model == null) 
            {
                throw new NullReferenceException("Model is null");
            }

            SwDocument doc;

            switch (model)
            {
                case IPartDoc part:
                    doc = new SwPart(part, m_App, m_Logger, true);
                    break;

                case IAssemblyDoc assm:
                    doc = new SwAssembly(assm, m_App, m_Logger, true);
                    break;

                case IDrawingDoc drw:
                    doc = new SwDrawing(drw, m_App, m_Logger, true);
                    break;

                default:
                    throw new NotSupportedException($"Invalid cast of '{model.GetPathName()}' [{model.GetTitle()}] of type '{((object)model).GetType().FullName}'. Specific document type: {(swDocumentTypes_e)model.GetType()}");
            }

            NotifyDispatchedSafe(doc);

            return doc;
        }

        private void DispatchAllModels()
        {
            lock (m_Lock) 
            {
                m_Logger.Log($"Dispatching all ({m_ModelsDispatchQueue.Count}) models", LoggerMessageSeverity_e.Debug);

                var errors = new List<Exception>();

                foreach (var model in m_ModelsDispatchQueue)
                {
                    try
                    {
                        RegisterModel(model);
                    }
                    catch (Exception ex)
                    {
                        errors.Add(ex);
                    }
                }

                m_ModelsDispatchQueue.Clear();
                m_Logger.Log($"Cleared models queue",LoggerMessageSeverity_e.Debug);

                if (errors.Any()) 
                {
                    throw new DocumentsQueueDispatchException(errors.ToArray());
                }
            }
        }

        private void NotifyDispatchedSafe(SwDocument doc)
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
    }
}
