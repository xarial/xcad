//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Data;
using Xarial.XCad.SwDocumentManager.Data;
using Xarial.XCad.Toolkit.Data;

namespace Xarial.XCad.SwDocumentManager.Documents
{
    public interface ISwDmDocument : ISwDmObject, IXDocument
    {
        ISwDMDocument Document { get; }
        new ISwDmVersion Version { get; }
        new ISwDmCustomPropertiesCollection Properties { get; }

        TObj CreateObjectFromDispatch<TObj>(object disp)
            where TObj : ISwDmObject;
    }

    [DebuggerDisplay("{" + nameof(Title) + "}")]
    internal abstract class SwDmDocument : SwDmObject, ISwDmDocument
    {
        internal static SwDmDocumentType GetDocumentType(string path)
        {
            SwDmDocumentType docType;

            if (!string.IsNullOrEmpty(path))
            {
                switch (System.IO.Path.GetExtension(path).ToLower())
                {
                    case ".sldprt":
                    case ".sldblk":
                    case ".prtdot":
                    case ".sldlfp":
                        docType = SwDmDocumentType.swDmDocumentPart;
                        break;

                    case ".sldasm":
                    case ".asmdot":
                        docType = SwDmDocumentType.swDmDocumentAssembly;
                        break;

                    case ".slddrw":
                    case ".drwdot":
                        docType = SwDmDocumentType.swDmDocumentDrawing;
                        break;

                    default:
                        throw new NotSupportedException("Only native SOLIDWORKS files can be opened");
                }
            }
            else
            {
                throw new NotSupportedException("Cannot extract document type when path is not specified");
            }

            return docType;
        }

        #region Not Supported

