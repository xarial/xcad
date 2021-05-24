//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Drawing;
using System.Threading;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Utils;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwModelView : IXModelView, ISwObject
    {
        IModelView View { get; }
    }

    internal class SwModelView : SwObject, ISwModelView
    {
        private readonly IMathUtility m_MathUtils;

        internal IModelDoc2 Owner { get; }

        public virtual Rectangle ScreenRect
        {
            get
            {
                var box = View.GetVisibleBox() as int[];

                return new Rectangle(box[0], box[1], box[2] - box[0], box[3] - box[1]);
            }
        }

        public virtual TransformMatrix ScreenTransform => TransformConverter.ToTransformMatrix(View.Transform);

        public virtual TransformMatrix Transform
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

                return TransformConverter.ToTransformMatrix(data);
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

        public virtual IModelView View { get; }

        //TODO: implement creation of new views
        public bool IsCommitted => true;

        public override object Dispatch => View;

        internal SwModelView(IModelDoc2 model, IModelView view, IMathUtility mathUtils) : base(view)
        {
            View = view;
            Owner = model;
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

        /// <inheritdoc/>
        public void ZoomToBox(Box3D box)
        {
            var transform = View.Orientation3;

            var mathPt1 = m_MathUtils.CreatePoint(box.GetLeftBottomBack().ToArray()) as IMathPoint;
            var mathPt2 = m_MathUtils.CreatePoint(box.GetRightTopFront().ToArray()) as IMathPoint;
            var pt1 = mathPt1.IMultiplyTransform(transform).ArrayData as double[];
            var pt2 = mathPt2.IMultiplyTransform(transform).ArrayData as double[];

            Owner.ViewZoomTo2(pt1[0], pt1[1], pt1[2], pt2[0], pt2[1], pt2[2]);
        }

        public void Commit(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public interface ISwNamedView : ISwModelView, IXNamedView
    {
    }

    internal class SwNamedView : SwModelView, ISwNamedView
    {
        //TODO: implement overrides for transforms

        public string Name { get; }

        internal SwNamedView(IModelDoc2 model, IModelView view, IMathUtility mathUtils, string name)
            : base(model, view, mathUtils)
        {
            Name = name;
        }
    }

    public interface ISwStandardView : ISwNamedView, IXStandardView 
    {
    }

    internal class SwStandardView : SwNamedView, ISwStandardView
    {
        //TODO: implement overrides for transforms

        public StandardViewType_e Type { get; }

        internal swStandardViews_e SwViewType { get; }

        private static string GetStandardViewName(IModelDoc2 model, StandardViewType_e swViewType) 
        {
            var viewNames = model.GetModelViewNames() as string[];

            switch (swViewType)
            {
                case StandardViewType_e.Front:
                    return viewNames[1];

                case StandardViewType_e.Back:
                    return viewNames[2];

                case StandardViewType_e.Left:
                    return viewNames[3];

                case StandardViewType_e.Right:
                    return viewNames[4];

                case StandardViewType_e.Top:
                    return viewNames[5];

                case StandardViewType_e.Bottom:
                    return viewNames[6];

                case StandardViewType_e.Isometric:
                    return viewNames[7];

                case StandardViewType_e.Trimetric:
                    return viewNames[8];

                case StandardViewType_e.Dimetric:
                    return viewNames[9];

                default:
                    throw new NotImplementedException($"{swViewType} is not supported");
            }
        }

        internal SwStandardView(IModelDoc2 model, IModelView view, IMathUtility mathUtils, StandardViewType_e type) 
            : base(model, view, mathUtils, GetStandardViewName(model, type))
        {
            Type = type;

            switch (Type) 
            {
                case StandardViewType_e.Back:
                    SwViewType = swStandardViews_e.swBackView;
                    break;

                case StandardViewType_e.Bottom:
                    SwViewType = swStandardViews_e.swBottomView;
                    break;

                case StandardViewType_e.Dimetric:
                    SwViewType = swStandardViews_e.swDimetricView;
                    break;

                case StandardViewType_e.Front:
                    SwViewType = swStandardViews_e.swFrontView;
                    break;

                case StandardViewType_e.Isometric:
                    SwViewType = swStandardViews_e.swIsometricView;
                    break;

                case StandardViewType_e.Left:
                    SwViewType = swStandardViews_e.swLeftView;
                    break;

                case StandardViewType_e.Right:
                    SwViewType = swStandardViews_e.swRightView;
                    break;

                case StandardViewType_e.Top:
                    SwViewType = swStandardViews_e.swTopView;
                    break;

                case StandardViewType_e.Trimetric:
                    SwViewType = swStandardViews_e.swTrimetricView;
                    break;
            }
        }
    }
}