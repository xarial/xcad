using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad.Exceptions
{
    /// <summary>
    /// Indicates error with the operation of the table row
    /// </summary>
    public class TableRowOperationException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public TableRowOperationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public TableRowOperationException(string msg, Exception inner) : base(msg, inner)
        {
        }
    }
}
