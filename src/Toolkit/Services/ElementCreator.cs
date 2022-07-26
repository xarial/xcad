//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Toolkit.Exceptions;

namespace Xarial.XCad.Services
{
    /// <summary>
    /// Manages cached properties of <see cref="Base.IXTransaction"/>
    /// </summary>
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

        public bool Has<T>([CallerMemberName] string prpName = "") 
            => m_CachedProperties.ContainsKey(prpName);

        internal CachedProperties() 
        {
            m_CachedProperties = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Helper class to manage the lifecycle of <see cref="Base.IXTransaction"/>
    /// </summary>
    /// <typeparam name="TElem">Type of the underlying object</typeparam>
    public interface IElementCreator<TElem>
    {
        /// <summary>
        /// True if this element is created or false if it is a template
        /// </summary>
        bool IsCreated { get; }

        /// <summary>
        /// Provides access to manage cached properties
        /// </summary>
        CachedProperties CachedProperties { get; }

        /// <summary>
        /// Pointer to the specific element
        /// </summary>
        /// <exception cref="NonCommittedElementAccessException"/>
        TElem Element { get; }

        /// <summary>
        /// Sets the element to the specified value and updates the state
        /// </summary>
        /// <param name="elem">Element or null</param>
        void Set(TElem elem);

        /// <summary>
        /// Forcibly inits the element instance
        /// </summary>
        /// <param name="elem">Element to set</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="ElementAlreadyCommittedException"/>
        /// <remarks> Element must not be null and not commited. This method will also call the post creation handler</remarks>
        void Init(TElem elem, CancellationToken cancellationToken);

        /// <summary>
        /// Creates element from the template
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="ElementAlreadyCommittedException"/>
        /// <returns>Instance of the specific element</returns>
        TElem Create(CancellationToken cancellationToken);
    }

    public class ElementCreator<TElem> : IElementCreator<TElem>
    {
        public bool IsCreated { get; private set; }

        private TElem m_Element;

        private readonly Func<CancellationToken, TElem> m_Creator;
        private readonly Action<TElem, CancellationToken> m_PostCreator;

        public CachedProperties CachedProperties { get; }

        public ElementCreator(Func<CancellationToken, TElem> creator, TElem elem, bool created = false)
            : this(creator, null, elem, created)
        {
        }

        public ElementCreator(Func<CancellationToken, TElem> creator, Action<TElem, CancellationToken> postCreator, TElem elem, bool created = false)
        {
            m_Creator = creator;
            m_PostCreator = postCreator;

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

        public void Init(TElem elem, CancellationToken cancellationToken) 
        {
            if (!IsCreated)
            {
                if (elem == null)
                {
                    throw new ArgumentNullException(nameof(elem));
                }

                Set(elem);
                
                m_PostCreator?.Invoke(elem, cancellationToken);
            }
            else
            {
                throw new ElementAlreadyCommittedException();
            }
        }

        public void Set(TElem elem)
        {
            m_Element = elem;
            IsCreated = m_Element != null;
        }

        public TElem Create(CancellationToken cancellationToken)
        {
            if (!IsCreated)
            {
                m_Element = m_Creator.Invoke(cancellationToken);
                IsCreated = true;
                m_PostCreator?.Invoke(m_Element, cancellationToken);
                return m_Element;
            }
            else
            {
                throw new ElementAlreadyCommittedException();
            }
        }
    }
}