﻿//*********************************************************************
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
using Xarial.XCad.Geometry.Curves;
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

        private readonly ISwDocument m_Doc;
        private readonly ISketchManager m_SkMgr;

        internal SwSketchEntityCollection(ISwDocument doc, SwSketchBase sketch)
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
                return new SwSketchEntitiesEnumerator(m_Doc, m_Sketch);
            }
            else
            {
                return m_Cache.GetEnumerator();
            }
        }

        public IXLine PreCreateLine() => new SwSketchLine(m_Doc, null, false);
        public IXPoint PreCreatePoint() => new SwSketchPoint(m_Doc, null, false);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void RemoveRange(IEnumerable<IXSketchEntity> ents)
        {
            //TODO: implement removing of entities
        }

        public IXArc PreCreateArc() => new SwSketchArc(m_Doc, null, false);

        public IXPolylineCurve PreCreatePolyline()
        {
            throw new NotSupportedException();
        }

        public IXComplexCurve PreCreateComplex()
        {
            throw new NotSupportedException();
        }
    }

    public class SwSketchEntitiesEnumerator : IEnumerator<SwSketchEntity>
    {
        public SwSketchEntity Current => SwObject.FromDispatch<SwSketchEntity>(m_Entities[m_CurIndex], m_Doc);

        object IEnumerator.Current => Current;

        private readonly ISwDocument m_Doc;
        private readonly SwSketchBase m_Sketch;

        private List<object> m_Entities;
        private int m_CurIndex;

        internal SwSketchEntitiesEnumerator(ISwDocument doc, SwSketchBase sketch) 
        {
            m_Doc = doc;
            m_Sketch = sketch;
            m_Entities = new List<object>();

            Reset();
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            m_CurIndex++;
            return m_CurIndex < m_Entities.Count;
        }

        public void Reset()
        {
            m_CurIndex = -1;
            
            m_Entities.Clear();

            var segs = m_Sketch.Sketch.GetSketchSegments() as object[];

            if (segs != null) 
            {
                m_Entities.AddRange(segs);
            }

            var pts = m_Sketch.Sketch.GetSketchPoints2() as object[];

            if (pts != null) 
            {
                m_Entities.AddRange(pts);
            }
        }
    }
}