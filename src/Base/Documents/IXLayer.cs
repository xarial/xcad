using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents layer for the entitites
    /// </summary>
    /// <remarks>Entities which support layer are implementing <see cref="IHasLayer"/></remarks>
    public interface IXLayer : IXTransaction, IXObject, IHasColor
    {
        /// <summary>
        /// Name of the layer
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Visibility of the layer
        /// </summary>
        bool Visible { get; set; }
    }
}
