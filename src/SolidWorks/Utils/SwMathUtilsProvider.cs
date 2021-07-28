using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry;

namespace Xarial.XCad.SolidWorks.Utils
{
    /// <summary>
    /// This utility class allows to create SOLIDWORKS specific math objects
    /// </summary>
    /// <remarks>Some entities (such as face or temp body) might not have direct access to IMathUtility
    /// This wrapper intended to use workarounds to create those objects where IMathUtility is not available</remarks>
    internal class SwMathUtilsProvider
    {
        private readonly IMathUtility m_MathUtils;
        private readonly IMathTransform m_TemplateTransform;
        private readonly IMathPoint m_TemplatePoint;
        private readonly IMathVector m_TemplateVector;

        internal SwMathUtilsProvider(SwEntity ent)
        {
            if (!TryGetMathUtility(ent, out m_MathUtils)) 
            {
                m_TemplateTransform = GetTemplateTransform(ent.Body, out m_TemplatePoint, out m_TemplateVector);
            }
        }

        internal SwMathUtilsProvider(SwBody ent)
        {
            if (!TryGetMathUtility(ent, out m_MathUtils))
            {
                m_TemplateTransform = GetTemplateTransform(ent, out m_TemplatePoint, out m_TemplateVector);
            }
        }

        internal IMathTransform CreateTransform(TransformMatrix matrix)
        {
            if (m_MathUtils != null)
            {
                return m_MathUtils.ToMathTransform(matrix);
            }
            else 
            {
                var transform = (IMathTransform)m_TemplateTransform.Multiply(m_TemplateTransform);
                transform.ArrayData = matrix.ToMathTransformData();
                return transform;
            }
        }

        internal IMathPoint CreatePoint(Point point)
        {
            if (m_MathUtils != null)
            {
                return (IMathPoint)m_MathUtils.CreatePoint(point.ToArray());
            }
            else
            {
                var mathPt = (IMathPoint)m_TemplatePoint.Scale(1);
                mathPt.ArrayData = point.ToArray();
                return mathPt;
            }
        }

        internal IMathVector CreateVector(Vector vector)
        {
            if (m_MathUtils != null)
            {
                return (IMathVector)m_MathUtils.CreateVector(vector.ToArray());
            }
            else
            {
                var mathVec = (IMathVector)m_TemplateVector.Scale(1);
                mathVec.ArrayData = vector.ToArray();
                return mathVec;
            }
        }

        private bool TryGetMathUtility(SwObject obj, out IMathUtility mathUtils) 
        {
            if (obj.Document is SwDocument)
            {
                mathUtils = ((SwDocument)obj.Document).App.Sw.IGetMathUtility();
                return true;
            }
            else 
            {
                mathUtils = null;
                return false;
            }
        }

        private IMathTransform GetTemplateTransform(ISwBody body, out IMathPoint point, out IMathVector vector) 
        {
            if (body.Body.GetCoincidenceTransform2(body.Body, out MathTransform transform))
            {
                object axis = null;
                object none = null;
                double scale = 0;
                
                transform.GetData2(ref axis, ref none, ref none, ref none, ref scale);
                
                vector = (IMathVector)axis;
                point = vector.IConvertToPoint();

                return transform;
            }
            else
            {
                throw new Exception("Failed to create template math transform");
            }
        }
    }
}
