//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Exceptions;
using Xarial.XCad.Documents.Services;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Annotations;
using Xarial.XCad.SolidWorks.Data;
using Xarial.XCad.SolidWorks.Data.EventHandlers;
using Xarial.XCad.SolidWorks.Documents.EventHandlers;
using Xarial.XCad.SolidWorks.Documents.Exceptions;
using Xarial.XCad.SolidWorks.Documents.Services;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit;
using Xarial.XCad.Toolkit.Data;
using Xarial.XCad.Toolkit.Utils;
using Xarial.XCad.UI;

namespace Xarial.XCad.SolidWorks.Documents
{
    /// <summary>
    /// SOLIDWORKS-specific document
    /// </summary>
    public interface ISwDocument : ISwObject, IXDocument, IDisposable
    {
        /// <summary>
        /// Poiner to the underlying model document
        /// </summary>
        IModelDoc2 Model { get; }

        new ISwFeatureManager Features { get; }
        new ISwSelectionCollection Selections { get; }
        new ISwDimensionsCollection Dimensions { get; }
        new ISwCustomPropertiesCollection Properties { get; }
        new ISwVersion Version { get; }
        new TSwObj DeserializeObject<TSwObj>(Stream stream)
            where TSwObj : ISwObject;
        
        /// <summary>
        /// Creates xCAD object from a SOLIDWORKS dispatch object
        /// </summary>
        /// <typeparam name="TObj">Type of xCAD object</typeparam>
        /// <param name="disp">SOLIDWORKS specific COM object instance</param>
        /// <returns>xCAD object</returns>
        TObj CreateObjectFromDispatch<TObj>(object disp)
            where TObj : ISwObject;
    }

    [DebuggerDisplay("{" + nameof(Title) + "}")]
    internal abstract class SwDocument : SwObject, ISwDocument
    {
        private class Interconnect3DDisabler : IDisposable
        {
            private readonly ISldWorks m_App;
            private readonly bool? m_Is3DInterconnectEnabled;

            internal Interconnect3DDisabler(ISldWorks app)
            {
                m_App = app;

                if (m_App.IsVersionNewerOrEqual(SwVersion_e.Sw2020))
                {
                    var enable3DInterconnect = m_App.GetUserPreferenceToggle(
                        (int)swUserPreferenceToggle_e.swMultiCAD_Enable3DInterconnect);

                    if (enable3DInterconnect)
                    {
                        m_Is3DInterconnectEnabled = enable3DInterconnect;

                        m_App.SetUserPreferenceToggle(
                            (int)swUserPreferenceToggle_e.swMultiCAD_Enable3DInterconnect, false);
                    }
                }
            }

            public void Dispose()
            {
                if (m_Is3DInterconnectEnabled.HasValue)
                {
                    m_App.SetUserPreferenceToggle(
                            (int)swUserPreferenceToggle_e.swMultiCAD_Enable3DInterconnect, m_Is3DInterconnectEnabled.Value);
                }
            }
        }

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

        private DocumentEventDelegate m_DestroyedDel;
        private Action<SwDocument> m_HiddenDel;
        private DocumentCloseDelegate m_ClosingDel;

        public event DocumentEventDelegate Destroyed
        {
            add
            {
                m_DestroyedDel += value;
                AttachDestroyEventsIfNeeded();
            }
            remove
            {
                m_DestroyedDel -= value;
                DetachDestroyEventsIfNeeded();
            }
        }

        internal event Action<SwDocument> Hidden
        {
            add
            {
                m_HiddenDel += value;
                AttachDestroyEventsIfNeeded();
            }
            remove
            {
                m_HiddenDel -= value;
                DetachDestroyEventsIfNeeded();
            }
        }

        public event DocumentCloseDelegate Closing
        {
            add
            {
                m_ClosingDel += value;
                AttachDestroyEventsIfNeeded();
            }
            remove
            {
                m_ClosingDel -= value;
                DetachDestroyEventsIfNeeded();
            }
        }

