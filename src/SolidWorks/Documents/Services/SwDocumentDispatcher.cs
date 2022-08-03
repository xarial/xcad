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
        private class ModelInfo 
        {
            internal string Title { get; }
            internal string Path { get; }
            internal IModelDoc2 Model { get; }

            internal ModelInfo(string title, string path, IModelDoc2 model)
            {
                Title = title;
                Path = path;
                Model = model;
            }
        }

        internal event Action<SwDocument> Dispatched;

        private readonly List<SwDocument> m_DocsDispatchQueue;
        private readonly List<ModelInfo> m_ModelsDispatchQueue;

        private readonly object m_Lock;

        private readonly SwApplication m_App;
        private readonly IXLogger m_Logger;
        
        internal SwDocumentDispatcher(SwApplication app, IXLogger logger)
        {
            m_App = app;
            m_Logger = logger;

            m_DocsDispatchQueue = new List<SwDocument>();
            m_ModelsDispatchQueue = new List<ModelInfo>();

            m_Lock = new object();
        }

        /// <summary>
        /// Dispatches the loaded document
        /// </summary>
        /// <param name="title">Title of the document</param>
        /// <param name="path">Path of the document</param>
        /// <remarks>It is not safe to dispatch the pointer to IModelDoc2 as for assembly documents it can cause RPC_E_WRONG_THREAD when retrieved on EndDispatch</remarks>
        internal void Dispatch(string title, string path) 
        {
            lock (m_Lock) 
            {
                m_Logger.Log($"Adding '{title}' to the dispatch queue", LoggerMessageSeverity_e.Debug);

                m_ModelsDispatchQueue.Add(new ModelInfo(title, path, m_App.Sw.GetOpenDocument(title)));
                
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

        /// <summary>
        /// Removes the document from the queue
        /// </summary>
        /// <param name="doc">Document to remove from the queue</param>
        /// <param name="model">Actual pointer to the model. If null system will try to find the matching model</param>
        internal void EndDispatch(SwDocument doc, IModelDoc2 model = null) 
        {
            lock (m_Lock)
            {
                m_DocsDispatchQueue.Remove(doc);

                int index;

                if (model != null)
                {
                    index = m_ModelsDispatchQueue.FindIndex(i =>
                        (!string.IsNullOrEmpty(i.Path) && string.Equals(System.IO.Path.GetFileNameWithoutExtension(model.GetPathName()), System.IO.Path.GetFileNameWithoutExtension(i.Path), StringComparison.CurrentCultureIgnoreCase))
                        || string.Equals(System.IO.Path.GetFileNameWithoutExtension(model.GetTitle()), System.IO.Path.GetFileNameWithoutExtension(i.Title), StringComparison.CurrentCultureIgnoreCase));

                    if (index == -1)
                    {
                        index = m_ModelsDispatchQueue.FindIndex(m =>
                            string.Equals(System.IO.Path.GetFileNameWithoutExtension(model.GetTitle()),
                            System.IO.Path.GetFileNameWithoutExtension(m.Model?.GetTitle()), StringComparison.CurrentCultureIgnoreCase));
                    }
                }
                else 
                {
                    index = m_ModelsDispatchQueue.FindIndex(
                        d => (!string.IsNullOrEmpty(d.Path) && string.Equals(d.Path, doc.Path, StringComparison.CurrentCultureIgnoreCase))
                            || string.Equals(d.Title, doc.Title, StringComparison.CurrentCultureIgnoreCase));
                }

                if (index != -1)
                {
                    m_Logger.Log($"Removing '{doc.Title}' from the dispatch queue", LoggerMessageSeverity_e.Debug);

                    var modelInfo = m_ModelsDispatchQueue[index];

                    m_ModelsDispatchQueue.RemoveAt(index);

                    if (!doc.IsCommitted)
                    {
                        if (model != null || TryFindModel(modelInfo, out model))
                        {
                            doc.SetModel(model);
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(false);
                            m_Logger.Log($"Failed to find the loaded model: {modelInfo.Title} ({modelInfo.Path})", LoggerMessageSeverity_e.Error);
                        }
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

        private bool TryFindModel(ModelInfo info, out IModelDoc2 model) 
        {
            if (!string.IsNullOrEmpty(info.Path))
            {
                model = m_App.Sw.GetOpenDocumentByName(info.Path) as IModelDoc2;
            }
            else
            {
                var docs = (m_App.Sw.GetDocuments() as object[] ?? new object[0]).Cast<IModelDoc2>().ToArray();

                model = docs.FirstOrDefault(
                    d => string.Equals(
                        System.IO.Path.GetFileNameWithoutExtension(d.GetTitle()),
                        System.IO.Path.GetFileNameWithoutExtension(info.Title),
                        StringComparison.CurrentCultureIgnoreCase));
            }

            return model != null;
        }
        
        private void DispatchAllModels()
        {
            lock (m_Lock) 
            {
                m_Logger.Log($"Dispatching all ({m_ModelsDispatchQueue.Count}) models", LoggerMessageSeverity_e.Debug);

                var errors = new List<Exception>();

                foreach (var modelInfo in m_ModelsDispatchQueue)
                {
                    SwDocument doc;

                    if (TryFindModel(modelInfo, out IModelDoc2 model))
                    {
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

                            case null:
                                errors.Add(new NullReferenceException("Model is null"));
                                continue;

                            default:
                                errors.Add(new NotSupportedException($"Invalid cast of '{modelInfo.Path}' [{modelInfo.Title}] of type '{((object)model).GetType().FullName}'. Specific document type: {(swDocumentTypes_e)model.GetType()}"));
                                continue;
                        }

                        NotifyDispatchedSafe(doc);
                    }
                    else 
                    {
                        m_Logger.Log($"Failed to find the loaded model: {modelInfo.Title} ({modelInfo.Path}). This may be due to the external reference which is not loaded", LoggerMessageSeverity_e.Error);
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
