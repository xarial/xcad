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
    public class SwModelViewsCollection : IXViewRepository
    {
        IXStandardView IXViewRepository.this[StandardViewType_e type]=>null;
        IXView IXRepository<IXView>.this[string name] => this[name];
        IXView IXViewRepository.Active => Active;

        private readonly SwDocument3D m_Doc;
        private readonly IMathUtility m_MathUtils;

        public SwModelViewsCollection(SwDocument3D doc, IMathUtility mathUtils) 
        {
            m_Doc = doc;
            m_MathUtils = mathUtils;
        }

        public int Count => throw new NotImplementedException();

        //TODO: move the view creation to SwObject.FromDispatch
        public SwModelView Active => new SwModelView(m_Doc.Model, m_Doc.Model.IActiveView, m_MathUtils);
        
        public SwNamedView this[string name] 
        {
            get 
            {
                if (TryGet(name, out IXView view))
                {
                    return (SwNamedView)view;
                }
                else 
                {
                    throw new Exception("Failed to find the named view");

                }
            }
        }

        public SwStandardView this[StandardViewType_e type]
            => new SwStandardView(m_Doc.Model, null, m_MathUtils, type); //TODO: move the view creation to SwObject.FromDispatch

        public void AddRange(IEnumerable<IXView> ents)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<IXView> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void RemoveRange(IEnumerable<IXView> ents)
        {
            throw new NotImplementedException();
        }

        public bool TryGet(string name, out IXView ent)
        {
            var viewNames = m_Doc.Model.GetModelViewNames() as string[];

            if (viewNames.Contains(name, StringComparer.CurrentCultureIgnoreCase))
            {
                //TODO: move the view creation to SwObject.FromDispatch
                ent = new SwNamedView(m_Doc.Model, null, m_MathUtils, name);
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
