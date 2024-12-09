//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
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
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Exceptions;
using Xarial.XCad.Documents.Services;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Data;
using Xarial.XCad.SwDocumentManager.Data;
using Xarial.XCad.Toolkit;
using Xarial.XCad.Toolkit.Data;
using Xarial.XCad.UI;

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
        /// <summary>
        /// <see cref="ISwDmComponent.CachedPath"/> returns the last path when file was saved within SOLIDWORKS
        /// If files were renamed with Pack&Go, SOLIDWORKS File Utilities, PDM or Document Manager cached path will not be changed until opened
        /// </summary>
        internal class ChangedReferencesCollection 
        {
            private readonly string[] m_OriginalReferences;
            private readonly string[] m_NewReferences;

            internal ChangedReferencesCollection(ISwDMDocument doc) 
            {
                ((ISwDMDocument8)doc).GetChangedReferences(out object origRefs, out object newRefs);

                m_OriginalReferences = (string[])origRefs ?? new string[0];
                m_NewReferences = (string[])newRefs ?? new string[0];

                if (m_OriginalReferences.Length != m_NewReferences.Length) 
                {
                    throw new Exception("Count of original references does not match count of new references");
                }
            }

            internal IEnumerable<string> EnumerateByFileName(string filePath) 
            {
                for (int i = 0; i < m_OriginalReferences.Length; i++)
                {
                    var origRef = m_OriginalReferences[i];

                    if (string.Equals(System.IO.Path.GetFileName(origRef), System.IO.Path.GetFileName(filePath), StringComparison.CurrentCultureIgnoreCase))
                    {
                        yield return m_NewReferences[i];
                    }
                }
            }
        }

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
        public IOperationGroup PreCreateOperationGroup() => throw new NotSupportedException();
        public IXUnits Units => throw new NotSupportedException();
        public IXModelViewRepository ModelViews => throw new NotSupportedException();
        public IXAnnotationRepository Annotations => throw new NotSupportedException();

        #endregion

        IXVersion IXDocument.Version => Version;
        IXPropertyRepository IXDocument.Properties => Properties;

        public ISwDMDocument Document => m_Creator.Element;

        public IXIdentifier Id 
        {
            get 
            {
                if (OwnerApplication.IsVersionNewerOrEqual(SwDmVersion_e.Sw2015))
                {
                    var id = Convert.ToInt64(((ISwDMDocument19)Document).CreationDate2);
                    return new XIdentifier(id);
                }
                else 
                {
                    var creationDate = DateTime.Parse(Document.CreationDate).ToUniversalTime();
                    var id = new DateTimeOffset(creationDate).ToUnixTimeSeconds();
                    return new XIdentifier(id);
                }
            }
        }

        public ISwDmVersion Version => SwDmApplicationFactory.CreateVersion(OwnerApplication.VersionMapper.FromFileRevision(Document.GetVersion()));

        public virtual string Title 
        {
            get 
            {
                var path = Path;

                if (!string.IsNullOrEmpty(path))
                {
                    if (IsFileExtensionShown)
                    {
                        return System.IO.Path.GetFileName(path);
                    }
                    else
                    {
                        return System.IO.Path.GetFileNameWithoutExtension(path);
                    }
                }
                else 
                {
                    return "";
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

        public IXDocumentDependencies Dependencies { get; }

        public override bool IsCommitted => m_Creator.IsCreated;

        public ISwDmCustomPropertiesCollection Properties => m_Properties.Value;

        public event DataStoreAvailableDelegate StreamReadAvailable;
        public event DataStoreAvailableDelegate StorageReadAvailable;
        public event DataStoreAvailableDelegate StreamWriteAvailable;
        public event DataStoreAvailableDelegate StorageWriteAvailable;
        
        public event DocumentSaveDelegate Saving;
        public event DocumentCloseDelegate Closing;
        public event DocumentEventDelegate Destroyed;
        public event DocumentSavedDelegate Saved;

        protected readonly IElementCreator<ISwDMDocument> m_Creator;

        internal ChangedReferencesCollection ChangedReferences => m_ChangedReferencesLazy.Value;

        protected readonly Action<ISwDmDocument> m_CreateHandler;
        protected readonly Action<ISwDmDocument> m_CloseHandler;

        private readonly Lazy<ISwDmCustomPropertiesCollection> m_Properties;
        private readonly Lazy<ChangedReferencesCollection> m_ChangedReferencesLazy;

        internal SwDmDocument(SwDmApplication dmApp, ISwDMDocument doc, bool isCreated, 
            Action<ISwDmDocument> createHandler, Action<ISwDmDocument> closeHandler,
            bool? isReadOnly = null) : base(doc, dmApp, null)
        {
            m_IsReadOnly = isReadOnly;

            m_CreateHandler = createHandler;
            m_CloseHandler = closeHandler;

            Dependencies = new SwDmDocumentDependencies(this);

            m_Creator = new ElementCreator<ISwDMDocument>(OpenDocument, doc, isCreated);

            m_Properties = new Lazy<ISwDmCustomPropertiesCollection>(() => new SwDmDocumentCustomPropertiesCollection(this));
            m_ChangedReferencesLazy = new Lazy<ChangedReferencesCollection>(() => new ChangedReferencesCollection(Document));
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

            var docType = GetDocumentType(path);

            if (!IsDocumentTypeCompatible(docType))
            {
                throw new DocumentPathIncompatibleException(this);
            }

            var doc = OwnerApplication.SwDocMgr.GetDocument(path, docType,
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

        protected abstract bool IsDocumentTypeCompatible(SwDmDocumentType docType);

        public void Close()
        {
            if (!m_IsClosed.HasValue || !m_IsClosed.Value)
            {
                Document.CloseDoc();
                Closing?.Invoke(this, DocumentCloseType_e.Destroy);

                m_CloseHandler.Invoke(this);
                m_IsClosed = true;
                Destroyed?.Invoke(this);
            }
        }

        public override void Commit(CancellationToken cancellationToken)
        {
            m_Creator.Create(cancellationToken);
            m_CreateHandler.Invoke(this);
        }

        public IStorage OpenStorage(string name, bool write)
        {
            if (this.IsVersionNewerOrEqual(SwDmVersion_e.Sw2015)) 
            {
                var storage = new SwDm3rdPartyStorage((ISwDMDocument19)Document, name, write);

                if (storage.Exists)
                {
                    return storage;
                }
                else 
                {
                    return Storage.Null;
                }
            }
            else 
            {
                throw new NotSupportedException("This API is only available in SOLIDWORKS 2015 or newer");
            }
        }

        public Stream OpenStream(string name, bool write)
        {
            if (this.IsVersionNewerOrEqual(SwDmVersion_e.Sw2015))
            {
                var stream = new SwDm3rdPartyStream((ISwDMDocument19)Document, name, write);

                if (stream.Exists)
                {
                    return stream;
                }
                else 
                {
                    return Stream.Null;
                }
            }
            else
            {
                throw new NotSupportedException("This API is only available in SOLIDWORKS 2015 or newer");
            }
        }

        public void Save()
            => PerformSave(DocumentSaveType_e.SaveCurrent, Path, f =>
            {
                if (!string.Equals(f, Path))
                {
                    throw new NotSupportedException("File name can be changed for SaveAs file only");
                }

                return true;
            }, (d, f) => d.Save());

        public IXSaveOperation PreCreateSaveAsOperation(string filePath)
            => new SwDmSaveOperation(this, filePath);

        internal void PerformSave(DocumentSaveType_e saveType, string path, Func<string, bool> canSave,
            Func<ISwDMDocument, string, SwDmDocumentSaveError> saveFunc) 
        {
            var saveArgs = new DocumentSaveArgs();
            saveArgs.FileName = path;

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
                        Saved?.Invoke(this, saveType, false);
                    }
                }
            }
            else 
            {
                Saved?.Invoke(this, saveType, true);
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

        public IXDocumentOptions Options => throw new NotImplementedException();

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

        public SwDmUnknownDocument(SwDmApplication dmApp, SwDMDocument doc, bool isCreated,
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
                    m_SpecificDoc = new SwDmPart(OwnerApplication, model, IsCommitted, m_CreateHandler, m_CloseHandler, isReadOnly);
                    break;

                case SwDmDocumentType.swDmDocumentAssembly:
                    m_SpecificDoc = new SwDmAssembly(OwnerApplication, model, IsCommitted, m_CreateHandler, m_CloseHandler, isReadOnly);
                    break;

                case SwDmDocumentType.swDmDocumentDrawing:
                    m_SpecificDoc = new SwDmDrawing(OwnerApplication, model, IsCommitted, m_CreateHandler, m_CloseHandler, isReadOnly);
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

        protected override bool IsDocumentTypeCompatible(SwDmDocumentType docType) => true;
    }

    internal class SwDmUnknownDocument3D : SwDmUnknownDocument, ISwDmDocument3D
    {
        public SwDmUnknownDocument3D(SwDmApplication dmApp, SwDMDocument doc, bool isCreated, Action<ISwDmDocument> createHandler, Action<ISwDmDocument> closeHandler, bool? isReadOnly = null)
            : base(dmApp, doc, isCreated, createHandler, closeHandler, isReadOnly)
        {
        }

        IXModelView3DRepository IXDocument3D.ModelViews => throw new NotSupportedException();
        public ISwDmConfigurationCollection Configurations => throw new NotSupportedException();
        public IXDocumentEvaluation Evaluation => throw new NotSupportedException();
        public IXDocumentGraphics Graphics => throw new NotSupportedException();
        IXConfigurationRepository IXDocument3D.Configurations => throw new NotSupportedException();
        TSelObject IXObjectContainer.ConvertObject<TSelObject>(TSelObject obj) => throw new NotSupportedException();
        IXDocument3DSaveOperation IXDocument3D.PreCreateSaveAsOperation(string filePath) => throw new NotSupportedException();
    }

    public static class SwDmDocumentExtension 
    {
        public static bool IsVersionNewerOrEqual(this ISwDmDocument doc, SwDmVersion_e version)
            => doc.Version.IsVersionNewerOrEqual(version);
    }
}
