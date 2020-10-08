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
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Utils.Diagnostics;

namespace Xarial.XCad.SolidWorks.Documents
{
    [DebuggerDisplay("Documents: {" + nameof(Count) + "}")]
    public class SwDocumentCollection : IXDocumentCollection, IDisposable
    {
        public event DocumentCreateDelegate DocumentCreated;
        public event DocumentActivateDelegate DocumentActivated;

        IXDocument IXDocumentCollection.Active 
        {
            get => Active;
            set => Active = (SwDocument)value;
        }

        IXDocument IXDocumentCollection.Open(DocumentOpenArgs args) => Open(args);

        private const int S_OK = 0;

        private readonly SwApplication m_App;
        private readonly SldWorks m_SwApp;
        private readonly Dictionary<IModelDoc2, SwDocument> m_Documents;
        private readonly IXLogger m_Logger;
        private readonly DocumentsHandler m_DocsHandler;

        public SwDocument Active
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

        private readonly Dictionary<string, swDocumentTypes_e> m_NativeFileExts;
        
        internal SwDocumentCollection(SwApplication app, IXLogger logger)
        {
            m_NativeFileExts = new Dictionary<string, swDocumentTypes_e>(StringComparer.CurrentCultureIgnoreCase)
            {
                { ".sldprt", swDocumentTypes_e.swDocPART },
                { ".sldasm", swDocumentTypes_e.swDocASSEMBLY },
                { ".slddrw", swDocumentTypes_e.swDocDRAWING },
                { ".sldlfp", swDocumentTypes_e.swDocPART },
                { ".sldblk", swDocumentTypes_e.swDocPART }
            };
            
            m_App = app;
            m_SwApp = (SldWorks)m_App.Sw;
            m_Logger = logger;

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
            IModelDoc2 model = null;
            int errorCode = -1;

            if (m_NativeFileExts.TryGetValue(Path.GetExtension(args.Path), out swDocumentTypes_e docType))
            {
                swOpenDocOptions_e opts = 0;
                
                if (args.ReadOnly) 
                {
                    opts |= swOpenDocOptions_e.swOpenDocOptions_ReadOnly;
                }
                
                if (args.ViewOnly)
                {
                    opts |= swOpenDocOptions_e.swOpenDocOptions_ViewOnly;
                }

                if (args.Silent)
                {
                    opts |= swOpenDocOptions_e.swOpenDocOptions_Silent;
                }

                if (args.Rapid)
                {
                    if (docType == swDocumentTypes_e.swDocDRAWING)
                    {
                        if (m_App.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2020))
                        {
                            opts |= swOpenDocOptions_e.swOpenDocOptions_OpenDetailingMode;
                        }
                    }
                    else if (docType == swDocumentTypes_e.swDocASSEMBLY)
                    {
                        if (m_App.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2020))
                        {
                            //TODO: this option should be implemented as 'Large Design Review' (swOpenDocOptions_ViewOnly) with 'Edit Assembly Option'. Later option is not available in API
                        }
                    }
                    else if (docType == swDocumentTypes_e.swDocPART)
                    {
                        //There is no rapid option for SOLIDWORKS part document
                    }
                }

                int warns = -1;
                model = m_SwApp.OpenDoc6(args.Path, (int)docType, (int)opts, "", ref errorCode, ref warns);
            }
            else 
            {
                model = m_SwApp.LoadFile4(args.Path, "", null, ref errorCode);
            }
            
            if (model == null) 
            {
                string error = "";
                
                switch ((swFileLoadError_e)errorCode) 
                {
                    case swFileLoadError_e.swAddinInteruptError:
                        error = "File opening was interrupted by the user";
                        break;
                    case swFileLoadError_e.swApplicationBusy:
                        error = "Application is busy";
                        break;
                    case swFileLoadError_e.swFileCriticalDataRepairError:
                        error = "File has critical data corruption";
                        break;
                    case swFileLoadError_e.swFileNotFoundError:
                        error = "File not found at the specified path";
                        break;
                    case swFileLoadError_e.swFileRequiresRepairError:
                        error = "File has non-critical custom property data corruption";
                        break;
                    case swFileLoadError_e.swFileWithSameTitleAlreadyOpen:
                        error = "A document with the same name is already open";
                        break;
                    case swFileLoadError_e.swFutureVersion:
                        error = "The document was saved in a future version of SOLIDWORKS";
                        break;
                    case swFileLoadError_e.swGenericError:
                        error = "Unknown error while opening file";
                        break;
                    case swFileLoadError_e.swInvalidFileTypeError:
                        error = "Invalid file type";
                        break;
                    case swFileLoadError_e.swLiquidMachineDoc:
                        error = "File encrypted by Liquid Machines";
                        break;
                    case swFileLoadError_e.swLowResourcesError:
                        error = "File is open and blocked because the system memory is low, or the number of GDI handles has exceeded the allowed maximum";
                        break;
                    case swFileLoadError_e.swNoDisplayData:
                        error = "File contains no display data";
                        break;
                }

                throw new OpenDocumentFailedException(args.Path, errorCode, error);
            }

            return this[model];
        }

        public void Dispose()
        {
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

                m_DocsHandler.InitHandlers(doc);

                DocumentCreated?.Invoke(doc);
            }
            else
            {
                m_Logger.Log($"Conflict. {model.GetTitle()} already registered");
                Debug.Assert(false, "Document was not unregistered");
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