//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections;
using System.Collections.Generic;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features;

namespace Xarial.XCad.SolidWorks.Sketch
{
    public class SwSketchEntityCollection : IXSketchEntityRepository
    {
        public int Count => m_Sketch.IsCreated ? 0 : m_Cache.Count;

        public IXSketchEntity this[string name] => throw new NotImplementedException();

        public bool TryGet(string name, out IXSketchEntity ent) => throw new NotImplementedException();

        private readonly SwSketchBase m_Sketch;

        private readonly List<IXSketchEntity> m_Cache;

        private readonly SwDocument m_Doc;
        private readonly ISketchManager m_SkMgr;

        public SwSketchEntityCollection(SwDocument doc, SwSketchBase sketch)
        {
            m_Doc = doc;
            m_Sketch = sketch;
            m_SkMgr = doc.Model.SketchManager;
            m_Cache = new List<IXSketchEntity>();
        }

        public void AddRange(IEnumerable<IXSketchEntity> segments)
        {
            if (m_Sketch.IsCreated)
            {
                CreateSegments(segments, m_Sketch.Sketch);
            }
            else
            {
                m_Cache.AddRange(segments);
            }
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

        public IEnumerator<IXSketchEntity> GetEnumerator()
        {
            if (m_Sketch.IsCreated)
            {
                throw new NotImplementedException();
            }
            else
            {
                return m_Cache.GetEnumerator();
            }
        }

        public IXLine PreCreateLine()
        {
            return new SwSketchLine(m_Doc, null, false);
        }

        public IXPoint PreCreatePoint()
        {
            return new SwSketchPoint(m_Doc, null, false);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void RemoveRange(IEnumerable<IXSketchEntity> ents)
        {
            //TODO: implement removing of entities
        }

        public IXArc PreCreateArc()
        {
            throw new NotImplementedException();
        }
    }
}