//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks.Features
{
    public interface ISwPlane : IXPlane, ISwFeature 
    {
        /// <summary>
        /// Pointer to the referenced plane feature
        /// </summary>
        IRefPlane RefPlane { get; }
    }

    internal class SwPlane : SwFeature, ISwPlane
    {
        internal const string TypeName = "RefPlane";

        public IRefPlane RefPlane { get; private set; }

        private readonly IMathUtility m_MathUtils;

        internal SwPlane(IFeature feat, SwDocument doc, SwApplication app, bool created) : base(feat, doc, app, created)
        {
            if (feat != null)
            {
                RefPlane = feat.GetSpecificFeature2() as IRefPlane;
            }

            m_MathUtils = OwnerApplication.Sw.IGetMathUtility();
        }

        public override object Dispatch => RefPlane;

        public Plane Plane 
        {
            get 
            {
                if (IsCommitted)
                {
                    var x = (IMathVector)m_MathUtils.CreateVector(new double[] { 1, 0, 0 });
                    var z = (IMathVector)m_MathUtils.CreateVector(new double[] { 0, 0, 1 });
                    var origin = (IMathPoint)m_MathUtils.CreatePoint(new double[] { 0, 0, 0 });

                    var transform = RefPlane.Transform;

                    x = (IMathVector)x.MultiplyTransform(transform);
                    z = (IMathVector)z.MultiplyTransform(transform);
                    origin = (IMathPoint)origin.MultiplyTransform(transform);

                    return new Plane(new Point((double[])origin.ArrayData),
                        new Vector((double[])z.ArrayData),
                        new Vector((double[])x.ArrayData));
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<Plane>();
                }
            }
            set 
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else 
                {
                    throw new CommitedElementReadOnlyParameterException();
                }
            }
        }

        public override bool IsUserFeature 
        {
            get 
            {
                const int MAX_STANDARD_PLANES_COUNT = 3;

                var nextFeat = Feature;

                for (int i = 0; i < MAX_STANDARD_PLANES_COUNT; i++) 
                {
                    nextFeat = nextFeat.IGetNextFeature();

                    if (nextFeat == null) 
                    {
                        break;
                    }

                    var nextFeatTypeName = nextFeat.GetTypeName2();

                    if (nextFeatTypeName == SwOrigin.TypeName)//this feature is standard plane
                    {
                        return false;
                    }
                    else if (nextFeatTypeName != TypeName) //this feature is not a standard plane
                    {
                        break;
                    }
                }

                return true;
            }
        }

        public IXLoop OuterLoop { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IXLoop[] InnerLoops { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        protected override IFeature InsertFeature(CancellationToken cancellationToken)
        {
            if (Plane == null) 
            {
                throw new NullReferenceException("Plane is not specified");
            }

            var pt1 = Plane.Point;
            var pt2 = Plane.Point.Move(Plane.Direction, 0.1);
            var pt3 = Plane.Point.Move(Plane.Reference, 0.1);

            RefPlane = (IRefPlane)OwnerDocument.Model.CreatePlaneFixed2(pt1.ToArray(), pt2.ToArray(), pt3.ToArray(), false);

            var feat = (IFeature)RefPlane;

            return feat;
        }
    }
}
