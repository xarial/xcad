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
using System.Linq;
using System.Text;
using Xarial.XCad.Documents;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwDrawingViewsCollection : IXDrawingViewRepository 
    {
    }

    internal class SwDrawingViewsCollection : ISwDrawingViewsCollection
    {
        private readonly SwDrawing m_Draw;
        private readonly ISheet m_Sheet;

        internal SwDrawingViewsCollection(SwDrawing draw, ISheet sheet) 
        {
            m_Draw = draw;
            m_Sheet = sheet;
        }

        public IXDrawingView this[string name] 
        {
            get 
            {
                if (!TryGet(name, out IXDrawingView view)) 
                {
                    throw new Exception("Failed to find the view by name");
                }

                return view;
            }
        }

        public int Count => GetDrawingViews().Count();

        public void AddRange(IEnumerable<IXDrawingView> ents)
        {
            foreach (SwDrawingView view in ents) 
            {
                view.Commit();
            }
        }

        public IEnumerator<IXDrawingView> GetEnumerator() => GetDrawingViews().GetEnumerator();

        public void RemoveRange(IEnumerable<IXDrawingView> ents)
        {
            throw new NotImplementedException();
        }

        public bool TryGet(string name, out IXDrawingView ent)
        {
            ent = GetDrawingViews().FirstOrDefault(
                x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));

            return ent != null;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private IEnumerable<SwDrawingView> GetDrawingViews() 
        {
            var views = m_Sheet.GetViews() as object[];

            if (views != null)
            {
                foreach (IView view in views)
                {
                    yield return SwSelObject.FromDispatch<SwDrawingView>(view, m_Draw);
                }
            }
        }

        TDrawingView IXDrawingViewRepository.PreCreate<TDrawingView>()
        {
            if (typeof(TDrawingView).IsAssignableFrom(typeof(SwModelBasedDrawingView)))
            {
                return new SwModelBasedDrawingView(null, m_Draw, m_Sheet, false) as TDrawingView;
            }
            else 
            {
                throw new NotSupportedException("Precreation of this drawing view is not supported");
            }
        }
    }
}
