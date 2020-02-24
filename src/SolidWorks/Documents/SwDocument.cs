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
using Xarial.XCad.Annotations;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Features;
using Xarial.XCad.SolidWorks.Annotations;
using Xarial.XCad.SolidWorks.Data;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.Utils.Diagnostics;

namespace Xarial.XCad.SolidWorks.Documents
{
    [DebuggerDisplay("{" + nameof(Title) + "}")]
    public abstract class SwDocument : IXDocument, IDisposable
    {
        public event DocumentCloseDelegate Closing;

        internal event Action<IModelDoc2> Destroyed;

        IXFeatureRepository IXDocument.Features => Features;
        IXSelectionRepository IXDocument.Selections => Selections;
        IXDimensionRepository IXDocument.Dimensions => Dimensions;
        IXPropertyRepository IXDocument.Properties => Properties;

        private readonly ISldWorks m_App;
        private readonly ILogger m_Logger;
        
        public IModelDoc2 Model { get; }

        public string Path => Model.GetPathName();
        public string Title => Model.GetTitle();

        public SwFeatureManager Features { get; }

        public SwSelectionCollection Selections { get; }

        public SwDimensionsCollection Dimensions { get; }

        public SwCustomPropertiesCollection Properties { get; }

        internal ISldWorks App => m_App;

        internal SwDocument(IModelDoc2 model, ISldWorks app, ILogger logger)
        {
            Model = model;

            m_App = app;
            m_Logger = logger;

            Features = new SwFeatureManager(this, model.FeatureManager, m_App);
            
            Selections = new SwSelectionCollection(model);

            Dimensions = new SwDimensionsCollection(this);

            Properties = new SwCustomPropertiesCollection(App, Model, "");

            AttachEvents();
        }

        public void Close()
        {
            m_App.CloseDoc(Title);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            Properties.Dispose();

            if (disposing)
            {
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
    }
}