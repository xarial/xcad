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
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Toolkit.Utils;

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

        public IXDrawingView this[string name] => RepositoryHelper.Get(this, name);

        public int Count => GetSwViews().Count();

        public void AddRange(IEnumerable<IXDrawingView> ents, CancellationToken cancellationToken) => RepositoryHelper.AddRange(this, ents, cancellationToken);

        public IEnumerator<IXDrawingView> GetEnumerator() => GetDrawingViews().GetEnumerator();

        public void RemoveRange(IEnumerable<IXDrawingView> ents, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public bool TryGet(string name, out IXDrawingView ent)
        {
            var view = GetSwViews().FirstOrDefault(
                x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));

            if (view != null)
            {
                ent = m_Draw.CreateObjectFromDispatch<SwDrawingView>(view);
            }
            else 
            {
                ent = null;
            }

            return ent != null;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private IEnumerable<SwDrawingView> GetDrawingViews() 
        {
            foreach (IView view in GetSwViews())
            {
                yield return m_Draw.CreateObjectFromDispatch<SwDrawingView>(view);
            }
        }

        private IEnumerable<IView> GetSwViews() 
        {
            var isSheetFound = false;

            foreach (object[] sheet in m_Draw.Drawing.GetViews() as object[]) 
            {
                foreach (IView view in sheet) 
                {
                    if (string.Equals(view.Name, m_Sheet.GetName(), StringComparison.CurrentCultureIgnoreCase))
                    {
                        isSheetFound = true;
                    }
                    else if (isSheetFound) 
                    {
                        yield return view;
                    }
                    else
                    {
                        break;
                    }
                }

                if (isSheetFound) 
                {
                    break;
                }
            }
        }

        public T PreCreate<T>() where T : IXDrawingView
            => RepositoryHelper.PreCreate<IXDrawingView, T>(this,
                () => new SwModelBasedDrawingView(null, m_Draw, m_Sheet, false));
    }
}
