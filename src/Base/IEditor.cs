using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad
{
    /// <summary>
    /// Represents the editor of the specific object
    /// </summary>
    public interface IEditor<out TEnt> : IDisposable
        where TEnt : IXObject
    {
        /// <summary>
        /// Object being edited
        /// </summary>
        TEnt Target { get; }

        /// <summary>
        /// True to cancel editing
        /// </summary>
        bool Cancel { get; set; }
    }
}
