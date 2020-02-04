//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System.Drawing;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Documents
{
    public class SwModelView : IXView
    {
        private readonly IMathUtility m_MathUtils;

        private readonly IModelDoc2 m_Model;

        public Rectangle ScreenRect
        {
            get
            {
                var box = View.GetVisibleBox() as int[];

                //TODO: potential issue if feature manager is not docked on left
                var featMgrWidth = m_Model.GetFeatureManagerWidth();

                return new Rectangle(box[0] + featMgrWidth, box[1], box[2] - box[0] - featMgrWidth, box[3] - box[1]);
            }
        }

        public TransformMatrix ScreenTransform => TransformUtils.ToTransformMatrix(View.Transform);

        public TransformMatrix Transform
        {
            get
            {
                var origOr = View.Orientation3;
                var origScale = View.Scale2;
                var origTrans = View.Translation3;

                var vec = origTrans.ArrayData as double[];

                var data = origOr.ArrayData as double[];

                data[9] = vec[0];
                data[10] = vec[1];
                data[11] = vec[2];

                data[12] = origScale;

                return TransformUtils.ToTransformMatrix(data);
            }
            set
            {
                var matrix = m_MathUtils.ToMathTransform(value);

                object xVecObj = null;
                object yVecObj = null;
                object zVecObj = null;
                object transVecObj = null;
                double scale = 0;

                matrix.GetData2(ref xVecObj, ref yVecObj, ref zVecObj, ref transVecObj, ref scale);

                var data = matrix.ArrayData as double[];
                data[9] = 0;
                data[10] = 0;
                data[11] = 0;
                data[12] = 1;

                var orientation = m_MathUtils.CreateTransform(data) as MathTransform;

                View.Orientation3 = orientation;
                View.Scale2 = scale;
                View.Translation3 = (MathVector)transVecObj;
            }
        }

        public IModelView View { get; }

        internal SwModelView(IModelDoc2 model, IModelView view, IMathUtility mathUtils)
        {
            View = view;
            m_Model = model;
            m_MathUtils = mathUtils;
        }

        public void Freeze(bool freeze)
        {
            View.EnableGraphicsUpdate = !freeze;
        }

        public void Update()
        {
            View.GraphicsRedraw(null);
        }
    }
}