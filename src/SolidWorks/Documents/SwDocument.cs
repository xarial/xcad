//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
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
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Data;
using Xarial.XCad.Data.Enums;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Exceptions;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Features;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Annotations;
using Xarial.XCad.SolidWorks.Data;
using Xarial.XCad.SolidWorks.Data.EventHandlers;
using Xarial.XCad.SolidWorks.Documents.EventHandlers;
using Xarial.XCad.SolidWorks.Documents.Exceptions;
using Xarial.XCad.SolidWorks.Documents.Services;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Data;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwDocument : ISwObject, IXDocument, IDisposable
    {
        IModelDoc2 Model { get; }
        new ISwFeatureManager Features { get; }
        new ISwSelectionCollection Selections { get; }
        new ISwDimensionsCollection Dimensions { get; }
        new ISwCustomPropertiesCollection Properties { get; }
        new ISwVersion Version { get; }
        new ISwDocument3D[] Dependencies { get; }
        new TSwObj DeserializeObject<TSwObj>(Stream stream)
            where TSwObj : ISwObject;

        TObj CreateObjectFromDispatch<TObj>(object disp)
            where TObj : ISwObject;
    }
    
    [DebuggerDisplay("{" + nameof(Title) + "}")]
    internal abstract class SwDocument : SwObject, ISwDocument
    {
        protected static Dictionary<string, swDocumentTypes_e> m_NativeFileExts { get; }
        private bool? m_IsClosed;

        static SwDocument() 
        {
            m_NativeFileExts = new Dictionary<string, swDocumentTypes_e>(StringComparer.CurrentCultureIgnoreCase)
            {
                { ".sldprt", swDocumentTypes_e.swDocPART },
                { ".sldasm", swDocumentTypes_e.swDocASSEMBLY },
                { ".slddrw", swDocumentTypes_e.swDocDRAWING },
                { ".sldlfp", swDocumentTypes_e.swDocPART },
                { ".sldblk", swDocumentTypes_e.swDocPART },
                { ".prtdot", swDocumentTypes_e.swDocPART },
                { ".asmdot", swDocumentTypes_e.swDocASSEMBLY },
                { ".drwdot", swDocumentTypes_e.swDocDRAWING }
            };
        }

        internal event Action<SwDocument> Hidden;
        internal event Action<SwDocument> Destroyed;

        public event DocumentCloseDelegate Closing;
        
        public event DocumentRebuildDelegate Rebuilt 
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
        IXPropertyRepository IPropertiesOwner.Properties => Properties;
        IXDocument3D[] IXDocument.Dependencies => Dependencies;
        IXVersion IXDocument.Version => Version;

        TObj IXDocument.DeserializeObject<TObj>(Stream stream)
            => DeserializeBaseObject<TObj>(stream);

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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        internal void SetClosed()
        {
            m_IsClosed = true;
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

        private readonly Lazy<ISwFeatureManager> m_FeaturesLazy;
        private readonly Lazy<ISwSelectionCollection> m_SelectionsLazy;
        private readonly Lazy<ISwDimensionsCollection> m_DimensionsLazy;
        private readonly Lazy<ISwCustomPropertiesCollection> m_PropertiesLazy;

        public ISwFeatureManager Features => m_FeaturesLazy.Value;
        public ISwSelectionCollection Selections => m_SelectionsLazy.Value;
        public ISwDimensionsCollection Dimensions => m_DimensionsLazy.Value;
        public ISwCustomPropertiesCollection Properties => m_PropertiesLazy.Value;
                
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

        protected readonly ElementCreator<IModelDoc2> m_Creator;

        private bool m_AreEventsAttached;

        internal override ISwDocument OwnerDocument => this;

        private bool m_IsDisposed;

        internal SwDocument(IModelDoc2 model, ISwApplication app, IXLogger logger) 
            : this(model, app, logger, true)
        {
        }

        internal SwDocument(IModelDoc2 model, ISwApplication app, IXLogger logger, bool created) : base(model, null, app)
        {
            m_Logger = logger;

            m_Creator = new ElementCreator<IModelDoc2>(CreateDocument, model, created);

            m_Creator.Creating += OnCreating;

            m_FeaturesLazy = new Lazy<ISwFeatureManager>(() => new SwFeatureManager(this, app));
            m_SelectionsLazy = new Lazy<ISwSelectionCollection>(() => new SwSelectionCollection(this, app));
            m_DimensionsLazy = new Lazy<ISwDimensionsCollection>(() => new SwFeatureManagerDimensionsCollection(this.Features));
            m_PropertiesLazy = new Lazy<ISwCustomPropertiesCollection>(() => new SwFileCustomPropertiesCollection(this, app));

            Units = new SwUnits(this);

            m_StreamReadAvailableHandler = new StreamReadAvailableEventsHandler(this, app);
            m_StreamWriteAvailableHandler = new StreamWriteAvailableEventsHandler(this, app);
            m_StorageReadAvailableHandler = new StorageReadAvailableEventsHandler(this, app);
            m_StorageWriteAvailableHandler = new StorageWriteAvailableEventsHandler(this, app);
            m_DocumentRebuildEventHandler = new DocumentRebuildEventsHandler(this, app);
            m_DocumentSavingEventHandler = new DocumentSavingEventHandler(this, app);

            m_AreEventsAttached = false;

            if (IsCommitted)
            {
                AttachEvents();
            }

            m_IsDisposed = false;
        }

        public override object Dispatch => Model;

        private void OnCreating(IModelDoc2 model)
        {
            var cachedModel = m_Creator.CachedProperties.Get<IModelDoc2>(nameof(Model));

            Debug.Assert(cachedModel == null 
                || SwModelPointerEqualityComparer.AreEqual(cachedModel, model), "Invalid pointers");
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
                origVisible = OwnerApplication.Sw.GetDocumentVisible(docType);
            }

            try
            {
                if (docType != -1)
                {
                    var visible = !State.HasFlag(DocumentState_e.Hidden);

                    OwnerApplication.Sw.DocumentVisible(visible, docType);
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
                    OwnerApplication.Sw.DocumentVisible(origVisible, docType);
                }
            }
        }

        internal protected abstract swDocumentTypes_e? DocumentType { get; }

        public ISwDocument3D[] Dependencies 
        {
            get 
            {
                if (!string.IsNullOrEmpty(Path))
                {
                    string[] depsData;

                    if (IsCommitted && !Model.IsOpenedViewOnly())
                    {
                        depsData = Model.Extension.GetDependencies(false, true, false, true, true) as string[];
                    }
                    else 
                    {
                        depsData = OwnerApplication.Sw.GetDocumentDependencies2(Path, false, true, false) as string[];
                    }

                    if (depsData?.Any() == true)
                    {
                        var deps = new ISwDocument3D[depsData.Length / 2];

                        for (int i = 1; i < depsData.Length; i += 2) 
                        {
                            var path = depsData[i];

                            if (!((SwDocumentCollection)OwnerApplication.Documents).TryFindExistingDocumentByPath(path, out SwDocument refDoc))
                            {
                                refDoc = (SwDocument3D)((SwDocumentCollection)OwnerApplication.Documents).PreCreateFromPath(path);
                            }

                            deps[(i - 1) / 2] = (ISwDocument3D)refDoc;
                        }

                        return deps;
                    }
                    else 
                    {
                        return new ISwDocument3D[0];
                    }
                }
                else 
                {
                    throw new Exception("Dependencies can only be extracted for the document with specified path");
                }
            }
        }

        public ISwVersion Version 
        {
            get 
            {
                string[] versHistory;

                if (!string.IsNullOrEmpty(Path))
                {
                    versHistory = OwnerApplication.Sw.VersionHistory(Path) as string[];
                }
                else
                {
                    if (IsCommitted)
                    {
                        versHistory = Model.VersionHistory() as string[];
                    }
                    else
                    {
                        throw new Exception("Path is not specified");
                    }
                }

                var vers = GetVersion(versHistory);

                return SwApplicationFactory.CreateVersion(vers);
            }
        }

        public override bool Equals(IXObject other)
        {
            if (!object.ReferenceEquals(this, other) 
                && other is ISwDocument 
                && !IsCommitted && !((ISwDocument)other).IsCommitted)
            {
                return !string.IsNullOrEmpty(Path) && !string.IsNullOrEmpty(((ISwDocument)other).Path)
                    && string.Equals(Path, ((ISwDocument)other).Path, StringComparison.CurrentCultureIgnoreCase);
            }
            else
            {
                return base.Equals(other);
            }
        }

        public override bool IsAlive 
        {
            get 
            {
                var model = Model;

                try
                {
                    var title = model.GetTitle();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public int UpdateStamp => Model.GetUpdateStamp();

        public IXUnits Units { get; }

        private SwVersion_e GetVersion(string[] versHistory)
        {
            if (versHistory?.Any() == true)
            {
                var latestVers = versHistory.Last();

                var majorRev = int.Parse(latestVers.Substring(0, latestVers.IndexOf('[')));

                switch (majorRev)
                {
                    case 44:
                    case 243:
                    case 483:
                    case 629:
                    case 822:
                    case 1008:
                    case 1137:
                        return SwVersion_e.SwPrior2000;
                    case 1500:
                        return SwVersion_e.Sw2000;
                    case 1750:
                        return SwVersion_e.Sw2001;
                    case 1950:
                        return SwVersion_e.Sw2001Plus;
                    case 2200:
                        return SwVersion_e.Sw2003;
                    case 2500:
                        return SwVersion_e.Sw2004;
                    case 2800:
                        return SwVersion_e.Sw2005;
                    case 3100:
                        return SwVersion_e.Sw2006;
                    case 3400:
                        return SwVersion_e.Sw2007;
                    case 3800:
                        return SwVersion_e.Sw2008;
                    case 4100:
                        return SwVersion_e.Sw2009;
                    case 4400:
                        return SwVersion_e.Sw2010;
                    case 4700:
                        return SwVersion_e.Sw2011;
                    case 5000:
                        return SwVersion_e.Sw2012;
                    case 6000:
                        return SwVersion_e.Sw2013;
                    case 7000:
                        return SwVersion_e.Sw2014;
                    case 8000:
                        return SwVersion_e.Sw2015;
                    case 9000:
                        return SwVersion_e.Sw2016;
                    case 10000:
                        return SwVersion_e.Sw2017;
                    case 11000:
                        return SwVersion_e.Sw2018;
                    case 12000:
                        return SwVersion_e.Sw2019;
                    case 13000:
                        return SwVersion_e.Sw2020;
                    case 14000:
                        return SwVersion_e.Sw2021;
                    case 15000:
                        return SwVersion_e.Sw2022;
                    default:
                        throw new NotSupportedException($"'{latestVers}' version is not recognized");
                }
            }
            else
            {
                throw new NullReferenceException($"Version information is not found");
            }
        }

        private IModelDoc2 CreateNewDocument() 
        {
            var docTemplate = Template;

            if (string.IsNullOrEmpty(docTemplate))
            {
                if (!DocumentType.HasValue) 
                {
                    throw new Exception("Cannot find the default template for unknown document type");
                }

                var useDefTemplates = OwnerApplication.Sw.GetUserPreferenceToggle((int)swUserPreferenceToggle_e.swAlwaysUseDefaultTemplates);

                try
                {
                    OwnerApplication.Sw.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swAlwaysUseDefaultTemplates, true);

                    docTemplate = OwnerApplication.Sw.GetDocumentTemplate(
                        (int)DocumentType.Value, "", (int)swDwgPaperSizes_e.swDwgPapersUserDefined, 0.1, 0.1);
                }
                finally
                {
                    OwnerApplication.Sw.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swAlwaysUseDefaultTemplates, useDefTemplates);
                }
            }

            if (!string.IsNullOrEmpty(docTemplate))
            {
                var doc = OwnerApplication.Sw.NewDocument(docTemplate, (int)swDwgPaperSizes_e.swDwgPapersUserDefined, 0.1, 0.1) as IModelDoc2;

                if (doc != null)
                {
                    if (!string.IsNullOrEmpty(Title))
                    {
                        //TODO: need to communicate exception if title is not set, do not throw it from heer as the doc won't be registered
                        doc.SetTitle2(Title);
                    }

                    return doc;
                }
                else 
                {
                    throw new NewDocumentCreateException(docTemplate);
                }
            }
            else 
            {
                throw new DefaultTemplateNotFoundException();
            }
        }
        
        private IModelDoc2 OpenDocument()
        {
            IModelDoc2 model;
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
                        if (OwnerApplication.IsVersionNewerOrEqual(SwVersion_e.Sw2020))
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
                model = OwnerApplication.Sw.OpenDoc6(Path, (int)docType, (int)opts, "", ref errorCode, ref warns);
            }
            else
            {
                model = OwnerApplication.Sw.LoadFile4(Path, "", null, ref errorCode);
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
                        error = "File has non-critical data corruption and requires repair";
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
            OwnerApplication.Sw.CloseDoc(Model.GetTitle());
            m_IsClosed = true;
        }

        public void Dispose()
        {
            if (!m_IsDisposed)
            {
                m_IsDisposed = true;

                if (m_IsClosed != true)
                {
                    if (IsAlive)
                    {
                        Close();
                    }
                }

                Dispose(true);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (m_SelectionsLazy.IsValueCreated)
            {
                m_SelectionsLazy.Value.Dispose();
            }

            if (m_DimensionsLazy.IsValueCreated)
            {
                m_DimensionsLazy.Value.Dispose();
            }

            if (m_PropertiesLazy.IsValueCreated)
            {
                m_PropertiesLazy.Value.Dispose();
            }

            if (disposing)
            {
                m_StreamReadAvailableHandler.Dispose();
                m_StreamWriteAvailableHandler.Dispose();
                m_StorageReadAvailableHandler.Dispose();
                m_StorageWriteAvailableHandler.Dispose();

                DetachEvents();
            }
        }

        internal void AttachEvents()
        {
            if (!m_AreEventsAttached)
            {
                m_AreEventsAttached = true;

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
            else 
            {
                Debug.Assert(false, "Events already attached");
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

            try
            {
                if (destroyType == (int)swDestroyNotifyType_e.swDestroyNotifyDestroy)
                {
                    m_Logger.Log($"Destroying '{Model.GetTitle()}' document", XCad.Base.Enums.LoggerMessageSeverity_e.Debug);

                    try
                    {
                        Closing?.Invoke(this);
                    }
                    catch (Exception ex)
                    {
                        m_Logger.Log(ex);
                    }

                    Destroyed?.Invoke(this);

                    Dispose();
                }
                else if (destroyType == (int)swDestroyNotifyType_e.swDestroyNotifyHidden)
                {
                    Hidden?.Invoke(this);
                    m_Logger.Log($"Hiding '{Model.GetTitle()}' document", XCad.Base.Enums.LoggerMessageSeverity_e.Debug);
                }
                else
                {
                    Debug.Assert(false, "Not supported type of destroy");
                }
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
            }

            return S_OK;
        }

        public Stream OpenStream(string name, AccessType_e access)
            => new Sw3rdPartyStream(Model, name, access);

        public IStorage OpenStorage(string name, AccessType_e access)
            => new Sw3rdPartyStorage(Model, name, access);

        public virtual void Commit(CancellationToken cancellationToken)
        {
            if (((SwDocumentCollection)OwnerApplication.Documents).TryFindExistingDocumentByPath(Path, out _)) 
            {
                throw new DocumentAlreadyOpenedException(Path);
            }

            m_DocsDispatcher.BeginDispatch(this);
            
            try
            {
                m_Creator.Create(cancellationToken);
            }
            finally 
            {
                m_DocsDispatcher.EndDispatch(this);
            }
        }

        public void Save()
        {
            if (!string.IsNullOrEmpty(Path))
            {
                int errs = -1;
                int warns = -1;

                if (!Model.Save3((int)swSaveAsOptions_e.swSaveAsOptions_Silent, ref errs, ref warns))
                {
                    throw new SaveDocumentFailedException(errs, ParseSaveError((swFileSaveError_e)errs));
                }
            }
            else 
            {
                throw new SaveNeverSavedDocumentException();
            }
        }

        public void SaveAs(string filePath)
        {
            int errs = -1;
            int warns = -1;

            bool res;

            if (OwnerApplication.IsVersionNewerOrEqual(SwVersion_e.Sw2019, 1))
            {
                res = Model.Extension.SaveAs2(filePath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion,
                    (int)swSaveAsOptions_e.swSaveAsOptions_Silent, null, "", false, ref errs, ref warns);
            }
            else 
            {
                res = Model.Extension.SaveAs(filePath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion,
                    (int)swSaveAsOptions_e.swSaveAsOptions_Silent, null, ref errs, ref warns);
            }

            if (!res)
            {
                throw new SaveDocumentFailedException(errs, ParseSaveError((swFileSaveError_e)errs));
            }
        }

        private static string ParseSaveError(swFileSaveError_e err)
        {
            var errors = new List<string>();

            if (err.HasFlag(swFileSaveError_e.swFileLockError))
            {
                errors.Add("File lock error");
            }

            if (err.HasFlag(swFileSaveError_e.swFileNameContainsAtSign))
            {
                errors.Add("File name cannot contain the at symbol(@)");
            }

            if (err.HasFlag(swFileSaveError_e.swFileNameEmpty))
            {
                errors.Add("File name cannot be empty");
            }

            if (err.HasFlag(swFileSaveError_e.swFileSaveAsBadEDrawingsVersion))
            {
                errors.Add("Bad eDrawings data");
            }

            if (err.HasFlag(swFileSaveError_e.swFileSaveAsDoNotOverwrite))
            {
                errors.Add("Cannot overwrite an existing file");
            }

            if (err.HasFlag(swFileSaveError_e.swFileSaveAsInvalidFileExtension))
            {
                errors.Add("File name extension does not match the SOLIDWORKS document type");
            }

            if (err.HasFlag(swFileSaveError_e.swFileSaveAsNameExceedsMaxPathLength))
            {
                errors.Add("File name cannot exceed 255 characters");
            }

            if (err.HasFlag(swFileSaveError_e.swFileSaveAsNotSupported))
            {
                errors.Add("Save As operation is not supported in this environment");
            }

            if (err.HasFlag(swFileSaveError_e.swFileSaveFormatNotAvailable))
            {
                errors.Add("Save As file type is not valid");
            }

            if (err.HasFlag(swFileSaveError_e.swFileSaveRequiresSavingReferences))
            {
                errors.Add("Saving an assembly with renamed components requires saving the references");
            }

            if (err.HasFlag(swFileSaveError_e.swGenericSaveError))
            {
                errors.Add("Generic error");
            }

            if (err.HasFlag(swFileSaveError_e.swReadOnlySaveError))
            {
                errors.Add("File is readonly");
            }

            if (errors.Count == 0)
            {
                errors.Add("Unknown error");
            }

            return string.Join("; ", errors);
        }

        public TSwObj DeserializeObject<TSwObj>(Stream stream)
            where TSwObj : ISwObject
            => DeserializeBaseObject<TSwObj>(stream);

        private TObj DeserializeBaseObject<TObj>(Stream stream)
            where TObj : IXObject
        {
            stream.Seek(0, SeekOrigin.Begin);

            byte[] buffer;

            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                buffer = memoryStream.ToArray();
            }

            var obj = Model.Extension.GetObjectByPersistReference3(buffer, out int err);

            if (obj != null)
            {
                return (TObj)CreateObjectFromDispatch<ISwObject>(obj);
            }
            else
            {
                string reason;

                switch ((swPersistReferencedObjectStates_e)err)
                {
                    case swPersistReferencedObjectStates_e.swPersistReferencedObject_Deleted:
                        reason = "Object is deleted";
                        break;

                    case swPersistReferencedObjectStates_e.swPersistReferencedObject_Invalid:
                        reason = "Object is invalid";
                        break;

                    case swPersistReferencedObjectStates_e.swPersistReferencedObject_Suppressed:
                        reason = "Object is suppressed";
                        break;

                    default:
                        reason = "Unknown reason";
                        break;
                }

                throw new ObjectSerializationException($"Failed to serialize object: {reason}", err);
            }
        }

        public TObj CreateObjectFromDispatch<TObj>(object disp) where TObj : ISwObject
            => SwObjectFactory.FromDispatch<TObj>(disp, this, OwnerApplication);

        public void Rebuild() 
        {
            if (Model.ForceRebuild3(false)) 
            {
                throw new Exception("Failed to rebuild the model");
            }
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
                    if (!string.IsNullOrEmpty(Path))
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
                    else if (!string.IsNullOrEmpty(Template))
                    {
                        if (m_NativeFileExts.TryGetValue(
                            System.IO.Path.GetExtension(Template), out swDocumentTypes_e type))
                        {
                            return type;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else 
                    {
                        return null;
                    }
                }
            }
        }

        public override void Commit(CancellationToken cancellationToken)
        {
            if (((SwDocumentCollection)OwnerApplication.Documents).TryFindExistingDocumentByPath(Path, out SwDocument curDoc))
            {
                m_SpecificDoc = curDoc;
                m_Creator.Reset(curDoc.Model, true);
            }
            else
            {
                base.Commit(cancellationToken);
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
                    m_SpecificDoc = new SwPart(model as IPartDoc, OwnerApplication, m_Logger, true);
                    break;

                case swDocumentTypes_e.swDocASSEMBLY:
                    m_SpecificDoc = new SwAssembly(model as IAssemblyDoc, OwnerApplication, m_Logger, true);
                    break;

                case swDocumentTypes_e.swDocDRAWING:
                    m_SpecificDoc = new SwDrawing(model as IDrawingDoc, OwnerApplication, m_Logger, true);
                    break;

                default:
                    throw new Exception("Invalid document type");
            }

            return m_SpecificDoc;
        }
    }
}