using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents;
using Xarial.XCad.Base;
using SolidWorks.Interop.swdocumentmgr;
using System.Linq;

namespace Xarial.XCad.SwDocumentManager.Documents
{
    public interface ISwDmComponentCollection : IXComponentRepository 
    {
    }

    internal class SwDmComponentCollection : ISwDmComponentCollection
    {
        #region Not Supported

        public void AddRange(IEnumerable<IXComponent> ents)
            => throw new NotSupportedException();

        public void RemoveRange(IEnumerable<IXComponent> ents)
            => throw new NotSupportedException();

        #endregion

        private readonly ISwDmConfiguration m_Conf;
        private readonly SwDmAssembly m_ParentAssm;

        internal SwDmComponentCollection(SwDmAssembly parentAssm, ISwDmConfiguration conf) 
        {
            m_ParentAssm = parentAssm;
            m_Conf = conf;
        }

        public IXComponent this[string name] => this.Get(name);

        public int Count => ((ISwDMDocument8)m_ParentAssm).GetComponentCount();

        private IEnumerable<ISwDMComponent> IterateDmComponents() 
        {
            return (((ISwDMConfiguration2)m_Conf.Configuration)
                .GetComponents() as object[] ?? new object[0])
                .Cast<ISwDMComponent>();
        }

        public IEnumerator<IXComponent> GetEnumerator()
            => IterateDmComponents()
            .Select(c => CreateComponentInstance(c))
            .GetEnumerator();

        protected virtual SwDmComponent CreateComponentInstance(ISwDMComponent dmComp) 
        {
            var comp = SwDmObjectFactory.FromDispatch<SwDmComponent>(dmComp, m_ParentAssm);
            return comp;
        }

        public bool TryGet(string name, out IXComponent ent)
        {
            var comp = IterateDmComponents().FirstOrDefault(c => string.Equals(((ISwDMComponent7)c).Name2,
                name, StringComparison.CurrentCultureIgnoreCase));

            if (comp != null)
            {
                ent = CreateComponentInstance(comp);
                return true;
            }
            else
            {
                ent = null;
                return false;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
