using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad.Toolkit.Attributes
{
    /// <summary>
    /// Attribute which marks the revision number in the file in the major version of the application
    /// </summary>
    /// <remarks>Used in <see cref="Services.IVersionMapper{TVersion}"/></remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public class FileRevisionAttribute : Attribute
    {
        /// <summary>
        /// File revision
        /// </summary>
        public int Revision { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="revision">File revision</param>
        public FileRevisionAttribute(int revision)
        {
            Revision = revision;
        }
    }
}
