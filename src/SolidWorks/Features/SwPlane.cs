//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
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
    }

    internal class SwPlane : SwFeature, ISwPlane
    {
        private readonly IRefPlane m_RefPlane;

        private readonly IMathUtility m_MathUtils;

        internal SwPlane(IFeature feat, ISwDocument doc, ISwApplication app, bool created) : base(feat, doc, app, created)
        {
            m_RefPlane = feat.GetSpecificFeature2() as IRefPlane;

            m_MathUtils = Application.Sw.IGetMathUtility();
        }

        public Plane Definition 
        {
            get 
            {
                var x = (IMathVector)m_MathUtils.CreateVector(new double[] { 1, 0, 0 });
                var z = (IMathVector)m_MathUtils.CreateVector(new double[] { 0, 0, 1 });
                var origin = (IMathPoint)m_MathUtils.CreatePoint(new double[] { 0, 0, 0 });

                var transform = m_RefPlane.Transform;

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
