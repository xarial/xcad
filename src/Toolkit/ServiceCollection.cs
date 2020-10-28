using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Toolkit
{
    public class ServiceCollection : IXServiceCollection
    {
        private readonly Dictionary<Type, Func<object>> m_Services;

        public ServiceCollection() 
        {
            m_Services = new Dictionary<Type, Func<object>>();
        }
        
        public void AddOrReplace(Type svcType, Func<object> svcFactory)
        {
            m_Services[svcType] = svcFactory;
        }

        public IServiceProvider CreateProvider()
        {
            var provider = new ServiceProvider(m_Services);
            return provider;
        }
    }
}