        public event DocumentEventDelegate Rebuilt
        {
            add => m_DocumentRebuildEventHandler.Attach(value);
            remove => m_DocumentRebuildEventHandler.Detach(value);
        }

        public event DocumentSaveDelegate Saving
        {
            add => m_DocumentSavingEventHandler.Attach(value);
            remove => m_DocumentSavingEventHandler.Detach(value);
        }

        public event DocumentSavedDelegate Saved
        {
            add => m_DocumentSavedEventHandler.Attach(value);
            remove => m_DocumentSavedEventHandler.Detach(value);
        }

        public event DataStoreAvailableDelegate StreamReadAvailable
        {
            add => m_StreamReadAvailableHandler.Attach(value);
            remove => m_StreamReadAvailableHandler.Detach(value);
        }

        public event DataStoreAvailableDelegate StorageReadAvailable
        {
            add => m_StorageReadAvailableHandler.Attach(value);
            remove => m_StorageReadAvailableHandler.Detach(value);
        }

        public event DataStoreAvailableDelegate StreamWriteAvailable
        {
            add => m_StreamWriteAvailableHandler.Attach(value);
            remove => m_StreamWriteAvailableHandler.Detach(value);
        }

        public event DataStoreAvailableDelegate StorageWriteAvailable
        {
            add => m_StorageWriteAvailableHandler.Attach(value);
            remove => m_StorageWriteAvailableHandler.Detach(value);
        }

        IXFeatureRepository IXDocument.Features => Features;
        IXSelectionRepository IXDocument.Selections => Selections;
        IXDimensionRepository IDimensionable.Dimensions => Dimensions;
        IXPropertyRepository IXDocument.Properties => Properties;
        IXVersion IXDocument.Version => Version;
        IXModelViewRepository IXDocument.ModelViews => ModelViews;

        TObj IXDocument.DeserializeObject<TObj>(Stream stream)
            => DeserializeBaseObject<TObj>(stream);

        protected readonly IXLogger m_Logger;

        private readonly StreamReadAvailableEventsHandler m_StreamReadAvailableHandler;
        private readonly StorageReadAvailableEventsHandler m_StorageReadAvailableHandler;
        private readonly StreamWriteAvailableEventsHandler m_StreamWriteAvailableHandler;
        private readonly StorageWriteAvailableEventsHandler m_StorageWriteAvailableHandler;
        private readonly DocumentRebuildEventsHandler m_DocumentRebuildEventHandler;
        private readonly DocumentSavingEventHandler m_DocumentSavingEventHandler;
        private readonly DocumentSavedEventHandler m_DocumentSavedEventHandler;

        public IModelDoc2 Model => m_Creator.Element;

        public IXIdentifier Id
        {
            get
            {
                var creationDate = DateTime.Parse(Model.SummaryInfo[(int)swSummInfoField_e.swSumInfoCreateDate2]).ToUniversalTime();
                var id = new DateTimeOffset(creationDate).ToUnixTimeSeconds();
                return new XIdentifier(id);
            }
        }

