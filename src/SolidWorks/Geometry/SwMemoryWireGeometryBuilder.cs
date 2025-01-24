//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
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
using Xarial.XCad.Geometry.Curves;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Geometry
{
    /// <summary>
    /// SOLIDWORKS specific memory geometry builder of wire bodies
    /// </summary>
    public interface ISwMemoryWireGeometryBuilder : IXWireGeometryBuilder
    {
        /// <summary>
        /// Merge curves
        /// </summary>
        /// <param name="curves">Curves to merge</param>
        /// <returns>Merged curve</returns>
        ISwCurve Merge(ISwCurve[] curves);
    }

    internal class SwMemoryWireGeometryBuilder : ISwMemoryWireGeometryBuilder
    {
        IXCurve IXWireGeometryBuilder.Merge(IXCurve[] curves) => Merge(curves.Cast<ISwCurve>().ToArray());

        private readonly SwApplication m_App;
        protected readonly IModeler m_Modeler;
        protected readonly IMathUtility m_MathUtils;

        public int Count => throw new NotImplementedException();

        public IXWireEntity this[string name] => throw new NotImplementedException();

        private readonly RepositoryHelper<IXWireEntity> m_RepoHelper;

        internal SwMemoryWireGeometryBuilder(SwApplication app)
        {
            m_App = app;

            m_RepoHelper = new RepositoryHelper<IXWireEntity>(this,
                TransactionFactory<IXWireEntity>.Create(() => new SwTempWireBody(null, m_App)),
                TransactionFactory<IXWireEntity>.Create(() => new SwCircleCurve(null, null, m_App, false)),
                TransactionFactory<IXWireEntity>.Create(() => new SwArcCurve(null, null, m_App, false)),
                TransactionFactory<IXWireEntity>.Create(() => new SwLineCurve(null, null, m_App, false)),
                TransactionFactory<IXWireEntity>.Create(() => new SwPolylineCurve(null, null, m_App, false)),
                TransactionFactory<IXWireEntity>.Create(() => new SwPoint(null, null, m_App)),
                TransactionFactory<IXWireEntity>.Create(() => new SwLoop(null, null, m_App)));

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

        public void AddRange(IEnumerable<IXWireEntity> ents, CancellationToken cancellationToken) 
            => m_RepoHelper.AddRange(ents, cancellationToken);

        public void RemoveRange(IEnumerable<IXWireEntity> ents, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public T PreCreate<T>() where T : IXWireEntity
            => m_RepoHelper.PreCreate<T>();

        public IEnumerator<IXWireEntity> GetEnumerator()
            => throw new NotImplementedException();

        IEnumerator IEnumerable.GetEnumerator()
            => throw new NotImplementedException();

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) 
            => m_RepoHelper.FilterDefault(this, filters, reverseOrder);
    }
}
