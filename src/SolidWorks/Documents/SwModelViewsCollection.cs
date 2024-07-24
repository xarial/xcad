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
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwModelViewsCollection : IXModelViewRepository
    {
        new ISwModelView Active { get; }
    }

    public interface ISwModelViews3DCollection : IXModelView3DRepository, ISwModelViewsCollection
    {
        new ISwStandardView this[StandardViewType_e type] { get; }
        new ISwNamedView this[string name] { get; }
    }

    internal class SwModelViewsCollection : ISwModelViewsCollection
    {
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        IXModelView IXRepository<IXModelView>.this[string name] => this[name];
        IXModelView IXModelViewRepository.Active => Active;

        protected readonly SwDocument m_Doc;
        protected readonly SwApplication m_App;

        private readonly RepositoryHelper<IXModelView> m_RepoHelper;

        public SwModelViewsCollection(SwDocument doc, SwApplication app)
        {
            m_Doc = doc;
            m_App = app;

            m_RepoHelper = new RepositoryHelper<IXModelView>(this);
        }

        public int Count => m_Doc.Model.GetModelViewCount() - 1;

        public ISwModelView Active
        {
            get
            {
                var activeView = m_Doc.Model.IActiveView;

                if (activeView != null)
                {
                    return m_Doc.CreateObjectFromDispatch<ISwModelView>(activeView);
                }
                else
                {
                    return null;
                }
            }
        }

        public ISwNamedView this[string name] => (ISwNamedView)m_RepoHelper.Get(name);

        public bool TryGet(string name, out IXModelView ent)
        {
            var view = IterateModelViews().OfType<SwNamedView>()
                .FirstOrDefault(v => string.Equals(name, v.Name, StringComparison.CurrentCultureIgnoreCase));

            ent = view;
            return view != null;
        }

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) 
            => m_RepoHelper.FilterDefault(this, filters, reverseOrder);

        public void AddRange(IEnumerable<IXModelView> ents, CancellationToken cancellationToken) => throw new NotImplementedException();

        public IEnumerator<IXModelView> GetEnumerator() => IterateModelViews().GetEnumerator();

        public void RemoveRange(IEnumerable<IXModelView> ents, CancellationToken cancellationToken) => throw new NotImplementedException();
        
        public T PreCreate<T>() where T : IXModelView => throw new NotImplementedException();

        private IEnumerable<SwModelView> IterateModelViews() 
        {
            var viewNames = (string[])m_Doc.Model.GetModelViewNames();

            for (int i = 1; i < viewNames.Length; i++)
            {
                var viewName = viewNames[i];

                switch (i)
                {
                    case 0:
                        //Normal To
                        break;

                    case 1:
                        yield return new SwStandardView(null, m_Doc, m_App, StandardViewType_e.Front, viewName);
                        break;

                    case 2:
                        yield return new SwStandardView(null, m_Doc, m_App, StandardViewType_e.Back, viewName);
                        break;

                    case 3:
                        yield return new SwStandardView(null, m_Doc, m_App, StandardViewType_e.Left, viewName);
                        break;

                    case 4:
                        yield return new SwStandardView(null, m_Doc, m_App, StandardViewType_e.Right, viewName);
                        break;

                    case 5:
                        yield return new SwStandardView(null, m_Doc, m_App, StandardViewType_e.Top, viewName);
                        break;

                    case 6:
                        yield return new SwStandardView(null, m_Doc, m_App, StandardViewType_e.Bottom, viewName);
                        break;

                    case 7:
                        yield return new SwStandardView(null, m_Doc, m_App, StandardViewType_e.Isometric, viewName);
                        break;

                    case 8:
                        yield return new SwStandardView(null, m_Doc, m_App, StandardViewType_e.Trimetric, viewName);
                        break;

                    case 9:
                        yield return new SwStandardView(null, m_Doc, m_App, StandardViewType_e.Dimetric, viewName);
                        break;

                    default:
                        yield return new SwNamedView(null, m_Doc, m_App, viewName);
                        break;
                }
            }
        }
    }

    internal class SwModelViews3DCollection : SwModelViewsCollection, ISwModelViews3DCollection
    {
        IXStandardView IXModelView3DRepository.this[StandardViewType_e type] => this[type];

        public SwModelViews3DCollection(SwDocument3D doc, SwApplication app) : base(doc, app)
        {
        }

        public ISwStandardView this[StandardViewType_e type]
            => new SwStandardView(null, m_Doc, m_App, type); //TODO: move the view creation to SwObject.FromDispatch
    }
}
