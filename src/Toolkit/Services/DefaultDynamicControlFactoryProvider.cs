using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Services;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.Toolkit.Services
{
    /// <summary>
    /// Default control factory provider
    /// </summary>
    public class DefaultDynamicControlFactoryProvider : IDynamicControlFactoryProvider
    {
        private readonly Dictionary<Type, IDynamicControlFactory> m_Factories;

        /// <summary>
        /// Default constructor
        /// </summary>
        public DefaultDynamicControlFactoryProvider() 
        {
            m_Factories = new Dictionary<Type, IDynamicControlFactory>();
        }

        /// <inheritdoc/>
        public IDynamicControlFactory Provide(IXPropertyPage page, Type ctrlFactType, object tag)
        {
            if (!m_Factories.TryGetValue(ctrlFactType, out var fact)) 
            {
                fact = (IDynamicControlFactory)Activator.CreateInstance(ctrlFactType);
                m_Factories.Add(ctrlFactType, fact);
            }

            return fact;
        }
    }
}
