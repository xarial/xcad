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

    /// <summary>
    /// Base class for the sync and asycn element creators
    /// </summary>
    /// <typeparam name="TElem">Type of element</typeparam>
    public abstract class ElementCreatorBase<TElem> : IElementCreatorBase<TElem>
    {
        /// <inheritdoc/>
        public bool IsCreated { get; protected set; }

        /// <summary>
        /// Internal instance of the actual element
        /// </summary>
        private TElem ElementInstance 
        {
            get 
            {
                if (m_IsLazyElemInstance)
                {
                    return m_LazyElemInstance.Value;
                }
                else 
                {
                    return m_ElemInstance;
                }
            }
        }

        /// <summary>
        /// Post element created Callback function (optional)
        /// </summary>
        protected readonly Action<TElem, CancellationToken> m_PostCreator;

        /// <inheritdoc/>
        public CachedProperties CachedProperties { get; }

        private bool m_IsLazyElemInstance;
        private TElem m_ElemInstance;
        private Lazy<TElem> m_LazyElemInstance;

        public ElementCreatorBase(Action<TElem, CancellationToken> postCreator, TElem elem, bool created = false) : this(postCreator)
        {
            IsCreated = created;
            m_ElemInstance = elem;

            m_IsLazyElemInstance = false;
        }

        /// <summary>
        /// Constructor for the lazy instance of the element
        /// </summary>
        /// <param name="lazyElem">Lazy element instance</param>
        /// <remarks>This constructor is for the created element</remarks>
        public ElementCreatorBase(Lazy<TElem> lazyElem, Action<TElem, CancellationToken> postCreator) : this(postCreator)
        {
            if (lazyElem == null) 
            {
                throw new ArgumentNullException(nameof(lazyElem));
            }

            m_LazyElemInstance = lazyElem;
            m_IsLazyElemInstance = true;
            IsCreated = true;
        }

        private ElementCreatorBase(Action<TElem, CancellationToken> postCreator) 
        {
            m_PostCreator = postCreator;
            CachedProperties = new CachedProperties();
        }

        /// <inheritdoc/>
        public TElem Element
        {
            get
            {
                if (IsCreated)
                {
                    return ElementInstance;
                }
                else
                {
                    throw new NonCommittedElementAccessException();
                }
            }
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public void Set(TElem elem)
        {
            m_ElemInstance = elem;
            m_LazyElemInstance = null;
            IsCreated = ElementInstance != null;
            m_IsLazyElemInstance = false;
        }
    }

    /// <summary>
    /// Implementation of synchronous element creator
    /// </summary>
    /// <typeparam name="TElem">Type of element</typeparam>
    public class ElementCreator<TElem> : ElementCreatorBase<TElem>, IElementCreator<TElem>
    {
        private readonly Func<CancellationToken, TElem> m_Factory;
        
        public ElementCreator(Func<CancellationToken, TElem> fact, TElem elem, bool created = false)
            : this(fact, null, elem, created)
        {
        }

        public ElementCreator(Func<CancellationToken, TElem> fact, Action<TElem, CancellationToken> postCreator, TElem elem, bool created = false) 
            : base(postCreator, elem, created)
        {
            m_Factory = fact;
        }

        public ElementCreator(Lazy<TElem> lazyElem, Func<CancellationToken, TElem> fact) : base(lazyElem, null)
        {
            m_Factory = fact;
        }

        /// <inheritdoc/>
        public TElem Create(CancellationToken cancellationToken)
        {
            if (!IsCreated)
            {
                var inst = m_Factory.Invoke(cancellationToken);
                Set(inst);
                m_PostCreator?.Invoke(inst, cancellationToken);
                return inst;
            }
            else
            {
                throw new ElementAlreadyCommittedException();
            }
        }
    }

    /// <summary>
    /// Implementation of asynchronous element creator
    /// </summary>
    /// <typeparam name="TElem">Type of element</typeparam>
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
                var inst = await m_AsyncCreator.Invoke(cancellationToken);
                Set(inst);
                m_PostCreator?.Invoke(inst, cancellationToken);
                return inst;
            }
            else
            {
                throw new ElementAlreadyCommittedException();
            }
        }
    }
}