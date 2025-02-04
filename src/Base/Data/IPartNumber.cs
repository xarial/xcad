using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents.Enums;

namespace Xarial.XCad.Data
{
    /// <summary>
    /// Represents the part number
    /// </summary>
    public interface IPartNumber
    {
        /// <summary>
        /// Type of part number
        /// </summary>
        PartNumberSourceType_e Type { get; set; }

        /// <summary>
        /// Value of part number
        /// </summary>
        string Value { get; set; }
    }
}
