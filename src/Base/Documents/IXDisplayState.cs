using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents display state
    /// </summary>
    public interface IXDisplayState : IXObject, IHasName, IXRepository<IXAppearance>
    {
        /// <summary>
        /// Gets appearance for the specified entities in this display state
        /// </summary>
        /// <param name="objs">Objects to get appearance for</param>
        /// <returns>Appearance</returns>
        IXAppearance this[IHasColor[] objs] { get; }
    }
}
