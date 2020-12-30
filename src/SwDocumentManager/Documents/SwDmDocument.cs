//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections.Generic;
using System.IO;
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
using Xarial.XCad.SwDocumentManager.Data;
using Xarial.XCad.Toolkit.Data;

namespace Xarial.XCad.SwDocumentManager.Documents
{
    public interface ISwDmDocument : IXDocument
    {
        ISwDMDocument Document { get; }
        new ISwDmVersion Version { get; }
        new ISwDmCustomPropertiesCollection Properties { get; }
    }

    internal abstract class SwDmDocument : ISwDmDocument
    {
        #region Not Supported
        
        public string Template { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public IXFeatureRepository Features => throw new NotImplementedException();
        public IXSelectionRepository Selections => throw new NotSupportedException();
        public IXDimensionRepository Dimensions => throw new NotSupportedException();
        public event DocumentRebuildDelegate Rebuild;
        public IXObject DeserializeObject(Stream stream) => throw new NotSupportedException();

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

        public ITagsManager Tags { get; }

        public bool IsAlive 
        {
            get 
            {
                try
                {
                    var testVers = Document.GetVersion();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public IXDocument3D[] Dependencies => throw new NotImplementedException();

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

        internal SwDmDocument(ISwDmApplication dmApp, ISwDMDocument doc, bool isCreated, 
            Action<ISwDmDocument> createHandler, Action<ISwDmDocument> closeHandler)
        {
            SwDmApp = dmApp;

            m_CreateHandler = createHandler;
            m_CloseHandler = closeHandler;

            Tags = new TagsManager();
            m_Creator = new ElementCreator<ISwDMDocument>(OpenDocument, doc, isCreated);

            m_Properties = new Lazy<ISwDmCustomPropertiesCollection>(() => new SwDmDocumentCustomPropertiesCollection(this));
        }

        protected SwDmDocumentType DocumentType 
        {
            get 
            {
                SwDmDocumentType docType;

                switch (System.IO.Path.GetExtension(Path).ToLower())
                {
                    case ".sldprt":
                        docType = SwDmDocumentType.swDmDocumentPart;
                        break;

                    case ".sldasm":
                        docType = SwDmDocumentType.swDmDocumentAssembly;
                        break;

                    case ".slddrw":
                        docType = SwDmDocumentType.swDmDocumentDrawing;
                        break;

                    default:
                        throw new NotSupportedException("Only native SOLIDWORKS files can be opened");
                }

                return docType;
            }
        }

        private ISwDMDocument OpenDocument(CancellationToken cancellationToken) 
        {
            m_IsReadOnly = State.HasFlag(DocumentState_e.ReadOnly);

            var doc = SwDmApp.SwDocMgr.GetDocument(Path, DocumentType, 
                m_IsReadOnly.Value, out SwDmDocumentOpenError err);

            if (doc != null)
            {
                StreamReadAvailable?.Invoke(this);
                StorageReadAvailable?.Invoke(this);

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
                        errDesc = "File is read-only and cannot be opened for write access";
                        break;

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

                throw new OpenDocumentFailedException(Path, (int)err, errDesc);
            }
        }

        public void Close()
        {
            Document.CloseDoc();
            Closing?.Invoke(this);

            m_CloseHandler.Invoke(this);
        }

        public virtual void Commit(CancellationToken cancellationToken)
        {
            m_Creator.Create(cancellationToken);
            m_CreateHandler.Invoke(this);
        }

        public IStorage OpenStorage(string name, AccessType_e access)
        {
            throw new NotImplementedException();
        }

        public Stream OpenStream(string name, AccessType_e access)
        {
            throw new NotImplementedException();
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
            saveArgs.FileName = Path;

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
    }

    internal class SwDmUnknownDocument : SwDmDocument, IXUnknownDocument
    {
        private IXDocument m_SpecificDoc;

        public SwDmUnknownDocument(ISwDmApplication dmApp, SwDMDocument doc, bool isCreated,
            Action<ISwDmDocument> createHandler, Action<ISwDmDocument> closeHandler) 
            : base(dmApp, doc, isCreated, createHandler, closeHandler)
        {
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

            var model = Document;

            if (model == null)
            {
                throw new Exception("Model is not yet created, cannot get specific document");
            }

            switch (DocumentType)
            {
                case SwDmDocumentType.swDmDocumentPart:
                    m_SpecificDoc = new SwDmPart(SwDmApp, model, true, m_CreateHandler, m_CloseHandler);
                    break;

                case SwDmDocumentType.swDmDocumentAssembly:
                    m_SpecificDoc = new SwDmAssembly(SwDmApp, model, true, m_CreateHandler, m_CloseHandler);
                    break;

                case SwDmDocumentType.swDmDocumentDrawing:
                    m_SpecificDoc = new SwDmDrawing(SwDmApp, model, true, m_CreateHandler, m_CloseHandler);
                    break;

                default:
                    throw new Exception("Invalid document type");
            }

            return m_SpecificDoc;
        }
    }
}
