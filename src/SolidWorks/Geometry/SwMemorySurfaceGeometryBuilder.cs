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
using System.Text;
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.SolidWorks.Geometry.Primitives;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.Toolkit.Utils;

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

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) 
            => m_RepoHelper.FilterDefault(this, filters, reverseOrder);

        public int Count => throw new NotImplementedException();
        public IXPrimitive this[string name] => throw new NotImplementedException();

        private readonly SwApplication m_App;

        protected readonly IModeler m_Modeler;
        protected readonly IMathUtility m_MathUtils;

        private readonly IMemoryGeometryBuilderToleranceProvider m_TolProvider;

        private readonly RepositoryHelper<IXPrimitive> m_RepoHelper;

        internal SwMemorySheetGeometryBuilder(SwApplication app, IMemoryGeometryBuilderToleranceProvider tolProvider)
        {
            m_App = app;

            m_TolProvider = tolProvider;

            m_RepoHelper = new RepositoryHelper<IXPrimitive>(this,
                TransactionFactory<IXPrimitive>.Create(() => new SwTempPlanarSheet(null, m_App, false, m_TolProvider)),
                TransactionFactory<IXPrimitive>.Create(() => new SwTempSurfaceKnit(null, m_App, false, m_TolProvider)),
                TransactionFactory<IXPrimitive>.Create(() => new SwTempSurfaceExtrusion(null, m_App, false)),
                TransactionFactory<IXPrimitive>.Create(() => new SwTempSurfaceLoft(null, m_App, false)));

            m_MathUtils = m_App.Sw.IGetMathUtility();
            m_Modeler = m_App.Sw.IGetModeler();
        }

        public T PreCreate<T>() where T : IXPrimitive
            => m_RepoHelper.PreCreate<T>();
    }
}
