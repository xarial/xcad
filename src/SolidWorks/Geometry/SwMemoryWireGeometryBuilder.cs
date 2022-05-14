//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Curves;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.SolidWorks.Geometry.Curves;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public interface ISwMemoryWireGeometryBuilder : IXWireGeometryBuilder
    {
        ISwCurve Merge(ISwCurve[] curves);
    }

    internal class SwMemoryWireGeometryBuilder : ISwMemoryWireGeometryBuilder
    {
        IXCurve IXWireGeometryBuilder.Merge(IXCurve[] curves) => Merge(curves.Cast<ISwCurve>().ToArray());

        private readonly ISwApplication m_App;
        protected readonly IModeler m_Modeler;
        protected readonly IMathUtility m_MathUtils;

        public int Count => throw new NotImplementedException();

        public IXWireEntity this[string name] => throw new NotImplementedException();

        internal SwMemoryWireGeometryBuilder(ISwApplication app)
        {
            m_App = app;
            m_MathUtils = app.Sw.IGetMathUtility();
            m_Modeler = app.Sw.IGetModeler();
        }

        public ISwCurve Merge(ISwCurve[] curves)
        {
            var curve = m_Modeler.MergeCurves(curves.SelectMany(c => c.Curves).ToArray());

            if (curve == null) 
            {
                throw new NullReferenceException("Failed to merge input curves");
            }

            return m_App.CreateObjectFromDispatch<ISwCurve>(curve, null);
        }

        public bool TryGet(string name, out IXWireEntity ent)
            => throw new NotImplementedException();

        public void AddRange(IEnumerable<IXWireEntity> ents)
        {
            foreach (var ent in ents) 
            {
                ent.Commit();
            }
        }

        public void RemoveRange(IEnumerable<IXWireEntity> ents)
            => throw new NotImplementedException();

        public T PreCreate<T>() where T : IXWireEntity
        {
            IXWireEntity seg;

            if (typeof(IXArc).IsAssignableFrom(typeof(T)))
            {
                seg = new SwArcCurve(null, null, m_App, false);
            }
            else if (typeof(IXCircle).IsAssignableFrom(typeof(T)))
            {
                seg = new SwCircleCurve(null, null, m_App, false);
            }
            else if (typeof(IXLine).IsAssignableFrom(typeof(T)))
            {
                seg = new SwLineCurve(null, null, m_App, false);
            }
            else if (typeof(IXPolyline).IsAssignableFrom(typeof(T)))
            {
                seg = new SwPolylineCurve(null, null, m_App, false);
            }
            else if (typeof(IXPoint).IsAssignableFrom(typeof(T)))
            {
                seg = new SwPoint(null, null, m_App);
            }
            else 
            {
                throw new NotSupportedException("This segment type is not supported");
            }

            return (T)seg;
        }

        public IEnumerator<IXWireEntity> GetEnumerator()
            => throw new NotImplementedException();

        IEnumerator IEnumerable.GetEnumerator()
            => throw new NotImplementedException();
    }
}
