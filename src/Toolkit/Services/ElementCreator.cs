//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

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
                    throw new Exception("This is a template feature and has not been created yet. Commit this feature by adding to the feature collection");
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
                throw new Exception("Feature already created");
            }
        }
    }
}