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
using Xarial.XCad.Geometry;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public interface ISwBodyCollection : IXBodyRepository 
    {
    }

    internal abstract class SwBodyCollection : ISwBodyCollection
    {
        private readonly ISwDocument m_RootDoc;

        internal SwBodyCollection(ISwDocument rootDoc)
        {
            m_RootDoc = rootDoc;
        }

        public IXBody this[string name]
        {
            get 
            {
                if (!TryGet(name, out IXBody body)) 
                {
                    throw new Exception("Body with specified name is not found");
                }

                return body;
            }
        }

        public int Count => GetBodies().Count();

        public void AddRange(IEnumerable<IXBody> ents)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<IXBody> GetEnumerator() => GetBodies().GetEnumerator();

        public void RemoveRange(IEnumerable<IXBody> ents)
        {
            throw new NotImplementedException();
        }

        public bool TryGet(string name, out IXBody ent)
        {
            ent = GetBodies().FirstOrDefault(
                b => string.Equals(b.Name, name, StringComparison.CurrentCultureIgnoreCase));

            return ent != null;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private IEnumerable<SwBody> GetBodies() 
        {
            var bodies = GetSwBodies();

            if (bodies != null)
            {
                return bodies.Select(b => SwSelObject.FromDispatch<SwBody>(b, m_RootDoc));
            }
            else 
            {
                return Enumerable.Empty<SwBody>();
            }
        }

        protected abstract IEnumerable<IBody2> GetSwBodies();
    }
}
