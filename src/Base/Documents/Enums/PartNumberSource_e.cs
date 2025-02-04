using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Data;

namespace Xarial.XCad.Documents.Enums
{
    /// <summary>
    /// Source of the <see cref="IPartNumber"/>
    /// </summary>
    public enum PartNumberSourceType_e
    {
        /// <summary>
        /// Name of the document
        /// </summary>
        DocumentName,

        /// <summary>
        /// Name of the configuration
        /// </summary>
        ConfigurationName,

        /// <summary>
        /// Name, derived form parent
        /// </summary>
        ParentName,

        /// <summary>
        /// User specified value
        /// </summary>
        Custom
    }
}
