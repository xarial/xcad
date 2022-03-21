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
    internal class ServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, Func<object>> m_Services;

        internal ServiceProvider(Dictionary<Type, Func<object>> services)
        {
            m_Services = services;
        }

        public object GetService(Type serviceType)
        {
            if (m_Services.TryGetValue(serviceType, out var svcFact))
            {
                return svcFact.Invoke();
            }
            else
            {
                throw new ServiceNotRegisteredException(serviceType);
            }
        }
    }

    public static class IServiceProviderExtension 
    {
        public static TService GetService<TService>(this IServiceProvider provider) 
            => (TService)provider.GetService(typeof(TService));
    }
}
