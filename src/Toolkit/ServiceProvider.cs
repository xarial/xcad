//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;

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
            var svcFact = m_Services[serviceType];
            return svcFact.Invoke();
        }
    }

    public static class IServiceProviderExtension 
    {
        public static TService GetService<TService>(this IServiceProvider provider) 
            => (TService)provider.GetService(typeof(TService));
    }
}
