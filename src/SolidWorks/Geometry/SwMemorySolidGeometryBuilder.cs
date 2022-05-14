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
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry.Primitives;
using Xarial.XCad.SolidWorks.Services;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public interface ISwMemorySolidGeometryBuilder : IXSolidGeometryBuilder
    {
    }

    internal class SwMemorySolidGeometryBuilder : ISwMemorySolidGeometryBuilder
    {
        private readonly ISwApplication m_App;

        protected readonly IModeler m_Modeler;
        protected readonly IMathUtility m_MathUtils;

        private readonly IMemoryGeometryBuilderDocumentProvider m_GeomBuilderDocsProvider;

        public int Count => throw new NotImplementedException();
        public IXPrimitive this[string name] => throw new NotImplementedException();

        internal SwMemorySolidGeometryBuilder(ISwApplication app, IMemoryGeometryBuilderDocumentProvider geomBuilderDocsProvider)
        {
            m_App = app;

            m_MathUtils = m_App.Sw.IGetMathUtility();
            m_Modeler = m_App.Sw.IGetModeler();

            m_GeomBuilderDocsProvider = geomBuilderDocsProvider;
        }

        public bool TryGet(string name, out IXPrimitive ent) => throw new NotImplementedException();

        public void AddRange(IEnumerable<IXPrimitive> ents)
        {
            foreach (var ent in ents) 
            {
                ent.Commit();
            }
        }

        public void RemoveRange(IEnumerable<IXPrimitive> ents) => throw new NotImplementedException();

        public T PreCreate<T>() where T : IXPrimitive
        {
            ISwTempPrimitive prim;

            if (typeof(IXExtrusion).IsAssignableFrom(typeof(T)))
            {
                prim = new SwTempExtrusion(null, m_App, false);
            }
            else if (typeof(IXRevolve).IsAssignableFrom(typeof(T)))
            {
                prim = new SwTempRevolve(null, m_App, false);
            }
            else if (typeof(IXSweep).IsAssignableFrom(typeof(T)))
            {
                prim = new SwTempSweep(null, (SwPart)m_GeomBuilderDocsProvider.ProvideDocument(typeof(SwTempSweep)), m_App, false);
            }
            else if (typeof(IXKnit).IsAssignableFrom(typeof(T)))
            {
                prim = new SwTempSolidKnit(null, m_App, false);
            }
            else 
            {
                throw new NotSupportedException("This entity is not supported");
            }

            return (T)prim;
        }

        public IEnumerator<IXPrimitive> GetEnumerator() => throw new NotImplementedException();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