        public string Path
        {
            get
            {
                if (IsCommitted)
                {
                    try
                    {
                        return Model.GetPathName();
                    }
                    catch 
                    {
                        return m_CachedFilePath;
                    }
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
                    var userTitle = m_Creator.CachedProperties.Get<string>();

                    if (!string.IsNullOrEmpty(userTitle))
                    {
                        return userTitle;
                    }
                    else 
                    {
                        var path = Path;

                        if (!string.IsNullOrEmpty(path))
                        {
                            return System.IO.Path.GetFileName(path);
                        }
                        else 
                        {
                            return "";
                        }
                    }
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

            if (IsLightweightMode)
            {
                state |= DocumentState_e.Lightweight;
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
        protected abstract bool IsLightweightMode { get; }

        private readonly Lazy<SwFeatureManager> m_FeaturesLazy;
        private readonly Lazy<ISwSelectionCollection> m_SelectionsLazy;
        private readonly Lazy<ISwDimensionsCollection> m_DimensionsLazy;
        private readonly Lazy<SwCustomPropertiesCollection> m_PropertiesLazy;
        private readonly Lazy<SwAnnotationCollection> m_AnnotationsLazy;

        public IXDocumentDependencies Dependencies { get; }
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

                    if (!Model.GetSaveFlag()) 
                    {
                        throw new DirtyFlagIsNotSetException();
                    }
                }
                else 
                {
                    throw new NotSupportedException("Dirty flag cannot be removed. Save document to remove dirty flag");
                }
            }
        }
        
        public override bool IsCommitted => m_Creator.IsCreated;

        protected readonly IElementCreator<IModelDoc2> m_Creator;

        private bool m_AreEventsAttached;
        private bool m_AreDestroyEventsAttached;

        internal override SwDocument OwnerDocument => this;

        private bool m_IsDisposed;

        private readonly Lazy<ISwModelViewsCollection> m_ModelViewsLazy;

        /// <summary>
        /// This is a fallback file path in case the COM pointer to this document is broken (e.g. file is closed or SW is closed)
        /// </summary>
        private string m_CachedFilePath;

        internal SwDocument(IModelDoc2 model, SwApplication app, IXLogger logger) 
            : this(model, app, logger, true)
        {
        }

        internal SwDocument(IModelDoc2 model, SwApplication app, IXLogger logger, bool created) : base(model, null, app)
        {
            m_Logger = logger;

            m_Creator = new ElementCreator<IModelDoc2>(CreateDocument, CommitCache, model, created);

            m_FeaturesLazy = new Lazy<SwFeatureManager>(() => new SwDocumentFeatureManager(this, app, new Context(this)));
            m_SelectionsLazy = new Lazy<ISwSelectionCollection>(() => new SwSelectionCollection(this, app));
            m_DimensionsLazy = new Lazy<ISwDimensionsCollection>(() => new SwFeatureManagerDimensionsCollection(m_FeaturesLazy.Value, new Context(this)));
            m_PropertiesLazy = new Lazy<SwCustomPropertiesCollection>(() => new SwFileCustomPropertiesCollection(this, app));

            m_AnnotationsLazy = new Lazy<SwAnnotationCollection>(CreateAnnotations);

            m_ModelViewsLazy = new Lazy<ISwModelViewsCollection>(() => new SwModelViewsCollection(this, app));

            Units = new SwUnits(this);

            Options = new SwDocumentOptions(this);

            Dependencies = new SwDocumentDependencies(this, m_Logger);

            m_StreamReadAvailableHandler = new StreamReadAvailableEventsHandler(this, app);
            m_StreamWriteAvailableHandler = new StreamWriteAvailableEventsHandler(this, app);
            m_StorageReadAvailableHandler = new StorageReadAvailableEventsHandler(this, app);
            m_StorageWriteAvailableHandler = new StorageWriteAvailableEventsHandler(this, app);
            m_DocumentRebuildEventHandler = new DocumentRebuildEventsHandler(this, app);
            m_DocumentSavingEventHandler = new DocumentSavingEventHandler(this, app);
            m_DocumentSavedEventHandler = new DocumentSavedEventHandler(this, app);

            m_AreEventsAttached = false;

            if (IsCommitted)
            {
                m_CachedFilePath = model.GetPathName();
                AttachEvents();
            }

            m_IsDisposed = false;
        }

        public override object Dispatch => Model;

        internal void SetModel(IModelDoc2 model) => m_Creator.Set(model);

        private IModelDoc2 CreateDocument(CancellationToken cancellationToken)
        {
            //if (((SwDocumentCollection)OwnerApplication.Documents).TryFindExistingDocumentByPath(Path, out _))
            //{
            //    throw new DocumentAlreadyOpenedException(Path);
            //}

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

            IModelDoc2 model = null;

            try
            {
                if (docType != -1)
                {
                    var visible = !State.HasFlag(DocumentState_e.Hidden);

                    OwnerApplication.Sw.DocumentVisible(visible, docType);
                }

                if (string.IsNullOrEmpty(Path))
                {
                    model = CreateNewDocument();
                }
                else
                {
                    m_CachedFilePath = Path;
                    model = OpenDocument();
                }

                return model;
            }
            finally 
            {
                if (model != null)
                {
                    this.Bind(model);
                }
                
                if (docType != -1)
                {
                    OwnerApplication.Sw.DocumentVisible(origVisible, docType);
                }
            }
        }

        protected abstract SwAnnotationCollection CreateAnnotations();

        protected virtual void CommitCache(IModelDoc2 model, CancellationToken cancellationToken)
        {
            if (m_FeaturesLazy.IsValueCreated) 
            {
                m_FeaturesLazy.Value.CommitCache(cancellationToken);
            }

            if (m_PropertiesLazy.IsValueCreated) 
            {
                m_PropertiesLazy.Value.CommitCache(cancellationToken);
            }
        }

        internal protected abstract swDocumentTypes_e? DocumentType { get; }

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
                        if (!string.IsNullOrEmpty(Path))
                        {
                            versHistory = OwnerApplication.Sw.VersionHistory(Path) as string[];
                        }
                        else
                        {
                            throw new Exception("Path is not specified");
                        }
                    }
                }

