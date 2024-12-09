//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

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
using System.Xml.Linq;
using Xarial.XCad.Annotations;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Exceptions;
using Xarial.XCad.Documents.Services;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Features;
using Xarial.XCad.Inventor.Utils;
using Xarial.XCad.Services;
using Xarial.XCad.Toolkit;

namespace Xarial.XCad.Inventor.Documents
{
    /// <summary>
    /// Autodesk Inventor-specific document
    /// </summary>
    public interface IAiDocument : IXDocument 
    {
        /// <summary>
        /// Pointer to a document
        /// </summary>
        Document Document { get; }
    }

    [DebuggerDisplay("{" + nameof(Title) + "}")]
    internal abstract class AiDocument : AiObject, IAiDocument
    {
        public event DataStoreAvailableDelegate StreamReadAvailable;

        public event DataStoreAvailableDelegate StorageReadAvailable;
        public event DataStoreAvailableDelegate StreamWriteAvailable;
        public event DataStoreAvailableDelegate StorageWriteAvailable;
        public event DocumentEventDelegate Rebuilt;
        public event DocumentSaveDelegate Saving;
        public event DocumentSavedDelegate Saved;
        public event DocumentCloseDelegate Closing;
        public event DocumentEventDelegate Destroyed;

        public Document Document => m_Creator.Element;

        public IXIdentifier Id => new XIdentifier(Document.InternalName);

        public IXVersion Version
        {
            get
            {
                var softwareVersion = Document.SoftwareVersionSaved;

                return new AiVersion(softwareVersion, OwnerApplication.VersionMapper.FromApplicationRevision(softwareVersion.Major));
            }
        }

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

        public IXDocumentDependencies Dependencies => throw new NotImplementedException();

        public int UpdateStamp => throw new NotImplementedException();

        public override bool IsCommitted => m_Creator.IsCreated;

        public IXPropertyRepository Properties { get; }

        public IXDimensionRepository Dimensions => throw new NotImplementedException();

        private readonly ElementCreator<Document> m_Creator;

        private bool m_IsDisposed;
        private bool? m_IsClosed;

        private readonly IEqualityComparer<Document> m_DocumentEqualityComparer;

        private string m_Id;

        internal AiDocument(Document doc, AiApplication ownerApp) : base(doc, null, ownerApp)
        {
            m_Id = doc?.InternalName;

            Properties = new AiDocumentPropertySet(this);

            m_Creator = new ElementCreator<Document>(CreateDocument, doc, doc != null);

            Options = new AiDocumentOptions();

            m_DocumentEqualityComparer = new AiDocumentPointerEqualityComparer();

            ownerApp.Application.ApplicationEvents.OnCloseDocument += OnCloseDocument;
        }

        private void SetModel(Document doc)
        {
            m_Creator.Set(doc);
            m_Id = doc?.InternalName;
        }

        private Document CreateDocument(CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(Path))
            {
                var nameValueMap = OwnerApplication.Application.TransientObjects.CreateNameValueMap();
                var doc = OwnerApplication.Application.Documents.OpenWithOptions(Path, nameValueMap, !State.HasFlag(DocumentState_e.Hidden));
                SetModel(doc);
                return doc;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public void Close() => Document.Close(true);

        public override void Commit(CancellationToken cancellationToken)
            => m_Creator.Create(cancellationToken);

        public TObj DeserializeObject<TObj>(Stream stream) where TObj : IXObject
        {
            throw new NotImplementedException();
        }

        public IStorage OpenStorage(string name, bool write)
        {
            throw new NotImplementedException();
        }

        public Stream OpenStream(string name, bool write)
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

        protected TranslatorAddIn TryGetTranslator(string filePath)
        {
            var ext = System.IO.Path.GetExtension(filePath);

            return OwnerApplication.Application.ApplicationAddIns.OfType<TranslatorAddIn>().FirstOrDefault(a =>
            {
                var supportedExts = a.FileExtensions.Split(';').Select(e => e.TrimStart('*')).ToArray();
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

        private void OnCloseDocument(_Document DocumentObject, string FullDocumentName, EventTimingEnum BeforeOrAfter, NameValueMap Context, out HandlingCodeEnum HandlingCode)
        {
            if (BeforeOrAfter == EventTimingEnum.kAfter)
            {
                //NOTE: event is raised in different thread and API cannot be accessed and getting the id throws an exception, have to use cached value
                if (string.Equals(m_Id, DocumentObject.InternalName)) 
                {
                    m_IsClosed = true;
                    Destroyed?.Invoke(this);
                }
            }

            HandlingCode = HandlingCodeEnum.kEventHandled;
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

        public abstract IXSaveOperation PreCreateSaveAsOperation(string filePath);
    }

    internal class AiUnknownDocument : AiDocument, IXUnknownDocument
    {
        private IXDocument m_SpecificDoc;

        internal AiUnknownDocument(Document doc, AiApplication ownerApp) : base(doc, ownerApp)
        {
        }

        public IXDocument GetSpecific()
        {
            if (m_SpecificDoc != null)
            {
                return m_SpecificDoc;
            }

            var doc = Document;

            if (doc == null)
            {
                throw new Exception("Document is not yet created, cannot get specific document");
            }

            m_SpecificDoc = AiDocumentsCollection.CreateDocument(doc, OwnerApplication);

            return m_SpecificDoc;
        }

        public override IXSaveOperation PreCreateSaveAsOperation(string filePath) => throw new NotSupportedException();
    }
}