        public string Template { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public IXFeatureRepository Features => throw new NotImplementedException();
        public IXSelectionRepository Selections => throw new NotSupportedException();
        public IXDimensionRepository Dimensions => throw new NotSupportedException();
        public event DocumentEventDelegate Rebuilt
        {
            add => throw new NotSupportedException();
            remove => throw new NotSupportedException();
        }
        public TObj DeserializeObject<TObj>(Stream stream) where TObj : IXObject => throw new NotSupportedException();
        public void Rebuild() => throw new NotSupportedException();
        public IXUnits Units => throw new NotSupportedException();

        #endregion

        internal event Action<SwDmDocument> Disposed;

        IXVersion IXDocument.Version => Version;
        IXPropertyRepository IPropertiesOwner.Properties => Properties;

        public ISwDMDocument Document => m_Creator.Element;

        public ISwDmVersion Version => SwDmApplicationFactory.CreateVersion((SwDmVersion_e)Document.GetVersion());

        public virtual string Title 
        {
            get 
            {
                if (IsFileExtensionShown)
                {
                    return System.IO.Path.GetFileName(Path);
                }
                else
                {
                    return System.IO.Path.GetFileNameWithoutExtension(Path);
                }
            }
            set => throw new NotSupportedException("This property is read-only");
        }

        public string Path 
        {
            get 
            {
                if (IsCommitted)
                {
                    return Document.FullName;
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
                    throw new NotSupportedException("Path cannot be changed for an opened document");
                }
                else 
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public virtual bool IsDirty { get; set; }

        private bool? m_IsReadOnly;

        public DocumentState_e State 
        {
            get 
            {
                if (IsCommitted)
                {
                    if (m_IsReadOnly.Value)
                    {
                        return DocumentState_e.ReadOnly;
                    }
                    else 
                    {
                        return DocumentState_e.Default;
                    }
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
                    throw new Exception("This property is read-only");
                }
                else 
                {
                    if (value != DocumentState_e.Default && value != DocumentState_e.ReadOnly) 
                    {
                        throw new NotSupportedException("Only default and read-only states are supported");
                    }

                    m_Creator.CachedProperties.Set(value);
                }
            }
        }
        
        private bool? m_IsClosed;

        public override bool IsAlive 
        {
            get 
            {
                if (m_IsClosed.HasValue)
                {
                    return !m_IsClosed.Value;
                }
                else
                {
                    try
                    {
                        //This not causing exception - so does not work - keeping as placeholder for future
                        var testVers = Document.GetVersion();

                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        }

        public IEnumerable<IXDocument3D> Dependencies 
        {
            get
            {
                var deps = GetRawDependencies();

                if (deps.Any())
                {
                    var compsLazy = new Lazy<ISwDmComponent[]>(
                        () =>
                        {
                            if (string.Equals(System.IO.Path.GetExtension(Path), ".sldasm", StringComparison.CurrentCultureIgnoreCase))
                            {
                                var activeConfName = Document.ConfigurationManager.GetActiveConfigurationName();
                                var conf = (ISwDMConfiguration2)Document.ConfigurationManager.GetConfigurationByName(activeConfName);
                                var comps = (object[])conf.GetComponents();
                                if (comps != null)
                                {
                                    return comps.Select(c => CreateObjectFromDispatch<ISwDmComponent>(c)).ToArray();
                                }
                                else
                                {
                                    return new ISwDmComponent[0];
                                }
                            }
                            else
                            {
                                throw new Exception("Components can only be extracted from the assembly");
                            }
                        });

                    bool TryFindVirtualDocument(string filePath, out ISwDmDocument3D virtCompDoc)
                    {
                        try
                        {
                            var virtCompFileName = System.IO.Path.GetFileName(filePath);

                            virtCompDoc = m_VirtualDocumentsCache.FirstOrDefault(d => string.Equals(d.Title,
                                virtCompFileName, StringComparison.CurrentCultureIgnoreCase));

                            if (virtCompDoc != null) 
                            {
                                if (virtCompDoc.IsAlive)
                                {
                                    return true;
                                }
                                else 
                                {
                                    m_VirtualDocumentsCache.Remove(virtCompDoc);
                                    virtCompDoc = null;
                                }
                            }

                            var comp = compsLazy.Value.FirstOrDefault(c => string.Equals(
                                System.IO.Path.GetFileName(c.CachedPath), virtCompFileName,
                                StringComparison.CurrentCultureIgnoreCase));

                            if (comp != null)
                            {
                                virtCompDoc = comp.ReferencedDocument;
                                m_VirtualDocumentsCache.Add(virtCompDoc);
                                return true;
                            }
                        }
                        catch
                        {
                        }

                        virtCompDoc = null;
                        return false;
                    }

                    for (int i = 0; i < deps.Length; i++)
                    {
                        var depPath = deps[i].Item1;
                        var isVirtual = deps[i].Item2;

                        ISwDmDocument3D depDoc = null;

                        if (SwDmApp.Documents.TryGet(depPath, out ISwDmDocument curDoc))
                        {
                            depDoc = (ISwDmDocument3D)curDoc;
                        }

                        if (depDoc == null)
                        {
                            if (!isVirtual || !TryFindVirtualDocument(depPath, out depDoc))
                            {
                                try
                                {
                                    depDoc = (ISwDmDocument3D)SwDmApp.Documents.PreCreateFromPath(depPath);
                                }
                                catch
                                {
                                    depDoc = SwDmApp.Documents.PreCreate<ISwDmDocument3D>();
                                    depDoc.Path = depPath;
                                }

                                if (State.HasFlag(DocumentState_e.ReadOnly))
                                {
                                    depDoc.State = DocumentState_e.ReadOnly;
                                }
                            }
                        }

                        yield return depDoc;
                    }
                }
            }
        }

        public bool IsCommitted => m_Creator.IsCreated;

        public ISwDmCustomPropertiesCollection Properties => m_Properties.Value;

        public event DataStoreAvailableDelegate StreamReadAvailable;
        public event DataStoreAvailableDelegate StorageReadAvailable;
        public event DataStoreAvailableDelegate StreamWriteAvailable;
        public event DataStoreAvailableDelegate StorageWriteAvailable;
        
        public event DocumentSaveDelegate Saving;
        public event DocumentCloseDelegate Closing;
        
        protected readonly ElementCreator<ISwDMDocument> m_Creator;

        internal protected ISwDmApplication SwDmApp { get; }

        protected readonly Action<ISwDmDocument> m_CreateHandler;
        protected readonly Action<ISwDmDocument> m_CloseHandler;

        private readonly Lazy<ISwDmCustomPropertiesCollection> m_Properties;

        private readonly List<ISwDmDocument3D> m_VirtualDocumentsCache;

        internal SwDmDocument(ISwDmApplication dmApp, ISwDMDocument doc, bool isCreated, 
            Action<ISwDmDocument> createHandler, Action<ISwDmDocument> closeHandler,
            bool? isReadOnly = null) : base(doc)
        {
            SwDmApp = dmApp;

            m_IsReadOnly = isReadOnly;

            m_CreateHandler = createHandler;
            m_CloseHandler = closeHandler;

            m_VirtualDocumentsCache = new List<ISwDmDocument3D>();

            m_Creator = new ElementCreator<ISwDMDocument>(OpenDocument, doc, isCreated);

            m_Properties = new Lazy<ISwDmCustomPropertiesCollection>(() => new SwDmDocumentCustomPropertiesCollection(this));
        }

        public override object Dispatch => Document;

        public int UpdateStamp => Document.GetLastUpdateStamp();
        
        public override bool Equals(IXObject other)
        {
            if (!object.ReferenceEquals(this, other)
                && other is ISwDmDocument
                && !IsCommitted && !((ISwDmDocument)other).IsCommitted)
            {
                return !string.IsNullOrEmpty(Path) && !string.IsNullOrEmpty(((ISwDmDocument)other).Path)
                    && string.Equals(Path, ((ISwDmDocument)other).Path, StringComparison.CurrentCultureIgnoreCase);
            }
            else
            {
                return base.Equals(other);
            }
        }

        private ISwDMDocument OpenDocument(CancellationToken cancellationToken) 
        {
            m_IsReadOnly = State.HasFlag(DocumentState_e.ReadOnly);

            var doc = OpenDocument(Path, State);

            StreamReadAvailable?.Invoke(this);
            StorageReadAvailable?.Invoke(this);

            return doc;
        }

        private ISwDMDocument OpenDocument(string path, DocumentState_e state)
        {
            var isReadOnly = state.HasFlag(DocumentState_e.ReadOnly);

            var doc = SwDmApp.SwDocMgr.GetDocument(path, GetDocumentType(path),
                isReadOnly, out SwDmDocumentOpenError err);

            if (doc != null)
            {
                return doc;
            }
            else
            {
                string errDesc;

                switch (err)
                {
                    case SwDmDocumentOpenError.swDmDocumentOpenErrorFail:
                        errDesc = "Generic error";
                        break;

                    case SwDmDocumentOpenError.swDmDocumentOpenErrorNonSW:
                        errDesc = "Not a native SOLIDWORKS file";
                        break;

                    case SwDmDocumentOpenError.swDmDocumentOpenErrorFileNotFound:
                        errDesc = "File not found";
                        break;

                    case SwDmDocumentOpenError.swDmDocumentOpenErrorFileReadOnly:
                        throw new DocumentWriteAccessDeniedException(path, (int)err);

                    case SwDmDocumentOpenError.swDmDocumentOpenErrorNoLicense:
                        errDesc = "No Document Manager license found";
                        break;

                    case SwDmDocumentOpenError.swDmDocumentOpenErrorFutureVersion:
                        errDesc = "Opening future version of the file";
                        break;

                    default:
                        errDesc = "Unknown error";
                        break;
                }

                throw new OpenDocumentFailedException(path, (int)err, errDesc);
            }
        }

        public void Close()
        {
            if (!m_IsClosed.HasValue || !m_IsClosed.Value)
            {
                Document.CloseDoc();
                Closing?.Invoke(this, DocumentCloseType_e.Destroy);

                m_CloseHandler.Invoke(this);
                m_IsClosed = true;
                Disposed?.Invoke(this);
            }
        }

        public virtual void Commit(CancellationToken cancellationToken)
        {
            m_Creator.Create(cancellationToken);
            m_CreateHandler.Invoke(this);
        }

        public IStorage OpenStorage(string name, AccessType_e access)
        {
            if (SwDmApp.IsVersionNewerOrEqual(SwDmVersion_e.Sw2015)) 
            {
                return new SwDm3rdPartyStorage((ISwDMDocument19)Document, name, access);
            }
            else 
            {
                throw new NotSupportedException("This API is only available in SOLIDWORKS 2015 or newer");
            }
        }

        public Stream OpenStream(string name, AccessType_e access)
        {
            if (SwDmApp.IsVersionNewerOrEqual(SwDmVersion_e.Sw2015))
            {
                return new SwDm3rdPartyStream((ISwDMDocument19)Document, name, access);
            }
            else
            {
                throw new NotSupportedException("This API is only available in SOLIDWORKS 2015 or newer");
            }
        }

        public void Save()
            => PerformSave(DocumentSaveType_e.SaveCurrent, f =>
            {
                if (!string.Equals(f, Path))
                {
                    throw new NotSupportedException("File name can be changed for SaveAs file only");
                }

                return true;
            }, (d, f) => d.Save());

        public void SaveAs(string filePath)
            => PerformSave(DocumentSaveType_e.SaveAs, f => true, (d, f) => d.SaveAs(f));

        private void PerformSave(DocumentSaveType_e saveType, Func<string, bool> canSave,
            Func<ISwDMDocument, string, SwDmDocumentSaveError> saveFunc) 
        {
            var saveArgs = new DocumentSaveArgs();
            saveArgs.FileName = Path;

            Saving?.Invoke(this, saveType, saveArgs);

            if (!saveArgs.Cancel)
            {
                if (canSave.Invoke(saveArgs.FileName))
                {
                    StreamWriteAvailable?.Invoke(this);
                    StorageWriteAvailable?.Invoke(this);

                    var res = saveFunc.Invoke(Document, saveArgs.FileName);

                    if (ProcessSaveResult(res))
                    {
                        IsDirty = false;
                    }
                }
            }
        }

        private bool ProcessSaveResult(SwDmDocumentSaveError res)
        {
            if (res != SwDmDocumentSaveError.swDmDocumentSaveErrorNone)
            {
                string errDesc = "";

                switch (res)
                {
                    case SwDmDocumentSaveError.swDmDocumentSaveErrorFail:
                        errDesc = "Generic save error";
                        break;

                    case SwDmDocumentSaveError.swDmDocumentSaveErrorReadOnly:
                        errDesc = "Cannot save read-only file";
                        break;
                }

                throw new SaveDocumentFailedException((int)res, errDesc);
            }

            return true;
        }

        private bool IsFileExtensionShown
        {
            get
            {
                try
                {
                    const string REG_KEY = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
                    const int UNCHECKED = 0;
                    var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(REG_KEY);

                    if (key != null)
                    {
                        return (int)key.GetValue("HideFileExt") == UNCHECKED;
                    }
                }
                catch
                {
                }

                return false;
            }
        }

        private Tuple<string, bool>[] GetRawDependencies()
        {
            if (!IsCommitted)
            {
                this.Commit();
            }

            string[] deps;
            object isVirtualObj;

            var searchOpts = SwDmApp.SwDocMgr.GetSearchOptionObject();

            searchOpts.SearchFilters = (int)(
                SwDmSearchFilters.SwDmSearchExternalReference
                | SwDmSearchFilters.SwDmSearchRootAssemblyFolder
                | SwDmSearchFilters.SwDmSearchSubfolders
                | SwDmSearchFilters.SwDmSearchInContextReference);

            if (SwDmApp.IsVersionNewerOrEqual(SwDmVersion_e.Sw2017))
            {
                deps = ((ISwDMDocument21)Document).GetAllExternalReferences5(searchOpts, out _, out isVirtualObj, out _, out _) as string[];
            }
            else
            {
                deps = ((ISwDMDocument13)Document).GetAllExternalReferences4(searchOpts, out _, out isVirtualObj, out _) as string[];
            }

            if (deps != null)
            {
                var isVirtual = (bool[])isVirtualObj;

                if (isVirtual.Length != deps.Length)
                {
                    throw new Exception("Invalid API. Number of virtual components information does not match references count");
                }

                var res = new Tuple<string, bool>[deps.Length];

                for (int i = 0; i < res.Length; i++)
                {
                    res[i] = new Tuple<string, bool>(deps[i], isVirtual[i]);
                }

                return res;
            }
            else
            {
                return new Tuple<string, bool>[0];
            }
        }

        public TObj CreateObjectFromDispatch<TObj>(object disp) where TObj : ISwDmObject
            => SwDmObjectFactory.FromDispatch<TObj>(disp, this);

        public void Dispose()
        {
            if (m_IsClosed != true)
            {
                if (IsAlive)
                {
                    Close();
                }
            }
        }
    }

    internal class SwDmUnknownDocument : SwDmDocument, IXUnknownDocument
    {
        private SwDmDocument m_SpecificDoc;

        public SwDmUnknownDocument(ISwDmApplication dmApp, SwDMDocument doc, bool isCreated,
            Action<ISwDmDocument> createHandler, Action<ISwDmDocument> closeHandler, bool? isReadOnly = null) 
            : base(dmApp, doc, isCreated, createHandler, closeHandler, isReadOnly)
        {
            if (isCreated)
            {
                m_CreateHandler.Invoke((ISwDmDocument)GetSpecific());
            }
        }

        public override void Commit(CancellationToken cancellationToken)
        {
            m_Creator.Create(cancellationToken);
            m_CreateHandler.Invoke((ISwDmDocument)GetSpecific());
        }

        public IXDocument GetSpecific()
        {
            if (m_SpecificDoc != null)
            {
                return m_SpecificDoc;
            }

            var model = IsCommitted ? Document : null;

            var isReadOnly = State.HasFlag(DocumentState_e.ReadOnly);

            switch (GetDocumentType(Path))
            {
                case SwDmDocumentType.swDmDocumentPart:
                    m_SpecificDoc = new SwDmPart(SwDmApp, model, IsCommitted, m_CreateHandler, m_CloseHandler, isReadOnly);
                    break;

                case SwDmDocumentType.swDmDocumentAssembly:
                    m_SpecificDoc = new SwDmAssembly(SwDmApp, model, IsCommitted, m_CreateHandler, m_CloseHandler, isReadOnly);
                    break;

                case SwDmDocumentType.swDmDocumentDrawing:
                    m_SpecificDoc = new SwDmDrawing(SwDmApp, model, IsCommitted, m_CreateHandler, m_CloseHandler, isReadOnly);
                    break;

                default:
                    throw new Exception("Invalid document type");
            }

            if (!IsCommitted) 
            {
                //TODO: implement copy cache on ElementCreator
                m_SpecificDoc.Path = Path;
            }

            return m_SpecificDoc;
        }
    }

    internal class SwDmUnknownDocument3D : SwDmUnknownDocument, ISwDmDocument3D
    {
        public SwDmUnknownDocument3D(ISwDmApplication dmApp, SwDMDocument doc, bool isCreated, Action<ISwDmDocument> createHandler, Action<ISwDmDocument> closeHandler, bool? isReadOnly = null)
            : base(dmApp, doc, isCreated, createHandler, closeHandler, isReadOnly)
        {
        }

        public ISwDmConfigurationCollection Configurations => throw new NotImplementedException();
        public IXModelViewRepository ModelViews => throw new NotImplementedException();
        IXConfigurationRepository IXDocument3D.Configurations => throw new NotImplementedException();
        public IXBoundingBox PreCreateBoundingBox() => throw new NotImplementedException();
        public IXMassProperty PreCreateMassProperty() => throw new NotImplementedException();
        TSelObject IXObjectContainer.ConvertObject<TSelObject>(TSelObject obj) => throw new NotImplementedException();
    }
}
