//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry.Primitives;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Geometry
{
    /// <summary>
    /// SOLIDWORKS specific memory geometry builder for solid bodies
    /// </summary>
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

        private readonly IMemoryGeometryBuilderToleranceProvider m_TolProvider;

        private readonly RepositoryHelper<IXPrimitive> m_RepoHelper;

        internal SwMemorySolidGeometryBuilder(ISwApplication app, IMemoryGeometryBuilderDocumentProvider geomBuilderDocsProvider, IMemoryGeometryBuilderToleranceProvider tolProvider)
        {
            m_App = app;

            m_TolProvider = tolProvider;

            m_RepoHelper = new RepositoryHelper<IXPrimitive>(this,
                () => new SwTempSolidExtrusion(null, m_App, false),
                () => new SwTempSolidRevolve(null, m_App, false),
                () => new SwTempSolidSweep(null, (SwPart)m_GeomBuilderDocsProvider.ProvideDocument(typeof(SwTempSolidSweep)), m_App, false),
                () => new SwTempSolidKnit(null, m_App, false, m_TolProvider));

            m_MathUtils = m_App.Sw.IGetMathUtility();
            m_Modeler = m_App.Sw.IGetModeler();

            m_GeomBuilderDocsProvider = geomBuilderDocsProvider;
        }

        public bool TryGet(string name, out IXPrimitive ent) => throw new NotImplementedException();

        public void AddRange(IEnumerable<IXPrimitive> ents, CancellationToken cancellationToken) 
            => m_RepoHelper.AddRange(ents, cancellationToken);

        public void RemoveRange(IEnumerable<IXPrimitive> ents, CancellationToken cancellationToken) => throw new NotImplementedException();

        public T PreCreate<T>() where T : IXPrimitive
            => m_RepoHelper.PreCreate<T>();

        public IEnumerator<IXPrimitive> GetEnumerator() => throw new NotImplementedException();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) 
            => m_RepoHelper.FilterDefault(this, filters, reverseOrder);
    }
}
