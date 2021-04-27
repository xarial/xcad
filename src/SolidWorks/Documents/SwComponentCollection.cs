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
using Xarial.XCad.Base;
using Xarial.XCad.Documents;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwComponentCollection : IXComponentRepository
    {
        new ISwComponent this[string name] { get; }
    }

    internal abstract class SwComponentCollection : ISwComponentCollection
    {
        IXComponent IXRepository<IXComponent>.this[string name] => this[name];

        public ISwComponent this[string name] => (SwComponent)this.Get(name);

        public bool TryGet(string name, out IXComponent ent)
        {
            var comp = m_Assm.Assembly.GetComponentByName(name);

            if (comp != null)
            {
                ent = SwObject.FromDispatch<SwComponent>(comp, m_Assm);
                return true;
            }
            else
            {
                ent = null;
                return false;
            }
        }

        public int Count
        {
            get 
            {
                if (m_Assm.IsCommitted)
                {
                    if (m_Assm.Model.IsOpenedViewOnly())
                    {
                        throw new Exception("Components count is inaccurate in Large Design Review assembly");
                    }

                    return GetChildrenCount();
                }
                else
                {
                    throw new Exception("Assembly is not committed");
                }
            }
        }

        public int TotalCount 
        {
            get 
            {
                if (m_Assm.IsCommitted)
                {
                    if (m_Assm.Model.IsOpenedViewOnly())
                    {
                        throw new Exception("Total components count is inaccurate in Large Design Review assembly");
                    }

                    return GetTotalChildrenCount();
                }
                else
                {
                    throw new Exception("Assembly is not committed");
                }
            }
        }

        private readonly ISwAssembly m_Assm;

        internal SwComponentCollection(ISwAssembly assm)
        {
            m_Assm = assm;
        }

        public void AddRange(IEnumerable<IXComponent> ents)
        {
            throw new NotImplementedException();
        }

        protected abstract IEnumerable<IComponent2> GetChildren();
        protected abstract int GetChildrenCount();
        protected abstract int GetTotalChildrenCount();

        public IEnumerator<IXComponent> GetEnumerator()
        {
            if (m_Assm.IsCommitted)
            {
                if (m_Assm.Model.IsOpenedViewOnly())
                {
                    throw new Exception("Components cannot be extracted for the Large Design Review assembly");
                }

                return (GetChildren() ?? new IComponent2[0])
                    .Select(c => SwSelObject.FromDispatch<SwComponent>(c, m_Assm)).GetEnumerator();
            }
            else 
            {
                throw new Exception("Assembly is not committed");
            }
        }

        public void RemoveRange(IEnumerable<IXComponent> ents)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
