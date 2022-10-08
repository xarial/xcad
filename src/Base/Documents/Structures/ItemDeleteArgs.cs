using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad.Documents.Structures
{
    /// <summary>
    /// Argument of the item deletion event
    /// </summary>
    public class ItemDeleteArgs
    {
        /// <summary>
        /// Specifies if the deleting operation needs to be cancelled
        /// </summary>
        public bool Cancel { get; set; }
    }
}
