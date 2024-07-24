//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Data;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SwDocumentManager.Data
{
    public interface ISwDmCustomPropertiesCollection : IXPropertyRepository 
    {
    }

    internal abstract class SwDmCustomPropertiesCollection : ISwDmCustomPropertiesCollection
    {
        public IXProperty this[string name] => m_RepoHelper.Get(name);

        public void AddRange(IEnumerable<IXProperty> ents, CancellationToken cancellationToken)
            => m_RepoHelper.AddRange(ents, cancellationToken);

        public T PreCreate<T>() where T : IXProperty => (T)CreatePropertyInstance("", false);

        public void RemoveRange(IEnumerable<IXProperty> ents, CancellationToken cancellationToken)
        {
            foreach (SwDmCustomProperty prp in ents) 
            {
                prp.Delete();
            }
        }

        public bool TryGet(string name, out IXProperty ent)
        {
            if (Exists(name))
            {
                ent = CreatePropertyInstance(name, true);
                return true;
            }
            else
            {
                ent = null;
                return false;
            }
        }

        private readonly RepositoryHelper<IXProperty> m_RepoHelper;

        protected SwDmCustomPropertiesCollection() 
        {
            m_RepoHelper = new RepositoryHelper<IXProperty>(this);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) 
            => m_RepoHelper.FilterDefault(this, filters, reverseOrder);

        public abstract int Count { get; }
        public abstract IEnumerator<IXProperty> GetEnumerator();

        protected abstract bool Exists(string name);
        protected abstract ISwDmCustomProperty CreatePropertyInstance(string name, bool isCreated);
    }
}
