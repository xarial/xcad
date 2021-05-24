//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
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
    /// Represents the collection of drawing views
    /// </summary>
    public interface IXDrawingViewRepository : IXRepository<IXDrawingView>
    {
        /// <summary>
        /// Precreates a specific drawing view
        /// </summary>
        /// <typeparam name="TDrawingView">Type of drawing view to precreate</typeparam>
        /// <returns>Drawing view template</returns>
        TDrawingView PreCreate<TDrawingView>()
            where TDrawingView : class, IXDrawingView;
    }
}
