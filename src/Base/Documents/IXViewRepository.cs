using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents collection of views in the <see cref="IXDocument3D"/>
    /// </summary>
    public interface IXViewRepository : IXRepository<IXView>
    {
        /// <summary>
        /// Gets active view
        /// </summary>
        IXView Active { get; } //TODO: implement set

        /// <summary>
        /// Returns standard view by type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IXStandardView this[StandardViewType_e type] { get; }
    }
}
