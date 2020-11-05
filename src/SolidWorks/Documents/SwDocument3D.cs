//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Utils.Diagnostics;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwDocument3D : ISwDocument, IXDocument3D
    {
        new ISwConfigurationCollection Configurations { get; }
        new ISwModelViewsCollection ModelViews { get; }
        new TSelObject ConvertObject<TSelObject>(TSelObject obj)
            where TSelObject : ISwSelObject;
    }

    internal abstract class SwDocument3D : SwDocument, ISwDocument3D
    {
        private readonly IMathUtility m_MathUtils;

        IXConfigurationRepository IXDocument3D.Configurations => Configurations;
        IXModelViewRepository IXDocument3D.ModelViews => ModelViews;

        internal SwDocument3D(IModelDoc2 model, ISwApplication app, IXLogger logger, bool isCreated) : base(model, app, logger, isCreated)
        {
            m_MathUtils = app.Sw.IGetMathUtility();
            Configurations = new SwConfigurationCollection(app.Sw, this);
            ModelViews = new SwModelViewsCollection(this, m_MathUtils);
        }
        
        public ISwConfigurationCollection Configurations { get; }

        public ISwModelViewsCollection ModelViews { get; }

        public abstract Box3D CalculateBoundingBox();

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing) 
            {
                Configurations.Dispose();
            }
        }

        TSelObject IXObjectContainer.ConvertObject<TSelObject>(TSelObject obj) => ConvertObjectBoxed(obj) as TSelObject;

        public TSelObject ConvertObject<TSelObject>(TSelObject obj)
            where TSelObject : ISwSelObject
        {
            return (TSelObject)ConvertObjectBoxed(obj);
        }

        private ISwSelObject ConvertObjectBoxed(object obj)
        {
            if (obj is SwSelObject)
            {
                var disp = (obj as SwSelObject).Dispatch;
                var corrDisp = Model.Extension.GetCorresponding(disp);

                if (corrDisp != null)
                {
                    return SwSelObject.FromDispatch(corrDisp, this);
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