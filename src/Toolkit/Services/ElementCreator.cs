//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Xarial.XCad.Toolkit.Exceptions;

namespace Xarial.XCad.Services
{
    public class CachedProperties 
    {
        private readonly Dictionary<string, object> m_CachedProperties;

        public T Get<T>([CallerMemberName]string prpName = "")
        {
            object val;

            if (!m_CachedProperties.TryGetValue(prpName, out val))
            {
                val = default(T);
                m_CachedProperties.Add(prpName, val);
            }

            return (T)val;
        }

        public void Set<T>(T val, [CallerMemberName]string prpName = "")
        {
            m_CachedProperties[prpName] = val;
        }

        internal CachedProperties() 
        {
            m_CachedProperties = new Dictionary<string, object>();
        }
    }

    public class ElementCreator<TElem>
    {
        public event Action<TElem> Creating;

        public bool IsCreated { get; private set; }

        private TElem m_Element;

        private readonly Func<CancellationToken, TElem> m_Creator;

        public CachedProperties CachedProperties { get; }

        public ElementCreator(Func<CancellationToken,TElem> creator, TElem elem, bool created = false)
        {
            m_Creator = creator;
            IsCreated = created;
            m_Element = elem;

            CachedProperties = new CachedProperties();
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

        public void Create(CancellationToken cancellationToken)
        {
            if (!IsCreated)
            {
                m_Element = m_Creator.Invoke(cancellationToken);
                Creating?.Invoke(m_Element);
                IsCreated = true;
            }
            else
            {
                throw new ElementAlreadyCommittedException();
            }
        }
    }
}