//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.Toolkit.PageBuilder.Services
{
    /// <summary>
    /// Service for creating <see cref="IDynamicControlFactory"/>
    /// </summary>
    public interface IDynamicControlFactoryProvider
    {
        /// <summary>
        /// Provides dynamic control factory
        /// </summary>
        /// <param name="page">Calling page</param>
        /// <param name="ctrlFactType">Type of control factory specified in <see cref="DynamicControlsAttribute.FactoryType"/></param>
        /// <param name="tag">Tag of control factory specified in <see cref="DynamicControlsAttribute.Tag"/></param>
        /// <returns>Dynamic control factory</returns>
        IDynamicControlFactory Provide(IXPropertyPage page, Type ctrlFactType, object tag);
    }

    /// <summary>
    /// Default control factory provider
    /// </summary>
    public class DynamicControlFactoryProvider : IDynamicControlFactoryProvider
    {
        private readonly IServiceProvider m_Services;

        /// <summary>
        /// Default constructor
        /// </summary>
        public DynamicControlFactoryProvider(IServiceProvider services)
        {
            m_Services = services;
        }

        /// <inheritdoc/>
        public IDynamicControlFactory Provide(IXPropertyPage page, Type ctrlFactType, object tag)
        {
            if (m_Services.TryGetService(ctrlFactType, out var svc))
            {
                return (IDynamicControlFactory)svc;
            }
            else 
            {
                return (IDynamicControlFactory)Activator.CreateInstance(ctrlFactType);
            }
        }
    }
}
