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
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Curves;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Sketch
{
    public interface ISwSketchEntityCollection : IXSketchEntityRepository
    {
    }

    internal class SwSketchEntityCollection : ISwSketchEntityCollection
    {
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count 
        {
            get 
            {
                if (m_Sketch.IsCommitted)
                {
                    return ((object[])m_Sketch.Sketch.GetSketchSegments() ?? new object[0]).Length 
                        + m_Sketch.Sketch.GetSketchPointsCount2() 
                        + m_Sketch.Sketch.GetSketchBlockInstanceCount();
                }
                else 
                {
                    return m_Cache.Count;
                }
            }
        }

        public IXWireEntity this[string name] => RepositoryHelper.Get(this, name);

        private readonly SwSketchBase m_Sketch;

        private readonly EntityCache<IXWireEntity> m_Cache;

        private readonly SwApplication m_App;
        private readonly SwDocument m_Doc;

        internal SwSketchEntityCollection(SwSketchBase sketch, SwDocument doc, SwApplication app)
        {
            m_Doc = doc;
            m_App = app;
            m_Sketch = sketch;
            m_Cache = new EntityCache<IXWireEntity>(sketch, this, s => ((IXSketchEntity)s).Name);
        }

        internal void CommitCache(CancellationToken cancellationToken) => m_Cache.Commit(cancellationToken);

        public IXCurve Merge(IXCurve[] curves)
            => throw new NotSupportedException();

        protected virtual IEnumerable<ISwSketchEntity> IterateEntities() 
        {
            foreach (ISketchSegment seg in (object[])m_Sketch.Sketch.GetSketchSegments() ?? new object[0])
            {
                yield return m_Doc.CreateObjectFromDispatch<SwSketchSegment>(seg);
            }

            foreach (ISketchPoint pt in (object[])m_Sketch.Sketch.GetSketchPoints2() ?? new object[0])
            {
                yield return m_Doc.CreateObjectFromDispatch<SwSketchPoint>(pt);
            }

            foreach (ISketchBlockInstance blockInst in (object[])m_Sketch.Sketch.GetSketchBlockInstances() ?? new object[0])
            {
                yield return m_Doc.CreateObjectFromDispatch<SwSketchBlockInstance>(blockInst);
            }
        }

        public bool TryGet(string name, out IXWireEntity ent)
        {
            if (m_Sketch.IsCommitted)
            {
                foreach (var curEnt in IterateEntities())
                {
                    if (string.Equals(curEnt.Name, name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        ent = curEnt;
                        return true;
                    }
                }

                ent = null;
                return false;
            }
            else 
            {
                return m_Cache.TryGet(name, out ent);
            }
        }

        public void AddRange(IEnumerable<IXWireEntity> ents, CancellationToken cancellationToken)
        {
            if (m_Sketch.IsCommitted)
            {
                using (var editor = m_Sketch.CreateSketchEditor(m_Sketch.Sketch))
                {
                    RepositoryHelper.AddRange(ents, cancellationToken);
                }
            }
            else
            {
                m_Cache.AddRange(ents, cancellationToken);
            }
        }

        public void RemoveRange(IEnumerable<IXWireEntity> ents, CancellationToken cancellationToken)
        {
            if (m_Sketch.IsCommitted)
            {
                throw new NotImplementedException();
            }
            else 
            {
                m_Cache.RemoveRange(ents, cancellationToken);
            }
        }

        public T PreCreate<T>() where T : IXWireEntity
            => RepositoryHelper.PreCreate<IXWireEntity, T>(this,
                () => new SwSketchLine(m_Sketch, m_Doc, m_App),
                () => new SwSketchPoint(m_Sketch, m_Doc, m_App),
                () => new SwSketchCircle(m_Sketch, m_Doc, m_App),
                () => new SwSketchArc(m_Sketch, m_Doc, m_App),
                () => new SwSketchEllipse(m_Sketch, m_Doc, m_App),
                () => new SwSketchSpline(m_Sketch, m_Doc, m_App),
                () => new SwSketchText(m_Sketch, m_Doc, m_App));

        public IEnumerator<IXWireEntity> GetEnumerator()
        {
            if (m_Sketch.IsCommitted)
            {
                return IterateEntities().GetEnumerator();
            }
            else 
            {
                return m_Cache.GetEnumerator();
            }
        }
    }
}