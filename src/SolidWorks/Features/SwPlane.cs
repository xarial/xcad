//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks.Features
{
    public interface ISwPlane : IXPlane, ISwFeature 
    {
        IRefPlane RefPlane { get; }
    }

    internal class SwPlane : SwFeature, ISwPlane
    {
        public IRefPlane RefPlane { get; }

        private readonly IMathUtility m_MathUtils;

        internal SwPlane(IFeature feat, SwDocument doc, SwApplication app, bool created) : base(feat, doc, app, created)
        {
            RefPlane = feat.GetSpecificFeature2() as IRefPlane;

            m_MathUtils = OwnerApplication.Sw.IGetMathUtility();
        }

        public override object Dispatch => RefPlane;

        public Plane Definition 
        {
            get 
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
        }
    }
}
