//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
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
    public interface IElementCreatorBase<TElem>
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
    }

    /// <inheritdoc/>
    public interface IElementCreator<TElem> : IElementCreatorBase<TElem>
    {
        /// <summary>
        /// Creates element from the template
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="ElementAlreadyCommittedException"/>
        /// <returns>Instance of the specific element</returns>
        TElem Create(CancellationToken cancellationToken);
    }

    /// <inheritdoc/>
    public interface IAsyncElementCreator<TElem> : IElementCreatorBase<TElem>
    {
        /// <summary>
        /// Async creates element from the template
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="ElementAlreadyCommittedException"/>
        /// <returns>Instance of the specific element</returns>
        Task<TElem> CreateAsync(CancellationToken cancellationToken);
    }

    public abstract class ElementCreatorBase<TElem> : IElementCreatorBase<TElem>
    {
        public bool IsCreated { get; protected set; }

        protected TElem m_Element;

        protected readonly Action<TElem, CancellationToken> m_PostCreator;

        public CachedProperties CachedProperties { get; }

        public ElementCreatorBase(Action<TElem, CancellationToken> postCreator, TElem elem, bool created = false)
        {
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
    }

    public class ElementCreator<TElem> : ElementCreatorBase<TElem>, IElementCreator<TElem>
    {
        private readonly Func<CancellationToken, TElem> m_Creator;
        
        public ElementCreator(Func<CancellationToken, TElem> creator, TElem elem, bool created = false)
            : this(creator, null, elem, created)
        {
        }

        public ElementCreator(Func<CancellationToken, TElem> creator, Action<TElem, CancellationToken> postCreator, TElem elem, bool created = false) 
            : base(postCreator, elem, created)
        {
            m_Creator = creator;
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

    public class AsyncElementCreator<TElem> : ElementCreatorBase<TElem>, IAsyncElementCreator<TElem>
    {
        private readonly Func<CancellationToken, Task<TElem>> m_AsyncCreator;

        public AsyncElementCreator(Func<CancellationToken, Task<TElem>> asyncCreator, TElem elem, bool created = false)
            : this(asyncCreator, null, elem, created)
        {
        }

        public AsyncElementCreator(Func<CancellationToken, Task<TElem>> asyncCreator, Action<TElem, CancellationToken> postCreator, TElem elem, bool created = false)
            : base(postCreator, elem, created)
        {
            m_AsyncCreator = asyncCreator;
        }

        public async Task<TElem> CreateAsync(CancellationToken cancellationToken)
        {
            if (!IsCreated)
            {
                m_Element = await m_AsyncCreator.Invoke(cancellationToken);
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