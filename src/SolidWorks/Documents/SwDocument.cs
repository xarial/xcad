//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Data;
using Xarial.XCad.Data.Enums;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Exceptions;
using Xarial.XCad.Features;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Annotations;
using Xarial.XCad.SolidWorks.Data;
using Xarial.XCad.SolidWorks.Data.EventHandlers;
using Xarial.XCad.SolidWorks.Documents.EventHandlers;
using Xarial.XCad.SolidWorks.Documents.Services;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Data;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwDocument : IXDocument, IDisposable
    {
        IModelDoc2 Model { get; }
        new ISwFeatureManager Features { get; }
        new ISwSelectionCollection Selections { get; }
        new ISwDimensionsCollection Dimensions { get; }
        new ISwCustomPropertiesCollection Properties { get; }
    }

    [DebuggerDisplay("{" + nameof(Title) + "}")]
    internal abstract class SwDocument : ISwDocument
    {
        protected static Dictionary<string, swDocumentTypes_e> m_NativeFileExts { get; }

        static SwDocument() 
        {
            m_NativeFileExts = new Dictionary<string, swDocumentTypes_e>(StringComparer.CurrentCultureIgnoreCase)
            {
                { ".sldprt", swDocumentTypes_e.swDocPART },
                { ".sldasm", swDocumentTypes_e.swDocASSEMBLY },
                { ".slddrw", swDocumentTypes_e.swDocDRAWING },
                { ".sldlfp", swDocumentTypes_e.swDocPART },
                { ".sldblk", swDocumentTypes_e.swDocPART }
            };
        }

        public event DocumentCloseDelegate Closing;
        
        public event DocumentRebuildDelegate Rebuild 
        {
            add 
            {
                m_DocumentRebuildEventHandler.Attach(value);
            }
            remove 
            {
                m_DocumentRebuildEventHandler.Detach(value);
            }
        }

        public event DocumentSaveDelegate Saving
        {
            add
            {
                m_DocumentSavingEventHandler.Attach(value);
            }
            remove
            {
                m_DocumentSavingEventHandler.Detach(value);
            }
        }

        public event DataStoreAvailableDelegate StreamReadAvailable 
        {
            add 
            {
                m_StreamReadAvailableHandler.Attach(value);
            }
            remove 
            {
                m_StreamReadAvailableHandler.Detach(value);
            }
        }

        public event DataStoreAvailableDelegate StorageReadAvailable
        {
            add
            {
                m_StorageReadAvailableHandler.Attach(value);
            }
            remove
            {
                m_StorageReadAvailableHandler.Detach(value);
            }
        }

        public event DataStoreAvailableDelegate StreamWriteAvailable
        {
            add
            {
                m_StreamWriteAvailableHandler.Attach(value);
            }
            remove
            {
                m_StreamWriteAvailableHandler.Detach(value);
            }
        }

        public event DataStoreAvailableDelegate StorageWriteAvailable
        {
            add
            {
                m_StorageWriteAvailableHandler.Attach(value);
            }
            remove
            {
                m_StorageWriteAvailableHandler.Detach(value);
            }
        }

        IXFeatureRepository IXDocument.Features => Features;
        IXSelectionRepository IXDocument.Selections => Selections;
        IXDimensionRepository IXDocument.Dimensions => Dimensions;
        IXPropertyRepository IXDocument.Properties => Properties;

        protected readonly IXLogger m_Logger;

        private readonly StreamReadAvailableEventsHandler m_StreamReadAvailableHandler;
        private readonly StorageReadAvailableEventsHandler m_StorageReadAvailableHandler;
        private readonly StreamWriteAvailableEventsHandler m_StreamWriteAvailableHandler;
        private readonly StorageWriteAvailableEventsHandler m_StorageWriteAvailableHandler;
        private readonly DocumentRebuildEventsHandler m_DocumentRebuildEventHandler;
        private readonly DocumentSavingEventHandler m_DocumentSavingEventHandler;
        
        public IModelDoc2 Model => m_Creator.Element;

        public string Path
        {
            get
            {
                if (IsCommitted)
                {
                    return Model.GetPathName();
                }
                else
                {
                    return m_Creator.CachedProperties.Get<string>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    throw new NotSupportedException("Path can only be changed for the not commited document");
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public string Template
        {
            get
            {
                if (IsCommitted)
                {
                    throw new NotSupportedException("Template cannot be retrieved for the created document");
                }
                else
                {
                    return m_Creator.CachedProperties.Get<string>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    throw new NotSupportedException("Template cannot be changed for the committed document");
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public string Title
        {
            get
            {
                if (IsCommitted)
                {
                    return Model.GetTitle();
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<string>();
                }
            }
            set 
            {
                if (IsCommitted)
                {
                    if (string.IsNullOrEmpty(Path))
                    {
                        Model.SetTitle2(value);
                    }
                    else 
                    {
                        throw new NotSupportedException("Title can only be changed for new document");
                    }
                }
                else 
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }
        
        public DocumentState_e State 
        {
            get
            {
                if (IsCommitted)
                {
                    return GetDocumentState();
                }
                else
                {
                    return m_Creator.CachedProperties.Get<DocumentState_e>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    var curState = GetDocumentState();

                    if (curState == value)
                    {
                        //do nothing
                    }
                    else if (((int)curState - (int)value) == (int)DocumentState_e.Hidden)
                    {
                        Model.Visible = true;
                    }
                    else if ((int)value - ((int)curState) == (int)DocumentState_e.Hidden)
                    {
                        Model.Visible = false;
                    }
                    else
                    {
                        throw new Exception("Only visibility can changed after the document is loaded");
                    }
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        private DocumentState_e GetDocumentState()
        {
            var state = DocumentState_e.Default;

            if (IsRapidMode)
            {
                state |= DocumentState_e.Rapid;
            }

            if (Model.IsOpenedReadOnly())
            {
                state |= DocumentState_e.ReadOnly;
            }

            if (Model.IsOpenedViewOnly())
            {
                state |= DocumentState_e.ViewOnly;
            }

            if (!Model.Visible)
            {
                state |= DocumentState_e.Hidden;
            }

            return state;
        }

        protected abstract bool IsRapidMode { get; }

        public ISwFeatureManager Features { get; }

        public ISwSelectionCollection Selections { get; }

        public ISwDimensionsCollection Dimensions { get; }

        public ISwCustomPropertiesCollection Properties { get; }

        internal SwApplication App { get; }
        
        public bool IsDirty 
        {
            get => Model.GetSaveFlag();
            set
            {
                if (value == true)
                {
                    Model.SetSaveFlag();
                }
                else 
                {
                    throw new NotSupportedException("Dirty flag cannot be removed. Save document to remove dirty flag");
                }
            }
        }
        
        public bool IsCommitted => m_Creator.IsCreated;

        public ITagsManager Tags { get; }

        private readonly ElementCreator<IModelDoc2> m_Creator;
        
        internal SwDocument(IModelDoc2 model, SwApplication app, IXLogger logger) 
            : this(model, app, logger, true)
        {
        }

        internal SwDocument(IModelDoc2 model, SwApplication app, IXLogger logger, bool created)
        {
            App = app;
            
            m_Logger = logger;

            Tags = new TagsManager();

            m_Creator = new ElementCreator<IModelDoc2>(CreateDocument, model, created);

            m_Creator.Creating += OnCreating;

            Features = new SwFeatureManager(this);
            
            Selections = new SwSelectionCollection(this);

            Dimensions = new SwDocumentDimensionsCollection(this);

            Properties = new SwCustomPropertiesCollection(this, "");

            m_StreamReadAvailableHandler = new StreamReadAvailableEventsHandler(this);
            m_StreamWriteAvailableHandler = new StreamWriteAvailableEventsHandler(this);
            m_StorageReadAvailableHandler = new StorageReadAvailableEventsHandler(this);
            m_StorageWriteAvailableHandler = new StorageWriteAvailableEventsHandler(this);
            m_DocumentRebuildEventHandler = new DocumentRebuildEventsHandler(this);
            m_DocumentSavingEventHandler = new DocumentSavingEventHandler(this);

            if (IsCommitted)
            {
                AttachEvents();
            }
        }

        private void OnCreating(IModelDoc2 model)
        {
            var cachedModel = m_Creator.CachedProperties.Get<IModelDoc2>(nameof(Model));

            Debug.Assert(cachedModel == null 
                || new SwPointerEqualityComparer<IModelDoc2>(App.Sw)
                    .Equals(cachedModel, model), "Invalid pointers");
        }

        private SwDocumentDispatcher m_DocsDispatcher;

        internal void SetDispatcher(SwDocumentDispatcher dispatcher) 
        {
            m_DocsDispatcher = dispatcher;
        }

        protected IModelDoc2 CreateDocument(CancellationToken cancellationToken)
        {
            var docType = -1;

            if (DocumentType.HasValue)
            {
                docType = (int)DocumentType.Value;
            }

            var origVisible = true;

            if (docType != -1)
            {
                origVisible = App.Sw.GetDocumentVisible(docType);
            }

            try
            {
                if (docType != -1)
                {
                    var visible = !State.HasFlag(DocumentState_e.Hidden);

                    App.Sw.DocumentVisible(visible, docType);
                }

                if (string.IsNullOrEmpty(Path))
                {
                    return CreateNewDocument();
                }
                else
                {
                    return OpenDocument();
                }
            }
            finally 
            {
                if (docType != -1)
                {
                    App.Sw.DocumentVisible(origVisible, docType);
                }
            }
        }

        internal protected abstract swDocumentTypes_e? DocumentType { get; }
        
        private IModelDoc2 CreateNewDocument() 
        {
            var docTemplate = Template;

            if (string.IsNullOrEmpty(docTemplate))
            {
                var useDefTemplates = App.Sw.GetUserPreferenceToggle((int)swUserPreferenceToggle_e.swAlwaysUseDefaultTemplates);

                try
                {
                    App.Sw.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swAlwaysUseDefaultTemplates, true);

                    docTemplate = App.Sw.GetDocumentTemplate(
                        (int)DocumentType.Value, "", (int)swDwgPaperSizes_e.swDwgPapersUserDefined, 0.1, 0.1);
                }
                finally
                {
                    App.Sw.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swAlwaysUseDefaultTemplates, useDefTemplates);
                }
            }

            if (!string.IsNullOrEmpty(docTemplate))
            {
                var doc = App.Sw.NewDocument(docTemplate, (int)swDwgPaperSizes_e.swDwgPapersUserDefined, 0.1, 0.1) as IModelDoc2;

                if (doc != null)
                {
                    if (!string.IsNullOrEmpty(Title))
                    {
                        doc.SetTitle2(Title);
                    }
                    return doc;
                }
                else 
                {
                    throw new Exception($"Failed to create new document from the template: {docTemplate}");
                }
            }
            else 
            {
                throw new Exception("Failed to find the location of default document template");
            }
        }

        private IModelDoc2 OpenDocument()
        {
            IModelDoc2 model = null;
            int errorCode = -1;

            if (m_NativeFileExts.TryGetValue(System.IO.Path.GetExtension(Path), out swDocumentTypes_e docType))
            {
                swOpenDocOptions_e opts = 0;

                if (State.HasFlag(DocumentState_e.ReadOnly))
                {
                    opts |= swOpenDocOptions_e.swOpenDocOptions_ReadOnly;
                }

                if (State.HasFlag(DocumentState_e.ViewOnly))
                {
                    opts |= swOpenDocOptions_e.swOpenDocOptions_ViewOnly;
                }

                if (State.HasFlag(DocumentState_e.Silent))
                {
                    opts |= swOpenDocOptions_e.swOpenDocOptions_Silent;
                }

                if (State.HasFlag(DocumentState_e.Rapid))
                {
                    if (docType == swDocumentTypes_e.swDocDRAWING)
                    {
                        if (App.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2020))
                        {
                            opts |= swOpenDocOptions_e.swOpenDocOptions_OpenDetailingMode;
                        }
                    }
                    else if (docType == swDocumentTypes_e.swDocASSEMBLY)
                    {
                        opts |= swOpenDocOptions_e.swOpenDocOptions_OverrideDefaultLoadLightweight | swOpenDocOptions_e.swOpenDocOptions_LoadLightweight;
                    }
                    else if (docType == swDocumentTypes_e.swDocPART)
                    {
                        //There is no rapid option for SOLIDWORKS part document
                    }
                }

                int warns = -1;
                model = App.Sw.OpenDoc6(Path, (int)docType, (int)opts, "", ref errorCode, ref warns);
            }
            else
            {
                model = App.Sw.LoadFile4(Path, "", null, ref errorCode);
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

                throw new OpenDocumentFailedException(Path, errorCode, error);
            }

            return model;
        }

        public void Close()
        {
            App.Sw.CloseDoc(Title);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            Selections.Dispose();
            Dimensions.Dispose();
            Properties.Dispose();

            if (disposing)
            {
                m_StreamReadAvailableHandler.Dispose();
                m_StreamWriteAvailableHandler.Dispose();
                m_StorageReadAvailableHandler.Dispose();
                m_StorageWriteAvailableHandler.Dispose();
                DetachEvents();
            }
        }

        private void AttachEvents()
        {
            switch (Model)
            {
                case PartDoc part:
                    part.DestroyNotify2 += OnDestroyNotify;
                    break;

                case AssemblyDoc assm:
                    assm.DestroyNotify2 += OnDestroyNotify;
                    break;

                case DrawingDoc drw:
                    drw.DestroyNotify2 += OnDestroyNotify;
                    break;
            }
        }

        private void DetachEvents()
        {
            switch (Model)
            {
                case PartDoc part:
                    part.DestroyNotify2 -= OnDestroyNotify;
                    break;

                case AssemblyDoc assm:
                    assm.DestroyNotify2 -= OnDestroyNotify;
                    break;

                case DrawingDoc drw:
                    drw.DestroyNotify2 -= OnDestroyNotify;
                    break;
            }
        }

        private int OnDestroyNotify(int destroyType)
        {
            const int S_OK = 0;

            if (destroyType == (int)swDestroyNotifyType_e.swDestroyNotifyDestroy)
            {
                m_Logger.Log($"Destroying '{Model.GetTitle()}' document");

                Closing?.Invoke(this);
                
                Dispose();
            }
            else if (destroyType == (int)swDestroyNotifyType_e.swDestroyNotifyHidden)
            {
                m_Logger.Log($"Hiding '{Model.GetTitle()}' document");
            }
            else
            {
                Debug.Assert(false, "Not supported type of destroy");
            }

            return S_OK;
        }

        public Stream OpenStream(string name, AccessType_e access)
        {
            return new Sw3rdPartyStream(Model, name, access);
        }

        public IStorage OpenStorage(string name, AccessType_e access)
        {
            return new Sw3rdPartyStorage(Model, name, access);
        }

        public void Commit(CancellationToken cancellationToken)
        {
            m_DocsDispatcher.BeginDispatch(this);
            m_Creator.Create(cancellationToken);
            m_DocsDispatcher.EndDispatch(this);
        }
    }

    internal class SwUnknownDocument : SwDocument, IXUnknownDocument
    {
        public SwUnknownDocument(IModelDoc2 model, SwApplication app, IXLogger logger, bool isCreated) 
            : base(model, app, logger, isCreated)
        {
        }

        protected override bool IsRapidMode => throw new NotImplementedException();

        internal protected override swDocumentTypes_e? DocumentType 
        {
            get 
            {
                if (IsCommitted)
                {
                    return (swDocumentTypes_e)Model.GetType();
                }
                else 
                {
                    if (m_NativeFileExts.TryGetValue(
                        System.IO.Path.GetExtension(Path), out swDocumentTypes_e type))
                    {
                        return type;
                    }
                    else 
                    {
                        return null;
                    }
                }
            }
        }

        private IXDocument m_SpecificDoc;

        public IXDocument GetSpecific()
        {
            if (m_SpecificDoc != null)
            {
                return m_SpecificDoc;
            }

            var model = Model;

            if (model == null) 
            {
                throw new Exception("Model is not yet created, cannot get specific document");
            }

            switch (DocumentType)
            {
                case swDocumentTypes_e.swDocPART:
                    m_SpecificDoc = new SwPart(model as IPartDoc, App, m_Logger, true);
                    break;

                case swDocumentTypes_e.swDocASSEMBLY:
                    m_SpecificDoc = new SwAssembly(model as IAssemblyDoc, App, m_Logger, true);
                    break;

                case swDocumentTypes_e.swDocDRAWING:
                    m_SpecificDoc = new SwDrawing(model as IDrawingDoc, App, m_Logger, true);
                    break;

                default:
                    throw new Exception("Invalid document type");
            }

            return m_SpecificDoc;
        }
    }
}