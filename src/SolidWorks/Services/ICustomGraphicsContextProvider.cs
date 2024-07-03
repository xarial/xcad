//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Documents.Graphics;
using Xarial.XCad.Toolkit.Graphics;

namespace Xarial.XCad.SolidWorks.Services
{
    /// <summary>
    /// Service to provide custom graphics context
    /// </summary>
    public interface ICustomGraphicsContextProvider
    {
        /// <summary>
        /// Provides custom graphics context for the specified model view
        /// </summary>
        /// <param name="view">Model view</param>
        /// <returns>Custom graphics context</returns>
        IXCustomGraphicsContext ProvideContext(IXModelView view);
    }

    internal class OglCustomGraphicsContextProvider : ICustomGraphicsContextProvider
    {
        public IXCustomGraphicsContext ProvideContext(IXModelView view)
            => new OglCustomGraphicsContext((ISwModelView)view);
    }
}
