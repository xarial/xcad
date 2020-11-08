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

            m_Comparer = new SwPointerEqualityComparer<IModelDoc2>(app.Sw);

            m_DocsDispatchQueue = new List<SwDocument>();
            m_ModelsDispatchQueue = new List<IModelDoc2>();

            m_Lock = new object();
        }

        internal void Dispatch(IModelDoc2 model) 
        {
            lock (m_Lock) 
            {
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
                    m_ModelsDispatchQueue.RemoveAt(index);
                }

                if (doc is SwUnknownDocument) 
                {
                    doc = (SwDocument)(doc as SwUnknownDocument).GetSpecific();
                }

                Dispatched?.Invoke(doc);

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
                foreach (var model in m_ModelsDispatchQueue) 
                {
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
                            throw new NotSupportedException();
                    }

                    Dispatched?.Invoke(doc);
                }

                m_ModelsDispatchQueue.Clear();
            }
        }
    }
}
