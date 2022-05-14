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
using System.Text;
using System.Threading;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.SolidWorks.Geometry.Primitives;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public interface ISwMemorySheetGeometryBuilder : IXSheetGeometryBuilder
    {
    }

    internal class SwMemorySheetGeometryBuilder : ISwMemorySheetGeometryBuilder
    {
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool TryGet(string name, out IXPrimitive ent) => throw new NotImplementedException();
        public void AddRange(IEnumerable<IXPrimitive> ents, CancellationToken cancellationToken) => throw new NotImplementedException();
        public void RemoveRange(IEnumerable<IXPrimitive> ents, CancellationToken cancellationToken) => throw new NotImplementedException();

        public IEnumerator<IXPrimitive> GetEnumerator() => throw new NotImplementedException();

        public int Count => throw new NotImplementedException();
        public IXPrimitive this[string name] => throw new NotImplementedException();

        private readonly ISwApplication m_App;

        protected readonly IModeler m_Modeler;
        protected readonly IMathUtility m_MathUtils;

        internal SwMemorySheetGeometryBuilder(ISwApplication app)
        {
            m_App = app;

            m_MathUtils = m_App.Sw.IGetMathUtility();
            m_Modeler = m_App.Sw.IGetModeler();
        }

        public T PreCreate<T>() where T : IXPrimitive
        {
            ISwTempPrimitive prim;

            if (typeof(IXPlanarSheet).IsAssignableFrom(typeof(T)))
            {
                prim = new SwTempPlanarSheet(null, m_App, false);
            }
            else if (typeof(IXKnit).IsAssignableFrom(typeof(T)))
            {
                prim = new SwTempSurfaceKnit(null, m_App, false);
            }
            else
            {
                throw new NotSupportedException();
            }

            return (T)prim;
        }
    }
}
