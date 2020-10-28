//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.Toolkit.Exceptions;

namespace Xarial.XCad.Services
{
    public class ElementCreator<TElem>
    {
        public bool IsCreated { get; private set; }

        private TElem m_Element;

        private readonly Func<TElem> m_Creator;

        public ElementCreator(Func<TElem> creator, TElem elem, bool created = false)
        {
            m_Creator = creator;
            IsCreated = created;
            m_Element = elem;
        }

        public TElem Element
        {
            get
            {
                if (IsCreated)
                {
                    return m_Element;
                }
                else
                {
                    throw new NonCommittedElementAccessException();
                }
            }
        }

        public void Create()
        {
            if (!IsCreated)
            {
                m_Element = m_Creator.Invoke();
                IsCreated = true;
            }
            else
            {
                throw new ElementAlreadyCommittedException();
            }
        }
    }
}