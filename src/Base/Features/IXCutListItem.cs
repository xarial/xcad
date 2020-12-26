using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Data;
using Xarial.XCad.Geometry;

namespace Xarial.XCad.Features
{
    /// <summary>
    /// Represents the cut-list item feature
    /// </summary>
    public interface IXCutListItem : IXFeature, IPropertiesOwner
    {
        /// <summary>
        /// Bodies of this cut-list item
        /// </summary>
        IXSolidBody[] Bodies { get; }
    }
}
