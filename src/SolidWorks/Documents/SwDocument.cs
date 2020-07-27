//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Diagnostics;
using System.IO;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Data;
using Xarial.XCad.Data.Enums;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Features;
using Xarial.XCad.SolidWorks.Annotations;
using Xarial.XCad.SolidWorks.Data;
using Xarial.XCad.SolidWorks.Data.EventHandlers;
using Xarial.XCad.SolidWorks.Documents.EventHandlers;
using Xarial.XCad.SolidWorks.Features;

namespace Xarial.XCad.SolidWorks.Documents
{
    [DebuggerDisplay("{" + nameof(Title) + "}")]
    public abstract class SwDocument : IXDocument, IDisposable
    {
        public event DocumentCloseDelegate Closing;

        internal event Action<IModelDoc2> Destroyed;

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

        public IModelDoc2 Model { get; }

        public string Path => Model.GetPathName();
        public string Title => Model.GetTitle();

        public bool Visible
        {
            get => Model.Visible;
            set => Model.Visible = value; 
        }

        public SwFeatureManager Features { get; }

        public SwSelectionCollection Selections { get; }

        public SwDimensionsCollection Dimensions { get; }

        public SwCustomPropertiesCollection Properties { get; }

        internal SwApplication App { get; }
        internal ISldWorks SwApp { get; }

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

        internal SwDocument(IModelDoc2 model, SwApplication app, IXLogger logger)
        {
            Model = model;

            App = app;
            SwApp = app.Sw;

            m_Logger = logger;

            Features = new SwFeatureManager(this, model.FeatureManager, SwApp);
            
            Selections = new SwSelectionCollection(this);

            Dimensions = new SwDimensionsCollection(this);

            Properties = new SwCustomPropertiesCollection(SwApp, Model, "");

            m_StreamReadAvailableHandler = new StreamReadAvailableEventsHandler(this);
            m_StreamWriteAvailableHandler = new StreamWriteAvailableEventsHandler(this);
            m_StorageReadAvailableHandler = new StorageReadAvailableEventsHandler(this);
            m_StorageWriteAvailableHandler = new StorageWriteAvailableEventsHandler(this);
            m_DocumentRebuildEventHandler = new DocumentRebuildEventsHandler(this);
            m_DocumentSavingEventHandler = new DocumentSavingEventHandler(this);

            AttachEvents();
        }

        public void Close()
        {
            SwApp.CloseDoc(Title);
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
                Destroyed?.Invoke(Model);

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
    }
}