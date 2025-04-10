using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents;

namespace Xarial.XCad.SolidWorks.Documents.Exceptions
{
    /// <summary>
    /// Indicates that material cannot be removed for the configuration
    /// </summary>
    public class ConfigurationMaterialRemoveException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ConfigurationMaterialRemoveException() : base($"Material cannot be removed for the configuration. Remove material on part level instead via {nameof(IXPart)}::{nameof(IXPart.Material)}")
        {
        }
    }
}
