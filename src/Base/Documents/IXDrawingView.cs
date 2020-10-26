using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents the drawing view on <see cref="IXSheet"/>
    /// </summary>
    public interface IXDrawingView : IXSelObject, IXObjectContainer
    {
        /// <summary>
        /// Name of this drawing view
        /// </summary>
        string Name { get; set; }
    }
}
