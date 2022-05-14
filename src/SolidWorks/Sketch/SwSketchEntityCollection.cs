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
using Xarial.XCad.Base;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Curves;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.Geometry.Curves;

namespace Xarial.XCad.SolidWorks.Sketch
{
    public interface ISwSketchEntityCollection : IXSketchEntityRepository
    {
    }

    internal class SwSketchEntityCollection : ISwSketchEntityCollection
    {
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => m_Sketch.IsCommitted 
            ? ((object[])m_Sketch.Sketch.GetSketchSegments() ?? new object[0]).Length + m_Sketch.Sketch.GetSketchPointsCount2() + m_Sketch.Sketch.GetSketchBlockInstanceCount()
            : m_Cache.Count;

        public IXWireEntity this[string name] => this.Get(name);

        private readonly ISwSketchBase m_Sketch;

        private readonly List<IXSketchEntity> m_Cache;

        private readonly ISwApplication m_App;
        private readonly ISwDocument m_Doc;
        private readonly ISketchManager m_SkMgr;

        internal SwSketchEntityCollection(ISwSketchBase sketch, ISwDocument doc, ISwApplication app)
        {
            m_Doc = doc;
            m_App = app;
            m_Sketch = sketch;
            m_SkMgr = doc.Model.SketchManager;
            m_Cache = new List<IXSketchEntity>();
        }

        internal void CommitCache(ISketch sketch)
        {
            CreateSegments(m_Cache, sketch);

            m_Cache.Clear();
        }

        private void CreateSegments(IEnumerable<IXSketchEntity> segments, ISketch sketch)
        {
            var addToDbOrig = m_SkMgr.AddToDB;

            m_Sketch.SetEditMode(sketch, true);

            m_SkMgr.AddToDB = true;

            foreach (SwSketchEntity seg in segments)
            {
                seg.Commit();
            }

            m_SkMgr.AddToDB = addToDbOrig;

            m_Sketch.SetEditMode(sketch, false);
        }

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

        public void AddRange(IEnumerable<IXWireEntity> ents)
        {
            if (m_Sketch.IsCommitted)
            {
                CreateSegments(ents.Cast<IXSketchEntity>(), m_Sketch.Sketch);
            }
            else
            {
                m_Cache.AddRange(ents.Cast<IXSketchEntity>());
            }
        }

        public void RemoveRange(IEnumerable<IXWireEntity> ents)
            => throw new NotImplementedException();

        public T PreCreate<T>() where T : IXWireEntity
        {
            ISwSketchEntity skEnt;

            if (typeof(IXLine).IsAssignableFrom(typeof(T)))
            {
                skEnt = new SwSketchLine(null, m_Doc, m_App, false);
            }
            else if (typeof(IXPoint).IsAssignableFrom(typeof(T)))
            {
                skEnt = new SwSketchPoint(null, m_Doc, m_App, false);
            }
            else if (typeof(IXCircle).IsAssignableFrom(typeof(T)))
            {
                skEnt = new SwSketchCircle(null, m_Doc, m_App, false);
            }
            else
            {
                throw new NotSupportedException("Sketch entity is not supported");
            }

            return (T)skEnt;
        }

        public IEnumerator<IXWireEntity> GetEnumerator() => IterateEntities().GetEnumerator();
    }
}