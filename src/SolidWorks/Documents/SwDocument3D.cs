//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Linq;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI;
using Xarial.XCad.Utils.Diagnostics;

namespace Xarial.XCad.SolidWorks.Documents
{
    /// <summary>
    /// SOLIDWORKS-sepcific 3D document
    /// </summary>
    public interface ISwDocument3D : ISwDocument, IXDocument3D
    {
        new ISwConfigurationCollection Configurations { get; }
        new ISwModelViews3DCollection ModelViews { get; }
        new TSelObject ConvertObject<TSelObject>(TSelObject obj)
            where TSelObject : ISwSelObject;
    }

    internal abstract class SwDocument3D : SwDocument, ISwDocument3D
    {
        IXConfigurationRepository IXDocument3D.Configurations => Configurations;
        IXModelView3DRepository IXDocument3D.ModelViews => (IXModelView3DRepository)ModelViews;
        public override ISwModelViewsCollection ModelViews => ((ISwDocument3D)this).ModelViews;
        ISwModelViews3DCollection ISwDocument3D.ModelViews => m_ModelViewsLazy.Value;
        TSelObject IXObjectContainer.ConvertObject<TSelObject>(TSelObject obj) => ConvertObjectBoxed(obj) as TSelObject;

        internal SwDocument3D(IModelDoc2 model, SwApplication app, IXLogger logger, bool isCreated) : base(model, app, logger, isCreated)
        {
            m_ConfigurationsLazy = new Lazy<ISwConfigurationCollection>(CreateConfigurations);
            m_ModelViewsLazy = new Lazy<ISwModelViews3DCollection>(() => new SwModelViews3DCollection(this, app));

            Graphics = new SwDocumentGraphics(this);
            DisplayState = new SwDisplayState(new SwDocumentLevelDisplayStateDispatch(this), this, app);
        }

        private Lazy<ISwConfigurationCollection> m_ConfigurationsLazy;
        private Lazy<ISwModelViews3DCollection> m_ModelViewsLazy;

        public ISwConfigurationCollection Configurations => m_ConfigurationsLazy.Value;

        public abstract IXDocumentEvaluation Evaluation { get; }

        public IXDocumentGraphics Graphics { get; }

        public System.Drawing.Color? Color
        {
            get
            {
                if (IsCommitted)
                {
                    return GetColor(Model);
                }
                else
                {
                    return m_Creator.CachedProperties.Get<System.Drawing.Color?>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    SetColor(Model, value);
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public IXDisplayState DisplayState { get; }

        private System.Drawing.Color? GetColor(IModelDoc2 model) => SwColorHelper.GetColor(null,
                (o, c) => model.Extension.GetMaterialPropertyValues((int)o, c) as double[]);

        private void SetColor(IModelDoc2 model, System.Drawing.Color? color) => SwColorHelper.SetColor(color, null,
                (m, o, c) => model.Extension.SetMaterialPropertyValues(m, (int)o, c),
                (o, c) => model.Extension.RemoveMaterialProperty((int)o, c));

        protected override IModelDoc2 CreateNewDocument()
        {
            var doc = base.CreateNewDocument();
            
            var userColor = Color;

            if (userColor.HasValue)
            {
                SetColor(doc, userColor);
            }

            return doc;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (m_ConfigurationsLazy.IsValueCreated)
                {
                    m_ConfigurationsLazy.Value.Dispose();
                }
            }
        }

        protected abstract SwConfigurationCollection CreateConfigurations();

        IXDocument3DSaveOperation IXDocument3D.PreCreateSaveAsOperation(string filePath)
        {
            var ext = System.IO.Path.GetExtension(filePath);

            switch (ext.ToLower())
            {
                case ".pdf":
                    return new SwDocument3DPdfSaveOperation(this, filePath);

                case ".step":
                case ".stp":
                    return new SwStepSaveOperation(this, filePath);

                case ".ifc":
                    return new SwIfcSaveOperation(this, filePath);

                default:
                    return new SwDocument3DSaveOperation(this, filePath);
            }
        }

        public TSelObject ConvertObject<TSelObject>(TSelObject obj)
            where TSelObject : ISwSelObject
            => (TSelObject)ConvertObjectBoxed(obj);

        private ISwSelObject ConvertObjectBoxed(object obj)
        {
            if (obj is SwSelObject)
            {
                var disp = (obj as SwSelObject).Dispatch;
                var corrDisp = Model.Extension.GetCorresponding(disp);

                if (corrDisp != null)
                {
                    return this.CreateObjectFromDispatch<ISwSelObject>(corrDisp);
                }
                else
                {
                    throw new Exception("Failed to convert the pointer of the object");
                }
            }
            else
            {
                throw new InvalidCastException("Object is not SOLIDWORKS object");
            }
        }

        public override IXSaveOperation PreCreateSaveAsOperation(string filePath) => ((IXDocument3D)this).PreCreateSaveAsOperation(filePath);

        protected override void GetInitialSheetOrConfiguration(out SwSheet sheet, out SwConfiguration conf)
        {
            sheet = null;
            conf = null;

            if (m_ConfigurationsLazy.IsValueCreated)
            {
                conf = (SwConfiguration)m_ConfigurationsLazy.Value.Active;
            }
        }

        protected override void SetInitialSheetOrConfiguration(SwSheet sheet, SwConfiguration conf, IModelDoc2 model)
        {
            if (conf != null)
            {
                var confSw = (IConfiguration)model.GetConfigurationByName(conf.Name);

                if (confSw != null)
                {
                    conf.SetFromExisting(confSw);
                }
                else
                {
                    throw new Exception("Initial configuration is not found");
                }
            }
        }
    }
}