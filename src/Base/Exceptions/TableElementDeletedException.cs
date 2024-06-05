using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad.Exceptions
{
    /// <summary>
    /// Indicates that this row or column in the table is deleted
    /// </summary>
    public class TableElementDeletedException : Exception
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public TableElementDeletedException() : base("Table element is deleted")
        {
        }
    }
}
