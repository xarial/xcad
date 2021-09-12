//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Xarial.XCad.Annotations;
using Xarial.XCad.Data;
using Xarial.XCad.Data.Enums;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Exceptions;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.Features;
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
        public event DocumentEventDelegate Rebuilt;
        public TObj DeserializeObject<TObj>(Stream stream) where TObj : IXObject => throw new NotSupportedException();
        public void Rebuild() => throw new NotSupportedException();
        public IXUnits Units => throw new NotSupportedException();

        #endregion

        IXVersion IXDocument.Version => Version;
        IXPropertyRepository IPropertiesOwner.Properties => Properties;

        public ISwDMDocument Document => m_Creator.Element;

        public ISwDmVersion Version => SwDmApplicationFactory.CreateVersion((SwDmVersion_e)Document.GetVersion());

        public string Title 
        {
            get => System.IO.Path.GetFileName(Path);
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

        public bool IsDirty { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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
                ISwDMDocument doc = null;

                try
                {
                    if (IsCommitted)
                    {
                        doc = Document;
                    }
                    else
                    {
                        doc = OpenDocument(Path, DocumentState_e.ReadOnly);
                    }

                    var searchOpts = SwDmApp.SwDocMgr.GetSearchOptionObject();
                    searchOpts.SearchFilters = (int)(
                        SwDmSearchFilters.SwDmSearchExternalReference
                        | SwDmSearchFilters.SwDmSearchRootAssemblyFolder
                        | SwDmSearchFilters.SwDmSearchSubfolders
                        | SwDmSearchFilters.SwDmSearchInContextReference);

                    string[] deps;
                    object isVirtualObj;

                    if (SwDmApp.IsVersionNewerOrEqual(SwDmVersion_e.Sw2017))
                    {
                        deps = ((ISwDMDocument21)doc).GetAllExternalReferences5(searchOpts, out _, out isVirtualObj, out _, out _) as string[];
                    }
                    else
                    {
                        deps = ((ISwDMDocument13)doc).GetAllExternalReferences4(searchOpts, out _, out isVirtualObj, out _) as string[];
                    }

                    if (deps != null)
                    {
                        var isVirtual = (bool[])isVirtualObj;

                        if (isVirtual.Length != deps.Length) 
                        {
                            throw new Exception("Invalid API. Number of virtual components information does not match references count");
                        }

                        var compsLazy = new Lazy<ISwDmComponent[]>(
                            () =>
                            {
                                if (this is ISwDmAssembly)
                                {
                                    return ((ISwDmAssembly)this).Configurations.Active.Components.Cast<ISwDmComponent>().ToArray();
                                }
                                else 
                                {
                                    throw new Exception("Components can only be extracted from the assembly");
                                }
                            });

                        bool TryFindVirtualDocument(string filePath, out ISwDmDocument3D virtCompDoc)
                        {
                            var comp = compsLazy.Value.FirstOrDefault(c => string.Equals(
                                System.IO.Path.GetFileName(c.CachedPath), System.IO.Path.GetFileName(filePath),
                                StringComparison.CurrentCultureIgnoreCase));

                            if (comp != null)
                            {
                                try
                                {
                                    virtCompDoc = comp.ReferencedDocument;
                                    return true;
                                }
                                catch 
                                {
                                }
                            }

                            virtCompDoc = null;
                            return false;
                        }

                        for (int i = 0; i < deps.Length; i++) 
                        {
                            var depPath = deps[i];

                            ISwDmDocument3D depDoc;

                            if (!isVirtual[i] || !TryFindVirtualDocument(depPath, out depDoc))
                            {
                                depDoc = (ISwDmDocument3D)SwDmApp.Documents.PreCreateFromPath(depPath);
                            }
                            
                            yield return depDoc;
                        }
                    }
                }
                finally 
                {
                    if (!IsCommitted && doc != null) 
                    {
                        doc.CloseDoc();
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
        public event DocumentEventDelegate Closing;

        protected readonly ElementCreator<ISwDMDocument> m_Creator;

        internal protected ISwDmApplication SwDmApp { get; }

        protected readonly Action<ISwDmDocument> m_CreateHandler;
        protected readonly Action<ISwDmDocument> m_CloseHandler;

        private readonly Lazy<ISwDmCustomPropertiesCollection> m_Properties;

        internal SwDmDocument(ISwDmApplication dmApp, ISwDMDocument doc, bool isCreated, 
            Action<ISwDmDocument> createHandler, Action<ISwDmDocument> closeHandler,
            bool? isReadOnly = null) : base(doc)
        {
            SwDmApp = dmApp;

            m_IsReadOnly = isReadOnly;

            m_CreateHandler = createHandler;
            m_CloseHandler = closeHandler;

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
            Document.CloseDoc();
            Closing?.Invoke(this);

            m_CloseHandler.Invoke(this);
            m_IsClosed = true;
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
        {
            var saveArgs = new DocumentSaveArgs();
            saveArgs.FileName = Path;

            Saving?.Invoke(this, DocumentSaveType_e.SaveCurrent, saveArgs);

            if (!saveArgs.Cancel)
            {
                if (!string.Equals(saveArgs.FileName, Path))
                {
                    throw new NotSupportedException("File name can be changed for SaveAs file only");
                }

                StreamWriteAvailable?.Invoke(this);
                StorageWriteAvailable?.Invoke(this);

                var res = Document.Save();
                ProcessSaveResult(res);
            }
        }

        public void SaveAs(string filePath)
        {
            var saveArgs = new DocumentSaveArgs();
            saveArgs.FileName = filePath;

            Saving?.Invoke(this, DocumentSaveType_e.SaveAs, saveArgs);

            if (!saveArgs.Cancel)
            {
                StreamWriteAvailable?.Invoke(this);
                StorageWriteAvailable?.Invoke(this);

                //TODO: add support for saving to parasolid
                var res = Document.SaveAs(saveArgs.FileName);
                ProcessSaveResult(res);
            }
        }

        private void ProcessSaveResult(SwDmDocumentSaveError res)
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
}
