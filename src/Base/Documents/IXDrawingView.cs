using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents the drawing view on <see cref="IXSheet"/>
    /// </summary>
    public interface IXDrawingView : IXSelObject, IXObjectContainer, IXTransaction
    {
        /// <summary>
        /// Name of this drawing view
        /// </summary>
        string Name { get; set; }
    }
}
