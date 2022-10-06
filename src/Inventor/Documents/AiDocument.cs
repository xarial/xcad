using Inventor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.XCad.Annotations;
using Xarial.XCad.Data;
using Xarial.XCad.Data.Enums;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Exceptions;
using Xarial.XCad.Documents.Services;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Features;
using Xarial.XCad.Services;

namespace Xarial.XCad.Inventor.Documents
{
    public interface IAiDocument : IXDocument 
    {
        Document Document { get; }
    }

    [DebuggerDisplay("{" + nameof(Title) + "}")]
    internal class AiDocument : AiObject, IAiDocument
    {
        public event DataStoreAvailableDelegate StreamReadAvailable;

        public event DataStoreAvailableDelegate StorageReadAvailable;
        public event DataStoreAvailableDelegate StreamWriteAvailable;
        public event DataStoreAvailableDelegate StorageWriteAvailable;
        public event DocumentEventDelegate Rebuilt;
        public event DocumentSaveDelegate Saving;
        public event DocumentCloseDelegate Closing;

        public Document Document => m_Creator.Element;

        public IXVersion Version => throw new NotImplementedException();

        public IXUnits Units => throw new NotImplementedException();

        public IXDocumentOptions Options { get; }

        public string Title
        {
            get
            {
                if (IsCommitted)
                {
                    return Document.DisplayName;
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
                    throw new CommitedElementReadOnlyParameterException();
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public string Template { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Path
        {
            get
            {
                if (IsCommitted)
                {
                    return Document.FullFileName;
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
                    throw new CommitedElementReadOnlyParameterException();
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public bool IsDirty
        {
            get => Document.Dirty;
            set => Document.Dirty = value;
        }

        public DocumentState_e State
        {
            get
            {
                if (IsCommitted)
                {
                    throw new NotSupportedException();
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
                    throw new CommitedElementReadOnlyParameterException();
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public IXModelViewRepository ModelViews => throw new NotImplementedException();

        public IXAnnotationRepository Annotations => throw new NotImplementedException();

        public IXFeatureRepository Features => throw new NotImplementedException();

        public IXSelectionRepository Selections => throw new NotImplementedException();

        public IEnumerable<IXDocument3D> Dependencies => throw new NotImplementedException();

        public int UpdateStamp => throw new NotImplementedException();

        public bool IsCommitted => m_Creator.IsCreated;

        public IXPropertyRepository Properties => throw new NotImplementedException();

        public IXDimensionRepository Dimensions => throw new NotImplementedException();

        private readonly ElementCreator<Document> m_Creator;

        private bool m_IsDisposed;
        private bool? m_IsClosed;

        internal AiDocument(Document doc, AiApplication ownerApp) : base(doc, null, ownerApp)
        {
            m_Creator = new ElementCreator<Document>(CreateDocument, doc, doc != null);

            Options = new AiDocumentOptions();
        }

        internal void SetModel(Document doc) => m_Creator.Set(doc);

        private Document CreateDocument(CancellationToken cancellationToken)
        {
            var dispatcher = ((AiDocumentsCollection)OwnerApplication.Documents).Dispatcher;

            dispatcher.BeginDispatch(this);

            Document doc = null;

            try
            {
                if (!string.IsNullOrEmpty(Path))
                {
                    var nameValueMap = OwnerApplication.Application.TransientObjects.CreateNameValueMap();
                    doc = OwnerApplication.Application.Documents.OpenWithOptions(Path, nameValueMap, !State.HasFlag(DocumentState_e.Hidden));
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            finally
            {
                if (doc != null)
                {
                    dispatcher.EndDispatch(this, doc);
                }
                else
                {
                    dispatcher.TryRemoveFromDispatchQueue(this);
                }
            }

            return doc;
        }

        public void Close() => Document.Close(true);

        public void Commit(CancellationToken cancellationToken)
            => m_Creator.Create(cancellationToken);

        public TObj DeserializeObject<TObj>(Stream stream) where TObj : IXObject
        {
            throw new NotImplementedException();
        }

        public IStorage OpenStorage(string name, AccessType_e access)
        {
            throw new NotImplementedException();
        }

        public Stream OpenStream(string name, AccessType_e access)
        {
            throw new NotImplementedException();
        }

        public IOperationGroup PreCreateOperationGroup()
        {
            throw new NotImplementedException();
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        internal void SetClosed()
        {
            m_IsClosed = true;
        }

        public void Rebuild() => Document.Rebuild2(true);

        public void Save() => Document.Save2(true);

        public void SaveAs(string filePath)
        {
            var translator = TryGetTranslator(filePath);

            if (translator != null)
            {
                var context = OwnerApplication.Application.TransientObjects.CreateTranslationContext();

                var opts = OwnerApplication.Application.TransientObjects.CreateNameValueMap();

                SetSaveOptions(translator, opts, Options.SaveOptions);

                if (translator.HasSaveCopyAsOptions[Document, context, opts])
                {
                    context.Type = IOMechanismEnum.kFileBrowseIOMechanism;

                    var data = OwnerApplication.Application.TransientObjects.CreateDataMedium();
                    data.FileName = filePath;

                    DateTime? existingFileDate = null;

                    if (System.IO.File.Exists(filePath))
                    {
                        existingFileDate = System.IO.File.GetLastWriteTimeUtc(filePath);
                    }

                    translator.SaveCopyAs(Document, context, opts, data);

                    if (System.IO.File.Exists(filePath))
                    {
                        if (existingFileDate.HasValue)
                        {
                            if (System.IO.File.GetLastWriteTimeUtc(filePath) == existingFileDate)
                            {
                                throw new SaveDocumentFailedException(-1, "Failed to export file (file is not overwritten)");
                            }
                        }
                    }
                    else
                    {
                        throw new SaveDocumentFailedException(-1, "Failed to export file (file does not exist)");
                    }
                }
                else
                {
                    throw new SaveDocumentFailedException(-1, "Invalid options");
                }
            }
            else
            {
                Document.SaveAs(filePath, true);
            }
        }

        private void SetSaveOptions(TranslatorAddIn translator, NameValueMap opts, IXSaveOptions saveOpts) 
        {
            switch (translator.ClientId) 
            {
                case "{90AF7F40-0C01-11D5-8E83-0010B541CD80}":
                    
                    int protocolType;

                    switch (saveOpts.Step.Format) 
                    {
                        case StepFormat_e.Ap203:
                            protocolType = 2;
                            break;

                        case StepFormat_e.Ap214:
                            protocolType = 3;
                            break;

                        case StepFormat_e.Ap242:
                            protocolType = 5;
                            break;

                        default:
                            throw new NotSupportedException();
                    }
                    opts.Value["ApplicationProtocolType"] = protocolType;
                    break;
            }
        }

        private TranslatorAddIn TryGetTranslator(string filePath)
        {
            var ext = System.IO.Path.GetExtension(filePath);

            return OwnerApplication.Application.ApplicationAddIns.OfType<TranslatorAddIn>().FirstOrDefault(a =>
            {
                var supportedExts = a.FileExtensions.Split(';').Select(e=>e.TrimStart('*')).ToArray();
                return supportedExts.Contains(ext, StringComparer.CurrentCultureIgnoreCase);
            });
        }

        public override bool IsAlive 
        {
            get 
            {
                try
                {
                    var test = Document.InternalName;
                    return true;
                }
                catch 
                {
                    return false;
                }
            }
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
        }
    }
}
