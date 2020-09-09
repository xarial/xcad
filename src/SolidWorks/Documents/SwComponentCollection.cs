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
    public class SwComponentCollection : IXComponentRepository
    {
        IXComponent IXRepository<IXComponent>.this[string name] => this[name];

        public SwComponent this[string name] 
        {
            get 
            {
                if (TryGet(name, out IXComponent comp))
                {
                    return (SwComponent)comp;
                }
                else 
                {
                    throw new KeyNotFoundException($"Failed to get the component '{name}'");
                }
            }
        }

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

        public int Count => m_Assm.Assembly.GetComponentCount(false);

        private readonly SwAssembly m_Assm;

        private readonly IComponent2 m_Parent;

        internal SwComponentCollection(SwAssembly assm, IComponent2 parent) 
        {
            m_Assm = assm;
            m_Parent = parent;
        }

        public void AddRange(IEnumerable<IXComponent> ents)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<IXComponent> GetEnumerator()
        {
            return new SwComponentEnumerator(m_Assm, m_Parent);
        }

        public void RemoveRange(IEnumerable<IXComponent> ents)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal class SwComponentEnumerator : IEnumerator<IXComponent>
    {
        public IXComponent Current => SwSelObject.FromDispatch<SwComponent>(m_CurComp, m_Assm);

        object IEnumerator.Current => Current;

        private readonly SwAssembly m_Assm;

        private IComponent2 m_CurComp;

        private readonly IComponent2 m_Parent;
        private IComponent2[] m_Children;

        private int m_CurChildIndex;
        
        internal SwComponentEnumerator(SwAssembly assm, IComponent2 parent)
        {
            m_CurComp = null;
            m_Assm = assm;
            m_Parent = parent;
            Reset();
        }

        public bool MoveNext()
        {
            if (m_CurChildIndex == -1)
            {
                m_Children = (m_Parent.GetChildren() as object[])?.Cast<IComponent2>().ToArray();

                if (m_Children == null) 
                {
                    m_Children = new IComponent2[0];
                }
            }

            m_CurChildIndex++;

            if (m_CurChildIndex < m_Children.Length)
            {
                m_CurComp = m_Children[m_CurChildIndex];
                return true;
            }
            else 
            {
                return false;
            }
        }

        public void Reset()
        {
            m_CurComp = null;
            m_CurChildIndex = -1;
            m_Children = null;
        }

        public void Dispose()
        {
        }
    }
}
