using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Annotations
{
    /// <summary>
    /// Indicates that this object can have dimensions
    /// </summary>
    public interface IDimensionable
    {
        /// <summary>
        /// Dimensions repository
        /// </summary>
        IXDimensionRepository Dimensions { get; }
    }
}
