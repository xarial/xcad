//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xarial.XCad.Toolkit
{
    public class ServiceCollection : IXServiceCollection
    {
        internal class ServiceInfo 
        {
            internal Func<object> Factory { get; }
            internal ServiceLifetimeScope_e Lifetime { get; }

            internal ServiceInfo(Func<object> factory, ServiceLifetimeScope_e lifetime)
            {
                Factory = factory;
                Lifetime = lifetime;
            }
        }

        private readonly Dictionary<Type, ServiceInfo> m_Services;

        private bool m_IsProviderCreated;

        public ServiceCollection() : this(new Dictionary<Type, ServiceInfo>()) 
        {
        }

        private ServiceCollection(Dictionary<Type, ServiceInfo> svcs)
        {
            m_Services = svcs;
            m_IsProviderCreated = false;
        }

        public void Add(Type svcType, Func<object> svcFactory, ServiceLifetimeScope_e lifetime = ServiceLifetimeScope_e.Singleton, bool replace = true)
        {
            if (replace || !m_Services.ContainsKey(svcType))
            {
                m_Services[svcType] = new ServiceInfo(svcFactory, lifetime);
            }
        }

        public IServiceProvider CreateProvider()
        {
            if (!m_IsProviderCreated)
            {
                m_IsProviderCreated = true;
                return new ServiceProvider(m_Services);
            }
            else 
            {
                throw new Exception("Provider is already created");
            }
        }

        public IXServiceCollection Clone()
            => new ServiceCollection(m_Services.ToDictionary(x => x.Key, x => new ServiceInfo(x.Value.Factory, x.Value.Lifetime)));
    }
}
