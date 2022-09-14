//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
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
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwDrawingViewsCollection : IXDrawingViewRepository 
    {
    }

    internal class SwDrawingViewsCollection : ISwDrawingViewsCollection
    {
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private readonly SwDrawing m_Draw;
        private readonly SwSheet m_Sheet;

        private readonly EntityCache<IXDrawingView> m_Cache;

        public event DrawingViewCreatedDelegate ViewCreated
        {
            add => m_ViewCreatedEventsHandler.Attach(value);
            remove => m_ViewCreatedEventsHandler.Detach(value);
        }

        private readonly DrawingViewCreatedEventsHandler m_ViewCreatedEventsHandler;

        internal SwDrawingViewsCollection(SwDrawing draw, SwSheet sheet)
        {
            m_Draw = draw;
            m_Sheet = sheet;

            m_ViewCreatedEventsHandler = new DrawingViewCreatedEventsHandler(m_Sheet, m_Draw, m_Draw.OwnerApplication);

            m_Cache = new EntityCache<IXDrawingView>(sheet, this, v => v.Name);
        }

        public IXDrawingView this[string name] => RepositoryHelper.Get(this, name);

        public int Count
        {
            get
            {
                if (m_Sheet.IsCommitted)
                {
                    return GetSwViews().Count();
                }
                else 
                {
                    return m_Cache.Count;
                }
            }
        }

        public void AddRange(IEnumerable<IXDrawingView> ents, CancellationToken cancellationToken) 
        {
            if (m_Sheet.IsCommitted)
            {
                RepositoryHelper.AddRange(ents, cancellationToken);
            }
            else 
            {
                m_Cache.AddRange(ents, cancellationToken);
            }
        }

        public IEnumerator<IXDrawingView> GetEnumerator()
        {
            if (m_Sheet.IsCommitted)
            {
                return GetDrawingViews().GetEnumerator();
            }
            else 
            {
                return m_Cache.GetEnumerator();
            }
        }

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) => RepositoryHelper.FilterDefault(this, filters, reverseOrder);

        public void RemoveRange(IEnumerable<IXDrawingView> ents, CancellationToken cancellationToken)
        {
            if (m_Sheet.IsCommitted)
            {
                m_Draw.Selections.AddRange(ents);

                if (!m_Draw.Model.Extension.DeleteSelection2((int)swDeleteSelectionOptions_e.swDelete_Absorbed)) 
                {
                    throw new Exception("Failed to delete views");
                }
            }
            else 
            {
                m_Cache.RemoveRange(ents, cancellationToken);
            }
        }

        public bool TryGet(string name, out IXDrawingView ent)
        {
            if (m_Sheet.IsCommitted)
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
            else 
            {
                return m_Cache.TryGet(name, out ent);
            }
        }

        internal void CommitCache(CancellationToken cancellationToken) => m_Cache.Commit(cancellationToken);

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
                    if (string.Equals(view.Name, m_Sheet.Name, StringComparison.CurrentCultureIgnoreCase))
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
                () => new SwModelBasedDrawingView(m_Draw, m_Sheet),
                () => new SwProjectedDrawingView(m_Draw, m_Sheet),
                () => new SwAuxiliaryDrawingView(m_Draw, m_Sheet),
                () => new SwDetailDrawingView(m_Draw, m_Sheet),
                () => new SwSectionDrawingView(m_Draw, m_Sheet),
                () => new SwFlatPatternDrawingView(m_Draw, m_Sheet),
                () => new SwRelativeView(m_Draw, m_Sheet));
    }
}
