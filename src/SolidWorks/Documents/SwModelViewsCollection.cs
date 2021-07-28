//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwModelViewsCollection : IXModelViewRepository 
    {
        new ISwModelView Active { get; }
        new ISwStandardView this[StandardViewType_e type] { get; }
        new ISwNamedView this[string name] { get; }
    }

    internal class SwModelViewsCollection : ISwModelViewsCollection
    {
        IXStandardView IXModelViewRepository.this[StandardViewType_e type] => this[type];
        IXModelView IXRepository<IXModelView>.this[string name] => this[name];
        IXModelView IXModelViewRepository.Active => Active;

        private readonly ISwDocument3D m_Doc;
        private readonly ISwApplication m_App;

        public SwModelViewsCollection(ISwDocument3D doc, ISwApplication app) 
        {
            m_Doc = doc;
            m_App = app;
        }

        public int Count => throw new NotImplementedException();

        //TODO: move the view creation to SwObject.FromDispatch
        public ISwModelView Active => new SwModelView(m_Doc.Model.IActiveView, m_Doc, m_App);
        
        public ISwNamedView this[string name] 
        {
            get 
            {
                if (TryGet(name, out IXModelView view))
                {
                    return (SwNamedView)view;
                }
                else 
                {
                    throw new Exception("Failed to find the named view");

                }
            }
        }

        public ISwStandardView this[StandardViewType_e type]
            => new SwStandardView(null, m_Doc, m_App, type); //TODO: move the view creation to SwObject.FromDispatch

        public void AddRange(IEnumerable<IXModelView> ents)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<IXModelView> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void RemoveRange(IEnumerable<IXModelView> ents)
        {
            throw new NotImplementedException();
        }

        public bool TryGet(string name, out IXModelView ent)
        {
            var viewNames = m_Doc.Model.GetModelViewNames() as string[];

            if (viewNames.Contains(name, StringComparer.CurrentCultureIgnoreCase))
            {
                //TODO: move the view creation to SwObject.FromDispatch
                ent = new SwNamedView(null, m_Doc, m_App, name);
                return true;
            }
            else 
            {
                ent = null;
                return false;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
