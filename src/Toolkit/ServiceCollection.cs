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
        private readonly Dictionary<Type, Func<object>> m_Services;

        public IReadOnlyDictionary<Type, Func<object>> Services => m_Services;

        public ServiceCollection() : this(new Dictionary<Type, Func<object>>()) 
        {
        }

        private ServiceCollection(Dictionary<Type, Func<object>> svcs)
        {
            m_Services = svcs;
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

        public IXServiceCollection Clone()
            => new ServiceCollection(m_Services.ToDictionary(x => x.Key, x => x.Value));
    }
}
