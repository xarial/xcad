using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.UI.PropertyPage.Attributes;

namespace Xarial.XCad.Toolkit.PageBuilder.Exceptions
{
    /// <summary>
    /// Exception indicates that duplicate <see cref="ControlTagAttribute"/> is used
    /// </summary>
    public class DuplicateTagException : Exception
    {
        /// <summary>
        /// Tag
        /// </summary>
        public object Tag { get; }
        
        internal DuplicateTagException(object tag) : base($"Duplicate control tag '{tag}'") 
        {
            Tag = tag;
        }
    }
}
