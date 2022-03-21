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
        public int Count => m_Sketch.IsCommitted 
            ? ((object[])m_Sketch.Sketch.GetSketchSegments() ?? new object[0]).Length + m_Sketch.Sketch.GetSketchPointsCount2() + m_Sketch.Sketch.GetSketchBlockInstanceCount()
            : m_Cache.Count;

        public IXSketchEntity this[string name] => throw new NotImplementedException();

        public bool TryGet(string name, out IXSketchEntity ent) => throw new NotImplementedException();

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

        public void AddRange(IEnumerable<IXSketchEntity> segments)
        {
            if (m_Sketch.IsCommitted)
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
            if (m_Sketch.IsCommitted)
            {
                return IterateEntities().GetEnumerator();
            }
            else
            {
                return m_Cache.GetEnumerator();
            }
        }

        public IXLine PreCreateLine() => new SwSketchLine(null, m_Doc, m_App, false);
        public IXPoint PreCreatePoint() => new SwSketchPoint(null, m_Doc, m_App, false);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void RemoveRange(IEnumerable<IXSketchEntity> ents)
        {
            //TODO: implement removing of entities
        }

        public IXCircle PreCreateCircle() => new SwSketchCircle(null, m_Doc, m_App, false);

        public IXPolylineCurve PreCreatePolyline()
            => throw new NotSupportedException();

        public IXCurve Merge(IXCurve[] curves)
            => throw new NotSupportedException();

        public IXArc PreCreateArc() => new SwSketchArc(null, m_Doc, m_App, false);

        private IEnumerable<ISwSketchEntity> IterateEntities() 
        {
            foreach (ISketchSegment seg in (object[])m_Sketch.Sketch.GetSketchSegments() ?? new object[0])
            {
                yield return m_Doc.CreateObjectFromDispatch<ISwSketchSegment>(seg);
            }

            foreach (ISketchPoint pt in (object[])m_Sketch.Sketch.GetSketchPoints2() ?? new object[0])
            {
                yield return m_Doc.CreateObjectFromDispatch<ISwSketchPoint>(pt);
            }

            foreach (ISketchBlockInstance blockInst in (object[])m_Sketch.Sketch.GetSketchBlockInstances() ?? new object[0])
            {
                yield return m_Doc.CreateObjectFromDispatch<ISwSketchBlockInstance>(blockInst);
            }
        }
    }
}