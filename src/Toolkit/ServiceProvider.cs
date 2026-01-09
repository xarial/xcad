//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using Xarial.XCad.Toolkit.Exceptions;

namespace Xarial.XCad.Toolkit
{
    internal class ServiceProvider : IServiceProvider, IDisposable
    {
        private interface IServiceCreator : IDisposable
        {
            object CreateService();
        }

        private class TransientService : IServiceCreator
        {
            private readonly Func<object> m_Factory;

            internal TransientService(Func<object> factory) 
            {
                m_Factory = factory;
            }

            public object CreateService() => m_Factory.Invoke();

            public void Dispose() 
            {
                //No disposing
            }
        }

        private class SingletonService : IServiceCreator
        {
            private readonly Lazy<object> m_FactoryLazy;

            internal SingletonService(Func<object> factory)
            {
                m_FactoryLazy = new Lazy<object>(factory);
            }

            public object CreateService() => m_FactoryLazy.Value;

            public void Dispose()
            {
                if (m_FactoryLazy.IsValueCreated) 
                {
                    if (m_FactoryLazy.Value is IDisposable) 
                    {
                        ((IDisposable)m_FactoryLazy.Value).Dispose();
                    }
                }
            }
        }

        private readonly Dictionary<Type, IServiceCreator> m_Services;
        private bool m_IsDisposed;

        internal ServiceProvider(Dictionary<Type, ServiceCollection.ServiceInfo> services)
        {
            m_Services = new Dictionary<Type, IServiceCreator>();

            foreach (var svc in services) 
            {
                switch (svc.Value.Lifetime) 
                {
                    case ServiceLifetimeScope_e.Singleton:
                        m_Services.Add(svc.Key, new SingletonService(svc.Value.Factory));
                        break;

                    case ServiceLifetimeScope_e.Transient:
                        m_Services.Add(svc.Key, new TransientService(svc.Value.Factory));
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
        }

        public object GetService(Type serviceType)
        {
            if (m_Services.TryGetValue(serviceType, out var svcFact))
            {
                return svcFact.CreateService();
            }
            else
            {
                return null;
            }
        }

        public void Dispose()
        {
            if (!m_IsDisposed)
            {
                foreach (var svc in m_Services.Values)
                {
                    svc.Dispose();
                }

                m_IsDisposed = true;
            }
        }
    }

    /// <summary>
    /// Additional methods of <see cref="IServiceProvider"/>
    /// </summary>
    public static class IServiceProviderExtension 
    {
        /// <summary>
        /// Gets specific service (if registered)
        /// </summary>
        /// <typeparam name="TService">Service type</typeparam>
        /// <param name="provider">Service provider</param>
        /// <returns>Instance of the service</returns>
        /// <exception cref="ServiceNotRegisteredException">Service is not registered</exception>
        public static TService GetService<TService>(this IServiceProvider provider)
        {
            if (provider.TryGetService<TService>(out var svc))
            {
                return svc;
            }
            else 
            {
                throw new ServiceNotRegisteredException(typeof(TService));
            }
        }

        /// <summary>
        /// Tries to get specific service
        /// </summary>
        /// <typeparam name="TService">Service type</typeparam>
        /// <param name="provider">Service provider</param>
        /// <param name="service">Instance of the service</param>
        /// <returns>True if service is registered, False if servcice is not registered</returns>
        public static bool TryGetService<TService>(this IServiceProvider provider, out TService service)
        {
            if (TryGetService(provider, typeof(TService), out var svc))
            {
                service = (TService)svc;
                return true;
            }
            else 
            {
                service = default;
                return false;
            }
        }

        /// <summary>
        /// Tries to get specific service
        /// </summary>
        /// <param name="serviceType">Service type</param>
        /// <param name="provider">Service provider</param>
        /// <param name="service">Instance of the service</param>
        /// <returns>True if service is registered, False if servcice is not registered</returns>
        public static bool TryGetService(this IServiceProvider provider, Type serviceType, out object service)
        {
            var svc = provider.GetService(serviceType);

            if (svc != null)
            {
                service = svc;
                return true;
            }
            else
            {
                service = null;
                return false;
            }
        }
    }
}
