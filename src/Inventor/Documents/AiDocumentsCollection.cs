//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Inventor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Documents.Services;
using Xarial.XCad.Inventor.Utils;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.Inventor.Documents
{
    /// <summary>
    /// Autodesk Inventor specific documents collection
    /// </summary>
    public interface IAiDocumentsCollection : IXDocumentRepository 
    {
        /// <inheritdoc/>
        IAiDocument this[Document doc] { get; }
    }

    internal class AiDocumentsCollection : IAiDocumentsCollection, IDisposable
    {
        internal static AiDocument CreateDocument(Document nativeDoc, AiApplication app)
        {
            switch (nativeDoc)
            {
                case PartDocument part:
                    return new AiPart(part, app);

                case AssemblyDocument assm:
                    return new AiAssembly(assm, app);

                case DrawingDocument drw:
                    return new AiDrawing(drw, app);

                default:
                    throw new NotSupportedException();
            }
        }

        public event DocumentEventDelegate DocumentActivated;
        
        public event DocumentEventDelegate DocumentLoaded
        {
            add
            {
                if (m_DocumentLoaded == null)
                {
                    m_App.Application.ApplicationEvents.OnInitializeDocument += OnInitializeDocument;
                }

                m_DocumentLoaded += value;
            }
            remove
            {
                m_DocumentLoaded -= value;

                if (m_DocumentLoaded == null)
                {
                    m_App.Application.ApplicationEvents.OnInitializeDocument -= OnInitializeDocument;
                }
            }
        }

        public event DocumentEventDelegate DocumentOpened;
        public event DocumentEventDelegate NewDocumentCreated;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private readonly AiApplication m_App;

        private readonly IXLogger m_Logger;

        private readonly DocumentsHandler m_DocsHandler;

        private DocumentEventDelegate m_DocumentLoaded;

        private readonly RepositoryHelper<IXDocument> m_RepoHelper;

        internal AiDocumentsCollection(AiApplication app, IXLogger logger) 
        {
            m_App = app;
            m_Logger = logger;

            m_RepoHelper = new RepositoryHelper<IXDocument>(this,
                () => new AiUnknownDocument(null, m_App),
                () => new AiPart(null, m_App),
                () => new AiAssembly(null, m_App),
                () => new AiDrawing(null, m_App));

            m_DocsHandler = new DocumentsHandler(app, logger);
        }

        public IXDocument this[string name] => m_RepoHelper.Get(name);

        public IAiDocument this[Document doc] => CreateDocument(doc, m_App);

        public IXDocument Active 
        {
            get 
            {
                var activeDoc = m_App.Application.ActiveDocument;

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
                if (value == null) 
                {
                    throw new ArgumentNullException();
                }

                ((IAiDocument)value).Document.Activate();
            }
        }

        public int Count => m_App.Application.Documents.Count;

        public void AddRange(IEnumerable<IXDocument> ents, CancellationToken cancellationToken)
            => m_RepoHelper.AddRange(ents, cancellationToken);

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters)
            => m_RepoHelper.FilterDefault(this, filters, reverseOrder);

        public IEnumerator<IXDocument> GetEnumerator() 
        {
            foreach (Document doc in m_App.Application.Documents)
            {
                yield return CreateDocument(doc, m_App);
            }
        }

        public THandler GetHandler<THandler>(IXDocument doc) where THandler : IDocumentHandler
            => m_DocsHandler.GetHandler<THandler>(doc);

        public T PreCreate<T>() where T : IXDocument
        {
            var doc = m_RepoHelper.PreCreate<T>();

            if (!(doc is AiDocument))
            {
                throw new InvalidCastException("Document type must be of type AiDocument");
            }

            return doc;
        }

        public void RegisterHandler<THandler>(Func<THandler> handlerFact) where THandler : IDocumentHandler
            => m_DocsHandler.RegisterHandler(handlerFact);

        public void UnregisterHandler<THandler>()
            where THandler : IDocumentHandler
            => m_DocsHandler.UnregisterHandler<THandler>();

        public void RemoveRange(IEnumerable<IXDocument> ents, CancellationToken cancellationToken)
        {
            foreach (var doc in ents.Cast<IAiDocument>().ToArray()) 
            {
                doc.Close();
            }
        }

        public bool TryGet(string name, out IXDocument ent)
        {
            var doc = m_App.Application.Documents.ItemByName[name];

            if (doc != null)
            {
                ent = CreateDocument(doc, m_App);
                return true;
            }
            else 
            {
                ent = null;
                return false;
            }
        }

        private void OnInitializeDocument(_Document DocumentObject, string FullDocumentName,
            EventTimingEnum BeforeOrAfter, NameValueMap Context, out HandlingCodeEnum HandlingCode)
        {
            if (BeforeOrAfter == EventTimingEnum.kAfter)
            {
                try
                {
                    m_DocumentLoaded?.Invoke(CreateDocument(DocumentObject, m_App));
                }
                catch (Exception ex)
                {
                    m_Logger.Log(ex);
                }
            }

            HandlingCode = HandlingCodeEnum.kEventHandled;
        }

        public void Dispose()
        {
            m_DocsHandler.Dispose();
        }
    }
}
