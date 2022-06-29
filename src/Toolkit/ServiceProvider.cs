//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
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
                throw new ServiceNotRegisteredException(serviceType);
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

    public static class IServiceProviderExtension 
    {
        public static TService GetService<TService>(this IServiceProvider provider) 
            => (TService)provider.GetService(typeof(TService));
    }
}
