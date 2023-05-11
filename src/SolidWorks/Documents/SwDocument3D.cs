//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
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
            m_Configurations = new Lazy<ISwConfigurationCollection>(CreateConfigurations);
            m_ModelViewsLazy = new Lazy<ISwModelViews3DCollection>(() => new SwModelViews3DCollection(this, app));

            Graphics = new SwDocumentGraphics(this);
        }

        private Lazy<ISwConfigurationCollection> m_Configurations;
        private Lazy<ISwModelViews3DCollection> m_ModelViewsLazy;

        public ISwConfigurationCollection Configurations => m_Configurations.Value;

        public abstract IXDocumentEvaluation Evaluation { get; }

        public IXDocumentGraphics Graphics { get; }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (m_Configurations.IsValueCreated)
                {
                    m_Configurations.Value.Dispose();
                }
            }
        }

        protected abstract SwConfigurationCollection CreateConfigurations();

        protected override SwPdfSaveOperation CreatePdfSaveOperation(string filePath)
            => new SwDocument3DPdfSaveOperation(this, filePath);

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
    }
}