                if (versHistory?.Any() == true)
                {
                    var latestVers = versHistory.Last();

                    var majorRev = int.Parse(latestVers.Substring(0, latestVers.IndexOf('[')));

                    var vers = OwnerApplication.VersionMapper.FromFileRevision(majorRev);

                    return SwApplicationFactory.CreateVersion(vers);
                }
                else
                {
                    throw new NullReferenceException($"Version information is not found");
                }
            }
        }

        public override bool Equals(IXObject other)
        {
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }
            
            if(other == null)
            {
                return false;
            }

            if (other is ISwDocument)
            {
                if (IsCommitted && ((ISwDocument)other).IsCommitted)
                {
                    var model1 = Model;
                    var model2 = ((ISwDocument)other).Model;

                    if (object.ReferenceEquals(model1, model2))
                    {
                        return true;
                    }

                    bool isAlive1;
                    bool isAlive2;

                    string title1 = "";
                    string title2 = "";

                    try
                    {
                        title1 = model1.GetTitle();
                        isAlive1 = true;
                    }
                    catch
                    {
                        isAlive1 = false;
                    }

                    try
                    {
                        title2 = model2.GetTitle();
                        isAlive2 = true;
                    }
                    catch
                    {
                        isAlive2 = false;
                    }

                    if (isAlive1 && isAlive2)
                    {
                        //NOTE: in some cases drawings can have the same title so it might not be safe to only compare by titles
                        if (string.Equals(title1, title2, StringComparison.CurrentCultureIgnoreCase))
                        {
                            return OwnerApplication.Sw.IsSame(model1, model2) == (int)swObjectEquality.swObjectSame;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else if (!isAlive1 && !isAlive2)
                    {
                        if (!string.IsNullOrEmpty(m_CachedFilePath))
                        {
                            return string.Equals(m_CachedFilePath, ((SwDocument)other).m_CachedFilePath, StringComparison.CurrentCultureIgnoreCase);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else 
                    {
                        return false;
                    }
                }
                else if (!IsCommitted && !((ISwDocument)other).IsCommitted)
                {
                    if (!string.IsNullOrEmpty(Path))
                    {
                        return string.Equals(Path, ((ISwDocument)other).Path, StringComparison.CurrentCultureIgnoreCase);
                    }
                    else 
                    {
                        return false;
                    }
                }
                else 
                {
                    return false;
                }
            }
            else 
            {
                return false;
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
        public virtual IXDocumentOptions Options { get; }

        public virtual ISwModelViewsCollection ModelViews => m_ModelViewsLazy.Value;

        public IXAnnotationRepository Annotations => m_AnnotationsLazy.Value;

        private IModelDoc2 CreateNewDocument() 
        {
            var docTemplate = Template;

            GetPaperSize(out var paperSize, out var paperWidth, out var paperHeight);

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
                        (int)DocumentType.Value, "", (int)paperSize, paperWidth, paperHeight);
                }
                finally
                {
                    OwnerApplication.Sw.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swAlwaysUseDefaultTemplates, useDefTemplates);
                }
            }

            if (!string.IsNullOrEmpty(docTemplate))
            {
                var doc = OwnerApplication.Sw.NewDocument(docTemplate, (int)paperSize, paperWidth, paperHeight) as IModelDoc2;

                if (doc != null)
                {
                    if (!string.IsNullOrEmpty(Title))
                    {
                        //TODO: need to communicate exception if title is not set, do not throw it from here as the doc won't be registered
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
                        opts |= swOpenDocOptions_e.swOpenDocOptions_ViewOnly;

                        if (OwnerApplication.IsVersionNewerOrEqual(SwVersion_e.Sw2021, 4, 1))
                        {
                            opts |= swOpenDocOptions_e.swOpenDocOptions_LDR_EditAssembly;
                        }
                    }
                    else if (docType == swDocumentTypes_e.swDocPART)
                    {
                        //There is no rapid option for SOLIDWORKS part document
                    }
                }

                if (State.HasFlag(DocumentState_e.Lightweight))
                {
                    if (docType == swDocumentTypes_e.swDocDRAWING
                        || docType == swDocumentTypes_e.swDocASSEMBLY)
                    {
                        opts |= swOpenDocOptions_e.swOpenDocOptions_OverrideDefaultLoadLightweight | swOpenDocOptions_e.swOpenDocOptions_LoadLightweight;
                    }
                    else if (docType == swDocumentTypes_e.swDocPART)
                    {
                        //There is no rapid option for SOLIDWORKS part document
                    }
                }
                else 
                {
                    if (docType == swDocumentTypes_e.swDocDRAWING || docType == swDocumentTypes_e.swDocASSEMBLY)
                    {
                        opts |= swOpenDocOptions_e.swOpenDocOptions_OverrideDefaultLoadLightweight;
                    }
                    else if (docType == swDocumentTypes_e.swDocPART)
                    {
                        //There is no rapid option for SOLIDWORKS part document
                    }
                }

                if (!IsDocumentTypeCompatible(docType))
                {
                    throw new DocumentPathIncompatibleException(this);
                }

                int warns = -1;
                model = OwnerApplication.Sw.OpenDoc6(Path, (int)docType, (int)opts, "", ref errorCode, ref warns);
            }
            else
            {
                using (new Interconnect3DDisabler(OwnerApplication.Sw))
                {
                    model = OwnerApplication.Sw.LoadFile4(Path, "", null, ref errorCode);
                }

                if (model != null) 
                {
                    if (!IsDocumentTypeCompatible((swDocumentTypes_e)model.GetType()))
                    {
                        throw new DocumentPathIncompatibleException(this);
                    }
                }
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

        protected abstract bool IsDocumentTypeCompatible(swDocumentTypes_e docType);

        protected virtual void GetPaperSize(out swDwgPaperSizes_e size, out double width, out double height) 
        {
            size = swDwgPaperSizes_e.swDwgPapersUserDefined;
            width = -1;
            height = -1;
        }

        //NOTE: closing of document might note necessarily unload if from memory (if this document is used in active assembly or drawing)
        //do not dispose or set m_IsClosed flag in this function
        public void Close()
            => OwnerApplication.Sw.CloseDoc(Model.GetTitle());
        
        public void Dispose()
        {
            if (!m_IsDisposed)
            {
                m_IsDisposed = true;

                if (m_IsClosed != true)
                {
                    if (IsCommitted && IsAlive)
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

                AttachDestroyEventsIfNeeded();

                switch (Model)
                {
                    case PartDoc part:
                        part.FileSavePostNotify += OnFileSavePostNotify;
                        break;

                    case AssemblyDoc assm:
                        assm.FileSavePostNotify += OnFileSavePostNotify;
                        break;

                    case DrawingDoc drw:
                        drw.FileSavePostNotify += OnFileSavePostNotify;
                        break;
                }
            }
            else 
            {
                Debug.Assert(false, "Events already attached");
            }
        }

        private void AttachDestroyEventsIfNeeded()
        {
            if (!m_AreDestroyEventsAttached && (m_DestroyedDel != null || m_HiddenDel != null || m_ClosingDel != null))
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

                m_AreDestroyEventsAttached = true;
            }
        }

        private void DetachDestroyEventsIfNeeded()
        {
            if (m_AreDestroyEventsAttached && (m_DestroyedDel == null && m_HiddenDel == null && m_ClosingDel == null))
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

                m_AreDestroyEventsAttached = false;
            }
        }

        private int OnFileSavePostNotify(int saveType, string fileName)
        {
            if (saveType == (int)swFileSaveTypes_e.swFileSaveAs)
            {
                m_CachedFilePath = fileName;
            }
            
            return HResult.S_OK;
        }

        private void DetachEvents()
        {
            if (IsCommitted)
            {
                switch (Model)
                {
                    case PartDoc part:
                        part.DestroyNotify2 -= OnDestroyNotify;
                        part.FileSavePostNotify -= OnFileSavePostNotify;
                        break;

                    case AssemblyDoc assm:
                        assm.DestroyNotify2 -= OnDestroyNotify;
                        assm.FileSavePostNotify -= OnFileSavePostNotify;
                        break;

                    case DrawingDoc drw:
                        drw.DestroyNotify2 -= OnDestroyNotify;
                        drw.FileSavePostNotify -= OnFileSavePostNotify;
                        break;
                }
            }
        }

        private int OnDestroyNotify(int destroyType)
        {
            try
            {
                if (destroyType == (int)swDestroyNotifyType_e.swDestroyNotifyDestroy)
                {
                    m_Logger.Log($"Destroying '{Model.GetTitle()}' document", XCad.Base.Enums.LoggerMessageSeverity_e.Debug);

                    try
                    {
                        m_ClosingDel?.Invoke(this, DocumentCloseType_e.Destroy);
                    }
                    catch (Exception ex)
                    {
                        m_Logger.Log(ex);
                    }

                    m_DestroyedDel?.Invoke(this);

                    m_IsClosed = true;

                    Dispose();
                }
                else if (destroyType == (int)swDestroyNotifyType_e.swDestroyNotifyHidden)
                {
                    try
                    {
                        m_ClosingDel?.Invoke(this, DocumentCloseType_e.Hide);
                    }
                    catch (Exception ex)
                    {
                        m_Logger.Log(ex);
                    }

                    m_HiddenDel?.Invoke(this);

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
            
            return HResult.S_OK;
        }

        public Stream OpenStream(string name, bool write)
        {
            var stream = new Sw3rdPartyStream(Model, name, write);

            if (stream.Exists)
            {
                return stream;
            }
            else 
            {
                return Stream.Null;
            }
        }

        public IStorage OpenStorage(string name, bool write)
        {
            var storage = new Sw3rdPartyStorage(Model, name, write);

            if (storage.Exists)
            {
                return storage;
            }
            else 
            {
                return Storage.Null;
            }
        }

        public override void Commit(CancellationToken cancellationToken)
            => m_Creator.Create(cancellationToken);

        public void Save()
        {
            if (!string.IsNullOrEmpty(Path))
            {
                int errs = -1;
                int warns = -1;

                if (!Model.Save3((int)swSaveAsOptions_e.swSaveAsOptions_Silent, ref errs, ref warns))
                {
                    throw new SaveDocumentFailedException(errs, SwSaveOperation.ParseSaveError((swFileSaveError_e)errs));
                }
            }
            else 
            {
                throw new SaveNeverSavedDocumentException();
            }
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
                //do not throw exception - in some cases rebuild is happening, but false is returned
                //throw new Exception("Failed to rebuild the model");
            }
        }

        public IOperationGroup PreCreateOperationGroup() => new SwUndoObjectGroup(this);

        public abstract IXSaveOperation PreCreateSaveAsOperation(string filePath);
    }

    internal class SwUnknownDocument : SwDocument, IXUnknownDocument
    {
        public SwUnknownDocument(IModelDoc2 model, SwApplication app, IXLogger logger, bool isCreated) 
            : base(model, app, logger, isCreated)
        {
        }

        protected override bool IsLightweightMode => throw new NotSupportedException();
        protected override bool IsRapidMode => throw new NotSupportedException();
        protected override SwAnnotationCollection CreateAnnotations() => throw new NotSupportedException();

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
                m_Creator.Init(curDoc.Model, CancellationToken.None);
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

        protected override bool IsDocumentTypeCompatible(swDocumentTypes_e docType) => true;

        public override IXSaveOperation PreCreateSaveAsOperation(string filePath) => throw new NotSupportedException();
    }

    internal class SwUnknownDocument3D : SwUnknownDocument, ISwDocument3D
    {
        public SwUnknownDocument3D(IModelDoc2 model, SwApplication app, IXLogger logger, bool isCreated) 
            : base(model, app, logger, isCreated)
        {
        }

        public IXConfigurationRepository Configurations => throw new NotImplementedException();
        public IXDocumentEvaluation Evaluation => throw new NotImplementedException();
        public IXDocumentGraphics Graphics => throw new NotImplementedException();
        ISwConfigurationCollection ISwDocument3D.Configurations => throw new NotImplementedException();
        IXConfigurationRepository IXDocument3D.Configurations => throw new NotImplementedException();
        ISwModelViews3DCollection ISwDocument3D.ModelViews => throw new NotImplementedException();
        IXModelView3DRepository IXDocument3D.ModelViews => throw new NotImplementedException();
        TSelObject IXObjectContainer.ConvertObject<TSelObject>(TSelObject obj) => throw new NotImplementedException();
        TSelObject ISwDocument3D.ConvertObject<TSelObject>(TSelObject obj) => throw new NotImplementedException();
        IXDocument3DSaveOperation IXDocument3D.PreCreateSaveAsOperation(string filePath) => throw new NotImplementedException();
    }

    internal static class SwDocumentExtension 
    {
        internal static Image GetThumbnailImage(this SwDocument doc) 
        {
            using (var thumbnail = new ShellThumbnail(doc.Path)) 
            {
                return Image.FromHbitmap(thumbnail.BitmapHandle);
            }
        }

        internal static void SetUserPreferenceToggle(this SwDocument doc, swUserPreferenceToggle_e option, bool value) 
            => doc.Model.Extension.SetUserPreferenceToggle((int)option, (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, value);

        internal static bool GetUserPreferenceToggle(this SwDocument doc, swUserPreferenceToggle_e option)
            => doc.Model.Extension.GetUserPreferenceToggle((int)option, (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified);

        internal static void Bind(this SwDocument doc, IModelDoc2 model)
        {
            if (!doc.IsCommitted)
            {
                doc.SetModel(model);
            }

            if (doc.IsCommitted)
            {
                if (!(doc is SwUnknownDocument))
                {
                    doc.AttachEvents();
                }
            }
        }
    }
}