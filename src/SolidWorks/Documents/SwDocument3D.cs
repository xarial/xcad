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
    public abstract class SwDocument3D : SwDocument, IXDocument3D
    {
        private readonly IMathUtility m_MathUtils;

        IXView IXDocument3D.ActiveView => ActiveView;
        IXConfigurationRepository IXDocument3D.Configurations => Configurations;

        internal SwDocument3D(IModelDoc2 model, SwApplication app, IXLogger logger) : base(model, app, logger)
        {
            m_MathUtils = app.Sw.IGetMathUtility();
            Configurations = new SwConfigurationCollection(app.Sw, this);
        }

        public SwModelView ActiveView => new SwModelView(Model, Model.IActiveView, m_MathUtils);
        public SwConfigurationCollection Configurations { get; }

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
            where TSelObject : SwSelObject
        {
            return (TSelObject)ConvertObjectBoxed(obj);
        }

        private SwSelObject ConvertObjectBoxed(object obj)
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