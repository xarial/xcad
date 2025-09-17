using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.Services
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
}
