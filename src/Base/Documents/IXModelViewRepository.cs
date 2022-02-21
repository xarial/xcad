//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents collection of views in the <see cref="IXDocument"/>
    /// </summary>
    public interface IXModelViewRepository : IXRepository<IXModelView>
    {
        /// <summary>
        /// Gets active view
        /// </summary>
        IXModelView Active { get; }
    }

    /// <summary>
    /// Represents collection of views in the <see cref="IXDocument3D"/>
    /// </summary>
    public interface IXModelView3DRepository : IXModelViewRepository
    {
        /// <summary>
        /// Returns standard view by type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IXStandardView this[StandardViewType_e type] { get; }
    }
}
