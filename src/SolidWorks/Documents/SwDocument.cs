//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Data;
using Xarial.XCad.Data.Enums;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Features;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Annotations;
using Xarial.XCad.SolidWorks.Data;
using Xarial.XCad.SolidWorks.Data.EventHandlers;
using Xarial.XCad.SolidWorks.Documents.EventHandlers;
using Xarial.XCad.SolidWorks.Features;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwDocument : IXDocument, IDisposable
    {
        //TODO: think how to remove this
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        ISwApplication App { get; }

        IModelDoc2 Model { get; }
        new ISwFeatureManager Features { get; }
        new ISwSelectionCollection Selections { get; }
        new ISwDimensionsCollection Dimensions { get; }
        new ISwCustomPropertiesCollection Properties { get; }
    }

    [DebuggerDisplay("{" + nameof(Title) + "}")]
    internal abstract class SwDocument : ISwDocument
    {
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

        private readonly IXLogger m_Logger;

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
                    return m_CachedPath;
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
                    m_CachedPath = value;
                }
            }
        }

        private string m_CachedPath;
        private string m_CachedTitle;

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
                    return m_CachedTitle;
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
                    m_CachedTitle = value;
                }
            }
        }

        public bool Visible
        {
            get => Model.Visible;
            set => Model.Visible = value; 
        }

        public ISwFeatureManager Features { get; }

        public ISwSelectionCollection Selections { get; }

        public ISwDimensionsCollection Dimensions { get; }

        public ISwCustomPropertiesCollection Properties { get; }

        public ISwApplication App { get; }
        
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

        private readonly ElementCreator<IModelDoc2> m_Creator;

        internal SwDocument(IModelDoc2 model, ISwApplication app, IXLogger logger) 
            : this(model, app, logger, true)
        {
        }

        internal SwDocument(IModelDoc2 model, ISwApplication app, IXLogger logger, bool created)
        {
            App = app;
            
            m_Logger = logger;

            m_Creator = new ElementCreator<IModelDoc2>(CreateDocument, model, created);

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
        
        protected IModelDoc2 CreateDocument() 
        {
            if (string.IsNullOrEmpty(Path))
            {
                return CreateNewDocument();
            }
            else 
            {
                //TODO: implement opening of the document
                throw new NotImplementedException("");
            }
        }

        protected abstract swUserPreferenceStringValue_e DefaultTemplate { get; }

        private IModelDoc2 CreateNewDocument() 
        {
            var docTemplate = App.Sw.GetUserPreferenceStringValue((int)DefaultTemplate);

            if (!string.IsNullOrEmpty(docTemplate))
            {
                var doc = App.Sw.NewDocument(docTemplate, (int)swDwgPaperSizes_e.swDwgPapersUserDefined, 0.1, 0.1) as IModelDoc2;

                if (doc != null)
                {
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

        public void Commit()
        {
            m_Creator.Create();
            AttachEvents();
        }
    }
}