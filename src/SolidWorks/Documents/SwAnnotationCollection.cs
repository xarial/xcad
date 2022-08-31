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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.XCad.Annotations;
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

        public abstract int Count { get; }
                public void AddRange(IEnumerable<IXAnnotation> ents, CancellationToken cancellationToken)
            => RepositoryHelper.AddRange(ents, cancellationToken);

        public abstract IEnumerator<IXAnnotation> GetEnumerator();

        public T PreCreate<T>() where T : IXAnnotation
            => RepositoryHelper.PreCreate<IXAnnotation, T>(this,
                () => new SwNote(null, m_Doc, m_Doc.OwnerApplication));

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

        public override IEnumerator<IXAnnotation> GetEnumerator()
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

        public override IEnumerator<IXAnnotation> GetEnumerator()
        {
            foreach (var sheet in m_Drw.Sheets)
            {
                foreach (var view in sheet.DrawingViews)
                {
                    foreach (var ann in view.Annotations) 
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

        public override int Count => m_DrwView.DrawingView.GetAnnotationCount();

        public override IEnumerator<IXAnnotation> GetEnumerator()
        {
            var ann = m_DrwView.DrawingView.IGetFirstAnnotation2();

            while (ann != null)
            {
                yield return m_Doc.CreateObjectFromDispatch<ISwAnnotation>(ann);

                ann = ann.IGetNext2();
            }
        }
    }
}
