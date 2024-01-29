//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
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
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Documents.Exceptions;
using Xarial.XCad.Documents.Services;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.Exceptions;
using Xarial.XCad.SolidWorks.Documents.Services;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Utils;
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
        public event DocumentEventDelegate DocumentLoaded
        {
            add
            {
                if (m_DocumentLoaded == null)
                {
                    m_SwApp.DocumentLoadNotify2 += OnDocumentLoadNotify2;
                }

                m_DocumentLoaded += value;
            }
            remove
            {
                m_DocumentLoaded -= value;

                if (m_DocumentLoaded == null)
                {
                    m_SwApp.DocumentLoadNotify2 -= OnDocumentLoadNotify2;
                }
            }
        }

        public event DocumentEventDelegate DocumentActivated 
        {
            add 
            {
                if (m_DocumentActivated == null) 
                {
                    m_SwApp.ActiveModelDocChangeNotify += OnActiveModelDocChangeNotify;
                }

                m_DocumentActivated += value;
            }
            remove 
            {
                m_DocumentActivated -= value;

                if (m_DocumentActivated == null)
                {
                    m_SwApp.ActiveModelDocChangeNotify -= OnActiveModelDocChangeNotify;
                }
            }
        }

        public event DocumentEventDelegate NewDocumentCreated
        {
            add
            {
                if (m_NewDocumentCreated == null)
                {
                    m_SwApp.FileNewNotify2 += OnFileNewNotify;
                }

                m_NewDocumentCreated += value;
            }
            remove
            {
                m_NewDocumentCreated -= value;

                if (m_NewDocumentCreated == null)
                {
                    m_SwApp.FileNewNotify2 -= OnFileNewNotify;
                }
            }
        }

        public event DocumentEventDelegate DocumentOpened
        {
            add
            {
                if (m_DocumentOpened == null)
                {
                    m_SwApp.FileOpenPostNotify += OnFileOpenPostNotify;
                }

                m_DocumentOpened += value;
            }
            remove
            {
                m_DocumentOpened -= value;

                if (m_DocumentOpened == null)
                {
                    m_SwApp.FileOpenPostNotify -= OnFileOpenPostNotify;
                }
            }
        }

        IXDocument IXDocumentRepository.Active 
        {
            get => Active;
            set => Active = (SwDocument)value;
        }
        
        private readonly SwApplication m_App;
        private readonly SldWorks m_SwApp;
        private readonly IXLogger m_Logger;
        private readonly DocumentsHandler m_DocsHandler;

        private DocumentEventDelegate m_DocumentLoaded;
        private DocumentEventDelegate m_DocumentActivated;
        private DocumentEventDelegate m_DocumentOpened;
        private DocumentEventDelegate m_NewDocumentCreated;

        //NOTE: Creation of SwDocument has some additional API calls (e.g. subscribing the save event, caching the path)
        //this may have a performance effect when called very often (e.g. within the IXCommandGroup.CommandStateResolve)
        //cahcing of the document allows to reuse the instance and improves the performance
        private IModelDoc2 m_CachedNativeDoc;
        private SwDocument m_CachedDoc;

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
                var doc = m_SwApp.ActivateDoc3(value.Title, true, (int)swRebuildOnActivation_e.swDontRebuildActiveDoc,
                    ref errors);

                if (doc == null) 
                {
                    throw new Exception($"Failed to activate the document. Error code: {errors}");
                }
            }
        }

        public int Count => m_SwApp.GetDocumentCount();

        public IXDocument this[string name] => RepositoryHelper.Get(this, name);

        internal SwDocumentCollection(SwApplication app, IXLogger logger)
        {
            m_App = app;
            m_SwApp = (SldWorks)m_App.Sw;
            m_Logger = logger;

            m_DocsHandler = new DocumentsHandler(app, m_Logger);
        }
                
        private int OnActiveModelDocChangeNotify()
        {
            var activeDoc = m_SwApp.IActiveDoc2;

            try
            {
                m_DocumentActivated?.Invoke(CreateDocument(activeDoc));
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
            }

            return HResult.S_OK;
        }

        public ISwDocument this[IModelDoc2 model] => CreateDocument(model);

        public IEnumerator<IXDocument> GetEnumerator()
        {
            var openDocs = m_SwApp.GetDocuments() as object[];

            if (openDocs != null)
            {
                foreach (IModelDoc2 model in openDocs)
                {
                    yield return CreateDocument(model);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) 
            => RepositoryHelper.FilterDefault(this, filters, reverseOrder);

        private int OnFileOpenPostNotify(string fileName)
        {
            try
            {
                m_DocumentOpened?.Invoke(CreateDocument(FindModel(fileName, fileName)));
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
            }

            return HResult.S_OK;
        }

        private int OnFileNewNotify(object newDoc, int docType, string templateName)
        {
            try
            {
                m_NewDocumentCreated?.Invoke(CreateDocument((IModelDoc2)newDoc));
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
            }

            return HResult.S_OK;
        }

        private int OnDocumentLoadNotify2(string docTitle, string docPath)
        {
            try
            {
                m_DocumentLoaded?.Invoke(CreateDocument(FindModel(docTitle, docPath)));
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
            }

            return HResult.S_OK;
        }

        private ModelDoc2 FindModel(string docTitle, string docPath)
        {
            var docName = docPath;

            if (string.IsNullOrEmpty(docName))
            {
                docName = docTitle;
            }

            var foundModel = m_App.Sw.GetOpenDocument(docName);

            if (foundModel != null)
            {
                return foundModel;
            }
            else
            {
                foreach (ModelDoc2 model in m_App.Sw.GetDocuments() as object[] ?? new object[0])
                {
                    if (!string.IsNullOrEmpty(docPath))
                    {
                        if (string.Equals(model.GetPathName(), docPath, StringComparison.CurrentCultureIgnoreCase))
                        {
                            return model;
                        }
                    }
                    else if (!string.IsNullOrEmpty(docTitle))
                    {
                        if (string.Equals(model.GetTitle(), docTitle, StringComparison.CurrentCultureIgnoreCase))
                        {
                            return model;
                        }
                    }
                }
            }

            throw new Exception($"Failed to find the document by title and path: {docTitle} [{docPath}]");
        }

        public void RegisterHandler<THandler>(Func<THandler> handlerFact) 
            where THandler : IDocumentHandler
            => m_DocsHandler.RegisterHandler(handlerFact);

        public void UnregisterHandler<THandler>()
            where THandler : IDocumentHandler
            => m_DocsHandler.UnregisterHandler<THandler>();

        public THandler GetHandler<THandler>(IXDocument doc) 
            where THandler : IDocumentHandler
            => m_DocsHandler.GetHandler<THandler>(doc);

        public T PreCreate<T>() where T : IXDocument
        {
            var doc = RepositoryHelper.PreCreate<IXDocument, T>(this,
                () => new SwUnknownDocument(null, m_App, m_Logger, false),
                () => new SwUnknownDocument3D(null, m_App, m_Logger, false),
                () => new SwPart(null, m_App, m_Logger, false),
                () => new SwAssembly(null, m_App, m_Logger, false),
                () => new SwDrawing(null, m_App, m_Logger, false));

            if (!(doc is SwDocument))
            {
                throw new InvalidCastException("Document type must be of type SwDocument");
            }

            return doc;
        }

        public bool TryGet(string name, out IXDocument ent)
        {
            IModelDoc2 model = m_SwApp.GetOpenDocument(name);

            if (model == null)
            {
                model = m_SwApp.GetOpenDocumentByName(name) as IModelDoc2;
            }

            if (model != null)
            {
                ent = CreateDocument(model);
                return true;
            }
            else 
            {
                ent = null;
                return false;
            }
        }

        public void AddRange(IEnumerable<IXDocument> ents, CancellationToken cancellationToken) => RepositoryHelper.AddRange(ents, cancellationToken);

        public void RemoveRange(IEnumerable<IXDocument> ents, CancellationToken cancellationToken)
        {
            foreach (var doc in ents.ToArray()) 
            {
                doc.Close();
            }
        }

        internal bool TryFindExistingDocumentByPath(string path, out SwDocument doc)
        {
            if (!string.IsNullOrEmpty(path))
            {
                doc = (SwDocument)this.FirstOrDefault(
                    d => string.Equals(d.Path, path, StringComparison.CurrentCultureIgnoreCase));
            }
            else 
            {
                doc = null;
            }

            return doc != null;
        }

        private SwDocument CreateDocument(IModelDoc2 nativeDoc)
        {
            //NOTE: see the description of the m_CachedNativeDoc field
            if (nativeDoc != null && m_CachedNativeDoc == nativeDoc)
            {
                return m_CachedDoc;
            }
            else
            {
                SwDocument doc;

                switch (nativeDoc)
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
                        throw new NotSupportedException($"Invalid cast of '{nativeDoc.GetPathName()}' [{nativeDoc.GetTitle()}] of type '{((object)nativeDoc).GetType().FullName}'. Specific document type: {(swDocumentTypes_e)nativeDoc.GetType()}");
                }

                m_CachedNativeDoc = nativeDoc;
                m_CachedDoc = doc;

                return doc;
            }
        }

        public void Dispose()
        {
            m_SwApp.DocumentLoadNotify2 -= OnDocumentLoadNotify2;
            m_SwApp.ActiveModelDocChangeNotify -= OnActiveModelDocChangeNotify;
            m_SwApp.FileNewNotify2 -= OnFileNewNotify;
            m_SwApp.FileOpenPostNotify -= OnFileOpenPostNotify;

            m_DocsHandler.Dispose();
        }
    }

    /// <summary>
    /// Additional methods for documents collections
    /// </summary>
    public static class SwDocumentCollectionExtension 
    {
        /// <summary>
        /// Pre creates new document from path
        /// </summary>
        /// <param name="docsColl">Documents collection</param>
        /// <param name="path"></param>
        /// <returns>Pre-created document</returns>
        public static ISwDocument PreCreateFromPath(this ISwDocumentCollection docsColl, string path)
        {
            var ext = Path.GetExtension(path);

            ISwDocument doc;

            switch (ext.ToLower())
            {
                case ".sldprt":
                case ".sldblk":
                case ".prtdot":
                case ".sldlfp":
                    doc = docsColl.PreCreate<ISwPart>();
                    break;

                case ".sldasm":
                case ".asmdot":
                    doc = docsColl.PreCreate<ISwAssembly>();
                    break;

                case ".slddrw":
                case ".drwdot":
                    doc = docsColl.PreCreate<ISwDrawing>();
                    break;

                default:
                    throw new NotSupportedException("Only native documents are supported");
            }

            doc.Path = path;

            return doc;
        }
    }
}