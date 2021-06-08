//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Data;

namespace Xarial.XCad.SwDocumentManager.Data
{
    public interface ISwDmCustomPropertiesCollection : IXPropertyRepository 
    {
    }

    internal abstract class SwDmCustomPropertiesCollection : ISwDmCustomPropertiesCollection
    {
        public IXProperty this[string name] => this.Get(name);

        public void AddRange(IEnumerable<IXProperty> ents)
        {
            foreach (var prp in ents) 
            {
                prp.Commit();
            }
        }

        public IXProperty PreCreate() => CreatePropertyInstance("", false);

        public void RemoveRange(IEnumerable<IXProperty> ents)
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

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public abstract int Count { get; }
        public abstract IEnumerator<IXProperty> GetEnumerator();

        protected abstract bool Exists(string name);
        protected abstract ISwDmCustomProperty CreatePropertyInstance(string name, bool isCreated);
    }
}
