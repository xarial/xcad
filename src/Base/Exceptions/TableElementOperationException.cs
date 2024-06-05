using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad.Exceptions
{
    /// <summary>
    /// Indicates error with the operation of the table row or column
    /// </summary>
    public class TableElementOperationException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public TableElementOperationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public TableElementOperationException(string msg, Exception inner) : base(msg, inner)
        {
        }
    }
}
