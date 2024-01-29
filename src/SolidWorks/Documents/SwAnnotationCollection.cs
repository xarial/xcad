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
using System.Threading.Tasks;
using System.Windows.Forms;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.SolidWorks.Annotations;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwAnnotationCollection : IXAnnotationRepository 
    {
    }

    internal abstract class SwAnnotationCollection : ISwAnnotationCollection
    {
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected readonly SwDocument m_Doc;

        protected SwAnnotationCollection(SwDocument doc) 
        {
            m_Doc = doc;
        }

        public IXAnnotation this[string name] => RepositoryHelper.Get(this, name);

        public virtual int Count => IterateAllAnnotations().Count();

        public void AddRange(IEnumerable<IXAnnotation> ents, CancellationToken cancellationToken)
            => RepositoryHelper.AddRange(ents, cancellationToken);

        public IEnumerator<IXAnnotation> GetEnumerator() => IterateAllAnnotations().GetEnumerator();

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters)
        {
            bool notes;
            bool dimensions;
            bool all;

            if (filters?.Any() == true)
            {
                notes = false;
                dimensions = false;
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
                all = true;
            }

            foreach (var ent in RepositoryHelper.FilterDefault(IterateAnnotations(notes, dimensions, all), filters, reverseOrder))
            {
                yield return ent;
            }
        }

        private IEnumerable<ISwAnnotation> IterateAllAnnotations() => IterateAnnotations(true, true, true);

        protected abstract IEnumerable<ISwAnnotation> IterateAnnotations(bool notes, bool dimensions, bool other);

        public T PreCreate<T>() where T : IXAnnotation
            => RepositoryHelper.PreCreate<IXAnnotation, T>(this,
                () => new SwNote(null, m_Doc, m_Doc.OwnerApplication),
                () => new SwSectionLine(null, m_Doc, m_Doc.OwnerApplication),
                () => new SwDetailCircle(null, m_Doc, m_Doc.OwnerApplication));

        public void RemoveRange(IEnumerable<IXAnnotation> ents, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public bool TryGet(string name, out IXAnnotation ent)
            => throw new NotSupportedException();
    }

    internal class SwDocument3DAnnotationCollection : SwAnnotationCollection
    {
        public SwDocument3DAnnotationCollection(SwDocument3D doc) : base(doc)
        {
        }

        public override int Count => m_Doc.Model.Extension.GetAnnotationCount();

        protected override IEnumerable<ISwAnnotation> IterateAnnotations(bool notes, bool dimensions, bool all)
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

        protected override IEnumerable<ISwAnnotation> IterateAnnotations(bool notes, bool dimensions, bool all)
        {
            foreach (var sheet in m_Drw.Sheets)
            {
                foreach (var view in sheet.DrawingViews)
                {
                    foreach (ISwAnnotation ann in view.Annotations)
                    {
                        yield return ann;
                    }
                }
            }
        }
    }

    internal class SwDrawingViewAnnotationCollection : SwAnnotationCollection
    {
        private readonly SwDrawingView m_DrwView;

        public SwDrawingViewAnnotationCollection(SwDrawingView drwView) : base(drwView.OwnerDocument)
        {
            m_DrwView = drwView;
        }

        public override int Count => m_DrwView.DrawingView.GetAnnotationCount() 
            + m_DrwView.DrawingView.GetDimensionCount4() 
            + m_DrwView.DrawingView.GetDetailCircleCount2(out _)
            + m_DrwView.DrawingView.GetSectionLineCount2(out _);

        protected override IEnumerable<ISwAnnotation> IterateAnnotations(bool notes, bool dimensions, bool all)
        {
            if (all)
            {
                var ann = m_DrwView.DrawingView.IGetFirstAnnotation2();
                
                while (ann != null)
                {
                    yield return m_Doc.CreateObjectFromDispatch<ISwAnnotation>(ann);

                    ann = ann.IGetNext2();
                }

                var detailCircles = (object[])m_DrwView.DrawingView.GetDetailCircles();

                if (detailCircles != null) 
                {
                    foreach(IDetailCircle detCircle in detailCircles) 
                    {
                        yield return m_Doc.CreateObjectFromDispatch<ISwDetailCircle>(detCircle);
                    }
                }

                var sectionLines = (object[])m_DrwView.DrawingView.GetSectionLines();

                if (sectionLines != null)
                {
                    foreach (IDrSection sectionLine in sectionLines)
                    {
                        yield return m_Doc.CreateObjectFromDispatch<ISwSectionLine>(sectionLine);
                    }
                }
            }
            else 
            {
                if (notes)
                {
                    var note = m_DrwView.DrawingView.IGetFirstNote();

                    while (note != null)
                    {
                        yield return m_Doc.CreateObjectFromDispatch<ISwNote>(note);

                        note = note.IGetNext();
                    }
                }

                if (dimensions)
                {
                    var dispDim = m_DrwView.DrawingView.GetFirstDisplayDimension5();

                    while (dispDim != null)
                    {
                        yield return m_Doc.CreateObjectFromDispatch<ISwDimension>(dispDim);

                        dispDim = dispDim.GetNext5();
                    }
                }
            }
        }
    }
}
