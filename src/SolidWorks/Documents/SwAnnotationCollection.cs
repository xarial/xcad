//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.SolidWorks.Annotations;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Documents
{
    /// <summary>
    /// SOLIDWORKS specific annotations collection
    /// </summary>
    public interface ISwAnnotationCollection : IXAnnotationRepository 
    {
    }

    internal abstract class SwAnnotationCollection : ISwAnnotationCollection
    {
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected readonly SwDocument m_Doc;

        private readonly RepositoryHelper<IXAnnotation> m_RepoHelper;

        protected SwAnnotationCollection(SwDocument doc) 
        {
            m_Doc = doc;

            m_RepoHelper = new RepositoryHelper<IXAnnotation>(this,
                () => SwNote.New(null, m_Doc, m_Doc.OwnerApplication),
                () => SwDrawingNote.New(null, (SwDrawing)m_Doc, m_Doc.OwnerApplication),
                () => new SwSectionLine(null, m_Doc, m_Doc.OwnerApplication),
                () => new SwBreakLine(null, m_Doc, m_Doc.OwnerApplication),
                () => new SwDetailCircle(null, m_Doc, m_Doc.OwnerApplication));
        }

        public IXAnnotation this[string name] => m_RepoHelper.Get(name);

        public virtual int Count => IterateAllAnnotations().Count();

        public void AddRange(IEnumerable<IXAnnotation> ents, CancellationToken cancellationToken)
            => m_RepoHelper.AddRange(ents, cancellationToken);

        public IEnumerator<IXAnnotation> GetEnumerator() => IterateAllAnnotations().GetEnumerator();

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters)
        {
            bool notes;
            bool dimensions;
            bool all;
            bool detailCircles;
            bool sectionLines;
            bool breakLines;

            if (filters?.Any() == true)
            {
                notes = false;
                dimensions = false;
                detailCircles = false;
                sectionLines = false;
                breakLines = false;
                all = false;

                foreach (var filter in filters)
                {
                    if (typeof(IXNote).IsAssignableFrom(filter.Type)) 
                    {
                        notes = true;
                    }
                    else if (typeof(IXDimension).IsAssignableFrom(filter.Type)) 
                    {
                        dimensions = true;
                    }
                    else if (typeof(IXDetailCircle).IsAssignableFrom(filter.Type))
                    {
                        detailCircles = true;
                    }
                    else if (typeof(IXSectionLine).IsAssignableFrom(filter.Type))
                    {
                        sectionLines = true;
                    }
                    else if (typeof(IXBreakLine).IsAssignableFrom(filter.Type))
                    {
                        breakLines = true;
                    }
                    else if (filter.Type == null || typeof(IXAnnotation).IsAssignableFrom(filter.Type))
                    {
                        all = true;
                        break;
                    }
                }
            }
            else
            {
                notes = true;
                dimensions = true;
                detailCircles = true;
                sectionLines = true;
                breakLines = true;
                all = true;
            }

            foreach (var ent in m_RepoHelper.FilterDefault(IterateAnnotations(notes, dimensions, detailCircles, sectionLines, breakLines, all), filters, reverseOrder))
            {
                yield return ent;
            }
        }

        private IEnumerable<ISwAnnotation> IterateAllAnnotations() => IterateAnnotations(true, true, true, true, true, true);

        protected abstract IEnumerable<ISwAnnotation> IterateAnnotations(bool notes, bool dimensions, bool detailCircles,
            bool sectionLines, bool breakLines, bool other);

        public T PreCreate<T>() where T : IXAnnotation
            => m_RepoHelper.PreCreate<T>();

        public void RemoveRange(IEnumerable<IXAnnotation> ents, CancellationToken cancellationToken)
        {
            using (var selGrp = new SelectionGroup(m_Doc, true))
            {
                selGrp.AddRange(ents.Cast<SwAnnotation>().Select(e => e.Annotation).ToArray());

                if (!m_Doc.Model.Extension.DeleteSelection2((int)swDeleteSelectionOptions_e.swDelete_Absorbed))
                {
                    throw new Exception("Failed to delete annotations");
                }
            }
        }

        public bool TryGet(string name, out IXAnnotation ent)
            => throw new NotSupportedException();
    }

    internal class SwDocument3DAnnotationCollection : SwAnnotationCollection
    {
        public SwDocument3DAnnotationCollection(SwDocument3D doc) : base(doc)
        {
        }

        public override int Count => m_Doc.Model.Extension.GetAnnotationCount();

        protected override IEnumerable<ISwAnnotation> IterateAnnotations(bool notes, bool dimensions,
            bool detailCircles, bool sectionLines, bool breakLines, bool all)
        {
            var ann = m_Doc.Model.IGetFirstAnnotation2();

            while (ann != null)
            {
                yield return m_Doc.CreateObjectFromDispatch<ISwAnnotation>(ann);

                ann = ann.IGetNext2();
            }
        }
    }

    internal class SwDrawingAnnotationCollection : SwAnnotationCollection
    {
        private readonly SwDrawing m_Drw;

        public SwDrawingAnnotationCollection(SwDrawing drw) : base(drw)
        {
            m_Drw = drw;
        }

        public override int Count => m_Drw.Sheets.Sum(s => s.DrawingViews.Sum(v => ((ISwDrawingView)v).DrawingView.GetAnnotationCount()));

        protected override IEnumerable<ISwAnnotation> IterateAnnotations(bool notes, bool dimensions, bool detailCircles,
            bool sectionLines, bool breakLines, bool all)
        {
            foreach (SwSheet sheet in m_Drw.Sheets)
            {   
                foreach (var ann in sheet.Annotations.Iterate(notes, dimensions, detailCircles, sectionLines, breakLines, all))
                {
                    yield return ann;
                }

                foreach (SwDrawingView view in sheet.DrawingViews)
                {
                    foreach (var ann in view.Annotations.Iterate(notes, dimensions, detailCircles, sectionLines, breakLines, all))
                    {
                        yield return ann;
                    }
                }
            }
        }
    }

    internal class SwDrawingViewAnnotationCollection : SwAnnotationCollection
    {
        protected virtual SwDrawingView DrawingView { get; }

        public SwDrawingViewAnnotationCollection(SwDrawingView drwView) : this(drwView.OwnerDocument)
        {
            DrawingView = drwView;
        }

        protected SwDrawingViewAnnotationCollection(SwDocument doc) : base(doc)
        {
        }

        public override int Count => DrawingView.DrawingView.GetAnnotationCount() 
            + DrawingView.DrawingView.GetDimensionCount4() 
            + DrawingView.DrawingView.GetDetailCircleCount2(out _)
            + DrawingView.DrawingView.GetSectionLineCount2(out _)
            + DrawingView.DrawingView.GetBreakLineCount2(out _);

        internal IEnumerable<ISwAnnotation> Iterate(bool notes, bool dimensions, bool detailCircles,
            bool sectionLines, bool breakLines, bool all)
            => IterateAnnotations(notes, dimensions, detailCircles, sectionLines, breakLines, all);

        protected override IEnumerable<ISwAnnotation> IterateAnnotations(bool notes, bool dimensions, bool detailCircles, 
            bool sectionLines, bool breakLines, bool all)
        {
            if (all)
            {
                var ann = DrawingView.DrawingView.IGetFirstAnnotation2();
                
                while (ann != null)
                {
                    yield return m_Doc.CreateObjectFromDispatch<ISwAnnotation>(ann);

                    ann = ann.IGetNext2();
                }

                foreach(var detCircle in IterateDetailCircles())
                {
                    yield return detCircle;
                }

                foreach (var sectLine in IterateSectionLines())
                {
                    yield return sectLine;
                }

                foreach (var breakLine in IterateBreakLines())
                {
                    yield return breakLine;
                }
            }
            else 
            {
                if (notes)
                {
                    var note = DrawingView.DrawingView.IGetFirstNote();

                    while (note != null)
                    {
                        yield return m_Doc.CreateObjectFromDispatch<ISwNote>(note);

                        note = note.IGetNext();
                    }
                }

                if (dimensions)
                {
                    var dispDim = DrawingView.DrawingView.GetFirstDisplayDimension5();

                    while (dispDim != null)
                    {
                        yield return m_Doc.CreateObjectFromDispatch<ISwDimension>(dispDim);

                        dispDim = dispDim.GetNext5();
                    }
                }

                if (detailCircles)
                {
                    foreach (var detCircle in IterateDetailCircles())
                    {
                        yield return detCircle;
                    }
                }

                if (sectionLines)
                {
                    foreach (var sectLine in IterateSectionLines())
                    {
                        yield return sectLine;
                    }
                }

                if (breakLines)
                {
                    foreach (var breakLine in IterateBreakLines())
                    {
                        yield return breakLine;
                    }
                }
            }
        }

        private IEnumerable<ISwDetailCircle> IterateDetailCircles()
        {
            var detailCircles = (object[])DrawingView.DrawingView.GetDetailCircles();

            if (detailCircles != null)
            {
                foreach (IDetailCircle detCircle in detailCircles)
                {
                    yield return m_Doc.CreateObjectFromDispatch<ISwDetailCircle>(detCircle);
                }
            }
        }

        private IEnumerable<ISwSectionLine> IterateSectionLines()
        {
            var sectionLines = (object[])DrawingView.DrawingView.GetSectionLines();

            if (sectionLines != null)
            {
                foreach (IDrSection sectionLine in sectionLines)
                {
                    yield return m_Doc.CreateObjectFromDispatch<ISwSectionLine>(sectionLine);
                }
            }
        }

        private IEnumerable<ISwBreakLine> IterateBreakLines()
        {
            var breakLines = (object[])DrawingView.DrawingView.GetBreakLines();

            if (breakLines != null)
            {
                foreach (IBreakLine breakLine in breakLines)
                {
                    yield return m_Doc.CreateObjectFromDispatch<ISwBreakLine>(breakLine);
                }
            }
        }
    }

    internal class SwSheetAnnotationCollection : SwDrawingViewAnnotationCollection
    {
        private readonly SwSheet m_Sheet;

        protected override SwDrawingView DrawingView => m_Sheet.SheetView;

        public SwSheetAnnotationCollection(SwSheet sheet) : base(sheet.OwnerDocument)
        {
            m_Sheet = sheet;
        }
    }
}
